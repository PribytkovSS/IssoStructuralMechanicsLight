using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using IssoStMechLight.Models;
using SkiaSharp;
using SkiaSharp.Views;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using IssoStMechLight.ViewModels;
using System.Threading.Tasks;

namespace IssoStMechLight.Views
{
    public enum EditorActions
    {
        None,
        NewLinearFirstPoint,
        NewLinearLastPoint,
        NewDimensionFirstNode,
        NewDimensionLastNode,
        NewDimensionLinePlace,
        NewDimensionVFirstNode,
        NewForce, NewDstLoad,
        EditNode,
        SelectFrame,
        CopyElements,
        ArrayElements,
        MirrorElements
    }
    // Класс для отрисовки модели и обработки графического ввода
    // Модель состоит из отдельных компонентов - стержней, узлов, опор, размеров
    // Отдельный компонент может находиться в состоянии редактирования, перетаскивания и т.п.
    // всё это визуализируется с помощью данного класса
    public class ModelViewSurface
    {
        private RodModelVM modelVM;
        private SKCanvasView RModelView;
        private SurfaceGrid Grid = null;
        
        // Масштаб отображения - сколько пикселей на один метр модели
        public float scaleFactor = 50;
        public float ScaleFactor
        { get
            { return scaleFactor; }
            set
            {
                float factor = (value - scaleFactor) / value;
                ModelViewFragment.X += (ViewWidth / scaleFactor) * factor / 2;
                ModelViewFragment.Y += (ViewHeight / scaleFactor) * factor / 2;
                scaleFactor = value;
                if (Grid != null)
                {
                    Grid.UpdateNeeded = true;
                    Invalidate();
                }
            }
        }

        public bool SelectionStarted = false;
        public bool MirroAxisDefined = false;
        public bool CopyBasePointDefined = false;
        public IssoPoint2D CopyBasePoint, CopyTargetPoint;
        public SKRect SelectionRect = new SKRect(0, 0, 0, 0);
        public List<IssoPoint2D> MirrorAxis = new List<IssoPoint2D>();

        internal RodModel RModel { get { return modelVM.model; } }
        
        private IssoPoint2D ModelViewFragment; // Координаты нижней левой точки прямоугольной области модели, отображаемой на экране
        private EditorActions EditorAction;
        private IssoPoint2D StartPt, EndPt;

        private ComponentBasic editedComp = null;

        private bool drawDeformedShape = false;
        private bool MovingView = false;
        public float ViewHeight { get { return RModelView.CanvasSize.Height; } }
        public float ViewWidth { get { return RModelView.CanvasSize.Width; } }
        public IssoPoint2D Origin { get { return ModelViewFragment; } }

        public EventHandler OnMirrorConfirm = null;

        private ComponentBasic EditedComp
        {
            get { return editedComp; }
            set
            {
                if (editedComp != null)
                    editedComp.CompState = ComponentState.csNormal;
                editedComp = value;
                if (editedComp != null)
                    editedComp.CompState = ComponentState.csEdited;
            }
        }

        internal void ZoomAll()
        {
            // Изменяем масштаб отображения так, чтобы вся модель помещалась на экране
            // Координаты нижней левой точки тоже меняем
            float[] bounds = RModel.GetBounds();            
            float w = Math.Max(bounds[2] - bounds[0], 1);
            float h = Math.Max(bounds[3] - bounds[1], 1);
            ScaleFactor = Math.Min(ViewHeight / h, ViewWidth / w) * 0.90f;
            ModelViewFragment.X = bounds[0] - ViewWidth * 0.05f / ScaleFactor;
            ModelViewFragment.Y = bounds[1] - ViewHeight * 0.05f / ScaleFactor;
            Invalidate();
        }

        
        internal bool DeleteSelectedObject()
        {
            ComponentBasic b = GetSelectedComponent();
            if (b.CompType == ComponentTypes.ctLinear)
            {
                ComponentNode n1 = ((ComponentLinear)b).StartNode;
                ComponentNode n2 = ((ComponentLinear)b).EndNode;
                // Выясним, есть ли ещё компоненты, соединённые с этими узлами
                // Если нет - удаляем и узлы
                if (RModel.NodeSingle(n1)) RModel.CompsList.Remove(n1);
                if (RModel.NodeSingle(n2)) RModel.CompsList.Remove(n2);
            }
            if (b.CompType == ComponentTypes.ctNode)
            {
                // При удалении узла возникают ситуации
                // 1. Удаление крайнего узла. В этом случае требуется подтверждение на удаление,
                // поскольку удаляться должен и заканчивающийся в этом узле линейный компонент
                // 2. Удаление узла, в котором сходятся несколько элементов
                //   2.1. Если элементов два - сращиваем элементы в один
                //          То есть, фактически удаляем один из них - тот, на котором нет распределённой нагрузки
                //          а оставшийся улиняем до конца удалённого элемента
                //   2.2 TODO: В общем, слишком муторно, отложу на потом.
            }
            RModel.CompsList.Remove(b);
            editedComp = null;
            Invalidate();
            return true;
        }

        public Action<object, EventArgs> OnComponentSelected { get; internal set; }
        public Action<object> OnActionNone { get; internal set; }
        public Action<EditorActions> OnVisualStates { get; internal set; }

        public RodModel Model { get { return RModel;  } }

        public bool DrawDeformedShape
        {
            get
            {
                return drawDeformedShape;
            }
            set
            {
                drawDeformedShape = value;
                Invalidate();
            }  
        }

        public ModelViewSurface(RodModelVM viewModel, SKCanvasView view)
        {           
            RModelView = view;
            modelVM = viewModel;            
            RModelView.PaintSurface += OnPaintSurface;
            RModelView.SizeChanged += OnViewSizeChanged;
            ModelViewFragment = new IssoPoint2D() { X = 0, Y = 0 };
            StartPt = new IssoPoint2D() { X = 0, Y = 0 };
            EndPt = new IssoPoint2D() { X = 0, Y = 0 };
            Grid = new SurfaceGrid();
        }

        private void OnViewSizeChanged(object sender, EventArgs e)
        {
            Grid.UpdateNeeded = true;
        }

        public ComponentBasic GetSelectedComponent()
        {
            return EditedComp;
        }

        public void OnSelectedComponentChanged()
        {
            RModelView.InvalidateSurface();
        }

        private void WheelZoom(int delta)
        {
            // Если значение delta положительное - масштаб увеличиваем (отдаляем)
            // в противном случае - приближаем
            ScaleFactor += delta * 0.1f;
        }

        public void Touch(SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.WheelChanged)
            {
                WheelZoom(e.WheelDelta);
                return;
            }

            if (e.MouseButton == SKMouseButton.Right)
            {
                CancelAction();
                return;
            }
            try
            {
                IssoPoint2D pt1 = IssoConvert.SkPointToIssoPoint2D(e.Location, ScaleFactor, ModelViewFragment, ViewHeight);
                // TODO: Обработать активность привязок
                pt1 = Grid.Snap(pt1);
                modelVM.SnapPoint = modelVM.SnapToNodes(pt1);
                if (modelVM.SnapPoint.X != float.MinValue) pt1.X = modelVM.SnapPoint.X;
                if (modelVM.SnapPoint.Y != float.MinValue) pt1.Y = modelVM.SnapPoint.Y;
                // TODO: Окончание создания компонентов при "касательном" вводе 
                // НАЖАТИЕ 
                if (e.ActionType == SKTouchAction.Pressed)
                {
                    switch (EditorAction)
                    {
                        case EditorActions.None: StartPt = pt1; SelectComponent(pt1); break;
                        case EditorActions.SelectFrame: SelectByFrame(pt1, SelectionStarted); break;
                        case EditorActions.NewLinearFirstPoint: CreateNewLinear(pt1); break;
                        case EditorActions.NewLinearLastPoint: FinishNewLinear(pt1, e.MouseButton); break;
                        case EditorActions.EditNode: CreateOrEditNode(pt1); break;
                        case EditorActions.NewDimensionFirstNode: DimensionFirstNode(pt1); break;
                        case EditorActions.NewDimensionVFirstNode: DimensionFirstNode(pt1); break;
                        case EditorActions.NewDimensionLastNode: DimensionLastNode(pt1); break;
                        case EditorActions.NewDimensionLinePlace: DimensionLinePlace(pt1); break;
                        case EditorActions.NewForce: CreateForce(pt1); break;
                        case EditorActions.NewDstLoad: CreateDistLoad(pt1); break;
                        case EditorActions.MirrorElements: MirrorElements(pt1, true); break;
                        case EditorActions.CopyElements: CopyElements(pt1, CopyBasePointDefined); break;
                    }
                }
                // ДВИЖЕНИЕ
                if (e.ActionType == SKTouchAction.Moved)
                { 
                    EndPt = pt1;
                    MovingView = e.InContact && (IssoDist.PointDst(StartPt, EndPt) > (5 / ScaleFactor));
                    switch (EditorAction)
                    {
                        case EditorActions.NewLinearLastPoint: ChangeLastPoint(pt1); break;
                        case EditorActions.NewDimensionLastNode: ChangeDimPlace(pt1); break;
                        case EditorActions.NewDimensionLinePlace: ChangeDimPlace(pt1); break;
                        case EditorActions.SelectFrame: if (SelectionStarted) SelectByFrame(pt1, false); break;
                        case EditorActions.MirrorElements: MirrorElements(pt1, false); break;
                        case EditorActions.None: if (MovingView) MoveOrigin(); break;
                        case EditorActions.CopyElements: CopyTargetPoint = pt1; break;
                    }
                    return;
                }
            }
            finally
            {
                // ОТОБРАЖАЕМ ПРОЦЕСС РЕДАКТИРОВАНИЯ
                if (EditorAction != EditorActions.None)
                {
                    RModelView.InvalidateSurface();
                }
                OnVisualStates?.Invoke(EditorAction);
            }
        }

        private void CopyElements(IssoPoint2D pt1, bool copyBasePointDefined)
        {
            // Если базовая точка не задана, то первым делом её задаём
            if (!copyBasePointDefined)
            {
                CopyBasePoint = pt1;
                CopyBasePointDefined = true;
            }
            else
            {
                modelVM.CreateCopy(CopyBasePoint, pt1);
                //CopyBasePointDefined = false;
            }
        }

        private void MirrorElements(IssoPoint2D point, bool touched)
        {
            if (MirroAxisDefined) return;
            // Для отражения элемента надо, чтобы были предварительно выбраны
            // какие-нибудь элементы
            // Если выбран хотя бы один, пользователь должен указать отрезок, относительно которого будут
            // отражены выбранные объекты
            // Этим отрезком может быть как прямолинейный элемент, так и просто линия между двумя точками, указываемыми пользователем
            // Таким образом, у этой операции есть три стадии:
            // 1. Выбор объектов
            // 2. Выбор отрезка
            //     2.1 Если щелчок мимо каких-либо прямолинейных элементов, то начинаем рисовать ось - первую точку
            //       2.1.1 Ждём указания второй точки - а пока рисуем ось от первой точки до текущего положения курсора
            //     2.2 Если щёлкнули элемент - сразу переходим на третий шаг 
            // 3. Подтверждение  
            //    3.1 Отображаем цветом результат отражения
            //    3.2 На панели инструментов пишем текст с вопросом и кнопкой "Отразить"
            IssoPoint2D pointOnObject;
            ComponentBasic b = modelVM.GetComponent(point, out pointOnObject);                
            // Если был щелчок, то есть два варианта:
            // 1. Указана первая точка
            // 2. Первая точка уже была задана, задаётся вторая
            if (touched)
            {
                if (MirrorAxis.Count == 0)
                {
                    if (b?.CompType == ComponentTypes.ctLinear)
                    {
                        MirrorAxis.Add((b as ComponentLinear).Start);
                        MirrorAxis.Add((b as ComponentLinear).End);
                        MirroAxisDefined = true;
                    } else
                    {
                        MirrorAxis.Add(pointOnObject);
                    }
                }
                else
                {
                    if (MirrorAxis.Count == 1)
                            MirrorAxis.Add(pointOnObject);
                    else MirrorAxis[1] = pointOnObject;
                    MirroAxisDefined = true;
                }
            }
            else
            // Если щелчка не было, то 
            // 1. Если точек тоже не было - ничего не делаем
            // 2. Если одна уже есть - добавляем вторую с координатами Point
            // 3. Если есть две - изменяем координаты второй 
            {
                if (MirrorAxis.Count == 0) return;
                if (MirrorAxis.Count == 1)
                    MirrorAxis.Add(pointOnObject);
                else MirrorAxis[1] = pointOnObject;
            }

            // Если ось задана, все выделенные элементы перемещаем
            // в список
            if (MirroAxisDefined)
            {
                OnMirrorConfirm?.Invoke(this, null);
            }
        }

        private void SelectByFrame(IssoPoint2D pt1, bool over)
        {
            if (!SelectionStarted)
            {
                SelectionStarted = true;
                SelectionRect.Left = pt1.X;
                SelectionRect.Top = pt1.Y;
                SelectionRect.Right = SelectionRect.Left;
                SelectionRect.Bottom = SelectionRect.Top;
            } else
            {
                SelectionRect.Right = pt1.X;
                SelectionRect.Bottom = pt1.Y;
            }
            if (over)
            {
                if (modelVM.SelectElementsByRect(SelectionRect) > 0) OnComponentSelected?.Invoke(modelVM.FirstSelectedBeam, null); 
                SelectionStarted = false;
                EditorAction = EditorActions.None;
                CancelAction();
                Invalidate();                
            };
        }        

        private void SetDistLoad(IssoPoint2D pt1)
        {
            if (EditedComp?.CompType == ComponentTypes.ctDistributedLoad)
            {
                ComponentLoad load = (ComponentLoad)EditedComp;
                IssoPoint2D p1 = load.AppNodes[0].Location;
                IssoPoint2D p2 = load.AppNodes[1].Location;

                ComponentLinear lin = modelVM.GetLinear(p1, p2);

                if (lin != null)
                {
                    modelVM.SplitLinearAt(lin, p1);
                    lin = modelVM.GetLinear(p1, p2);
                    modelVM.SplitLinearAt(lin, p2);
                    lin = modelVM.GetLinear(p1, p2);
                    RModel.EnableChangeTracking();
                    RModel.CompsList.Add(new ComponentLoad(ComponentTypes.ctDistributedLoad, -10, lin));
                    RModel.DisableChangeTracking();
                }
                EditorAction = EditorActions.None;
            }
        }

        private void ChangeDstLoadEnd(IssoPoint2D pt1)
        {
            if (EditedComp?.CompType == ComponentTypes.ctDistributedLoad)
            {
                // Определяем, какому линейному компоненту принадлежит начальный узел нагрузки
                ComponentLoad load = (ComponentLoad)EditedComp;
                ComponentLinear lin = (ComponentLinear)modelVM.GetComponent(pt1, out IssoPoint2D pt2, ComponentTypes.ctLinear);
                if (lin != null)
                {
                    load.SetLastNodeAt(pt2);
                }
            }
        }

        private ComponentNode GetLoadStartNode(IssoPoint2D pt1, ComponentTypes loadType)
        {
            // Если пользователь указал узел, то создаём силу, приложенную в этом узле
            ComponentNode node = modelVM.GetNodeAtPoint(pt1);
            if ((node == null) || (!modelVM.CloseEnough(pt1, node.Location)))
            {
                ComponentBasic lin = modelVM.GetComponent(pt1, out IssoPoint2D pt2);
                // Если же он указал линейный компонент, то создаём узел в ближайшей точке к указанной
                // точке компонента, а уже в нём - силу
                if (lin?.CompType == ComponentTypes.ctLinear)
                {                    
                    if (loadType == ComponentTypes.ctForce)
                    {
                        if (lin?.CompType == ComponentTypes.ctLinear) modelVM.SplitLinearAt((ComponentLinear)lin, pt1);
                        node = modelVM.GetNodeAtPoint(pt1);
                    } else node = new ComponentNode(pt2);
                }
                else node = null;
            }
            return node;
        }

        private void CreateDistLoad(IssoPoint2D pt1)
        {
            //ComponentNode node = GetLoadStartNode(pt1, ComponentTypes.ctDistributedLoad);
            IssoPoint2D pt2;
            ComponentBasic c = modelVM.GetComponent(pt1, out pt2, ComponentTypes.ctLinear);
            if (c != null)
            {
                ComponentLoad load = new ComponentLoad(ComponentTypes.ctDistributedLoad, -10, (ComponentLinear)c);
                RModel.CompsList.Add(load);
                EditedComp = load;
                EditorAction = EditorActions.None;
                Invalidate();
            }
            else EditedComp = null;
        }

        private void CreateForce(IssoPoint2D pt1)
        {
            ComponentNode node = GetLoadStartNode(pt1, ComponentTypes.ctForce);
            if (node != null)
            {
                ComponentLoad load = new ComponentLoad(ComponentTypes.ctForce, -100, node);
                RModel.EnableChangeTracking();
                RModel.CompsList.Add(load);
                RModel.DisableChangeTracking();
                EditedComp = load;
            }
            else EditedComp = null;
        }

        private void MoveOrigin()
        {
            ModelViewFragment.X += (StartPt.X - EndPt.X);
            ModelViewFragment.Y += (StartPt.Y - EndPt.Y);
            Grid.UpdateNeeded = true;
            MovingView = false;
            Invalidate();
        }

        private void ChangeDimPlace(IssoPoint2D pt1)
        {
            // Завершение ввода нового линейного компонента
            if (EditedComp?.CompType != ComponentTypes.ctBinding) return;
            ((IssoBinding)EditedComp).LinePlace = pt1;
        }

        private void DimensionLinePlace(IssoPoint2D pt1)
        {
            // Завершение ввода нового линейного компонента
            if (EditedComp?.CompType != ComponentTypes.ctBinding) return;
            ((IssoBinding)EditedComp).LinePlace = pt1;
            RModel.EnableChangeTracking();
            RModel.CompsList.Add(EditedComp);
            RModel.DisableChangeTracking();
            EditorAction = EditorActions.None;
        }

        private void DimensionLastNode(IssoPoint2D pt1)
        {
            // Завершение ввода нового линейного компонента
            if (EditedComp?.CompType != ComponentTypes.ctBinding) return;           

            ComponentNode node = modelVM.GetNodeAtPoint(pt1);

            if (node != null)
            {
               ((IssoBinding)EditedComp).Target = node;
               EditorAction = EditorActions.NewDimensionLinePlace;
            }
        }

        private void DimensionFirstNode(IssoPoint2D pt1)
        {
            // Первый узел - начало линейного размера
            ComponentNode node = modelVM.GetNodeAtPoint(pt1);
            if (node != null)
            {
                IssoBindingType t = IssoBindingType.Horizontal;
                switch (EditorAction)
                {
                    case EditorActions.NewDimensionFirstNode: t = IssoBindingType.Horizontal; break;
                    case EditorActions.NewDimensionVFirstNode: t = IssoBindingType.Vertical; break;
                }
                IssoBinding bin = new IssoBinding(t, node, null, 0);
                EditedComp = bin;
                EditorAction = EditorActions.NewDimensionLastNode;
            }
        }

        private void ChangeLastPoint(IssoPoint2D pt1)
        {
            if (EditedComp?.CompType == ComponentTypes.ctLinear) ((ComponentLinear)EditedComp).End = pt1;
        }

        private void CreateOrEditNode(IssoPoint2D pt1)
        {
            // РЕДАКТИРОВАНИЕ УЗЛА
            // Сначала найдём ближайший к pt1 компонент и точку на нём - 
            // Если pt1 далека от какого-либо компонента, не делаем ничего
            // Если эта точка далека от существующего узла компонента, то
            // Создаём новый узел, разделяя компонент на две части
            IssoPoint2D pt2;
            ComponentBasic comp = modelVM.GetComponent(pt1, out pt2);
            // Если эта точка близка к началу или концу линейного компонента - выбираем начало или конец
            // Если нет - разбиваем компонент на две части
            if (comp?.CompType == ComponentTypes.ctLinear)
            {
                modelVM.SplitLinearAt((ComponentLinear)comp, pt2);
                EditedComp = new ComponentNode(pt2);
                RModel.CompsList.Add(EditedComp);
                OnComponentSelected?.Invoke(EditedComp, null);
            }
            if (comp?.CompType == ComponentTypes.ctNode)
            {
                EditedComp = comp;
                OnComponentSelected?.Invoke(EditedComp, null);
            }
        }

        private void FinishNewLinear(IssoPoint2D pt1, SKMouseButton MouseButton)
        {
            // Завершение ввода нового линейного компонента
            if (EditedComp?.CompType != ComponentTypes.ctLinear) return;

            ((ComponentLinear)EditedComp).End = pt1;
            ApplySnap((ComponentLinear)EditedComp, pt1);
            
            AddNewLinear();
            // Тут же начинаем новый элемент
            CreateNewLinear(((ComponentLinear)EditedComp).End);
        }

        private void ApplySnap(ComponentLinear linear, IssoPoint2D pt1)
        {            
            ComponentNode node = modelVM.GetNodeAtPoint(pt1);
            if (node != null)
                linear.End = node.Location;
            else
            {
                // Используем выравнивание. Если угол близок к 0, 90, 180, 270 и т.п. - выравниваем.
                float a = IssoBind.OrthoAngle(linear.AngleD);
                if ((a == 0) || (a == 180)) linear.EndNode.MoveTo(new IssoPoint2D() { X = pt1.X, Y = linear.Start.Y });
                if ((a == 90) || (a == 270) || (a == -90)) linear.EndNode.MoveTo(new IssoPoint2D() { Y = pt1.Y, X = linear.Start.X });
            }
        }

        private void AddNewLinear()
        {
            // Добавляем элемент в модель
            // Если в модели уже были узлы начала и конца этого нового элемента
            // то новые узлы не добавляем. В противном случае - добавляем и узлы
            if (EditedComp?.CompType == ComponentTypes.ctLinear)
            {
                ComponentLinear lin = (ComponentLinear)EditedComp;
                ComponentNode node1 = modelVM.GetNodeAtPoint(lin.Start);
                ComponentNode node2 = modelVM.GetNodeAtPoint(lin.End);
                if (node1 == null)
                {
                    RModel.CompsList.Add(lin.StartNode);
                    node1 = lin.StartNode;
                }
                if (node2 == null)
                {
                    RModel.CompsList.Add(lin.EndNode);
                    node2 = lin.EndNode;
                }
                EditedComp = new ComponentLinear(node1, node2, RModel);
                RModel.EnableChangeTracking();
                RModel.CompsList.Add(EditedComp);
                RModel.DisableChangeTracking();
            }
        }

        private void CreateNewLinear(IssoPoint2D pt1)
        {
            // НОВЫЙ ЛИНЕЙНЫЙ КОМПОНЕНТ
            // Компонент идёт в паре с двумя узлами.
            // Первый узел - в точке, указанной пользователем (pt1)
            // Если в этой точке уже есть другой узел, то берём его как начало элемента
            // Если тут нет узла - создаём его, и берём как стартовый
            // Если пользователь указал точку, лежащую на линейном компоненте - создаём в этом месте
            // узел, разделяя компонент на две части, и берём его как стартовый
            ComponentNode node = modelVM.GetNodeAtPoint(pt1);
            if (node == null)
            {
                IssoPoint2D pt2;
                ComponentBasic b = modelVM.GetComponent(pt1, out pt2);
                if (b?.CompType == ComponentTypes.ctLinear)
                {
                    modelVM.SplitLinearAt((ComponentLinear)b, pt2);
                    node = modelVM.GetNodeAtPoint(pt2);
                } node = new ComponentNode(pt1);
            }
            pt1.X += 0.1f;
            EditedComp = new ComponentLinear(node, new ComponentNode(pt1), RModel);
            EditorAction = EditorActions.NewLinearLastPoint;
        }

        private void SelectComponent(IssoPoint2D pt1)
        {
            // Если в данный момент никакой операции редактирования не выполняется,
            // то просто определяем, есть ли в зоне касания какой-либо компонент.
            // Если есть, то выбираем его
            if (EditorAction == EditorActions.None)
            {
                IssoPoint2D pt2;
                ComponentBasic comp = modelVM.GetComponent(pt1, out pt2);
                // НЕ НАДО) Меняем состояние ранее выбранного компонента на обычное
                // modelVM.DropSelection();                
                if (comp != null)
                {
                    EditedComp = comp;
                    EditedComp.CompState = ComponentState.csSelected;
                }
                else EditedComp = null;
                OnComponentSelected?.Invoke(EditedComp, null);
                Invalidate();
            }
        }

        public void IssoAction(EditorActions action)
        {
            //CancelAction();
            EditorAction = action;
            StartAction();
        }

        private void StartAction()
        {
            // Начинаем текущую операцию редактирования
        }

        public void CancelAction()
        {
            // Прерываем текущую операцию редактирования
            EditorAction = EditorActions.None;
            EditedComp = null;
            RModelView.InvalidateSurface();            
            MirroAxisDefined = false;
            SelectionStarted = false;
            CopyBasePointDefined = false;
            SelectionRect = new SKRect(0, 0, 0, 0);
            MirrorAxis.Clear();
            OnActionNone?.Invoke(this);
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();
            Draw(canvas);
        }

        public void Draw(SKCanvas canvas)
        {
            if (!MovingView) Grid.DrawGrid(this, canvas);
            // Сначала отображаем узлы и прочее, затем стержни          
            for (int i = 0; i < RModel.CompsList.Count; i++)
            {
                if (RModel.CompsList[i].CompType != ComponentTypes.ctLinear) DrawComponent(RModel.CompsList[i], canvas);
            }

            for (int i = 0; i < RModel.CompsList.Count; i++)
            {
                if (RModel.CompsList[i].CompType == ComponentTypes.ctLinear) DrawComponent(RModel.CompsList[i], canvas);
            }

            DrawComponent(EditedComp, canvas);
            
            switch (EditorAction)
            {
                case EditorActions.SelectFrame:
                    if (SelectionStarted)
                        modelVM.DrawSelectionRect(SelectionRect, canvas);
                    else modelVM.DrawSnapLines(this, canvas);
                    break;
                case EditorActions.MirrorElements: if (MirrorAxis.Count == 2) modelVM.DrawMirrorAxis(MirrorAxis, canvas); break;
                case EditorActions.ArrayElements: modelVM.PreviewArray(canvas); break;
                case EditorActions.CopyElements: modelVM.PreviewCopy(canvas, CopyBasePoint, CopyTargetPoint); break;
            }
           
            if (drawDeformedShape) modelVM.DrawDeformedShape(this, canvas);            
        }

        private void DrawComponent(ComponentBasic c, SKCanvas canvas)
        {
            if (c == null) return;
            switch (c.CompType)
            {
                case ComponentTypes.ctLinear: ComponentLinearVM.Draw((ComponentLinear)c, this, canvas); break;
                case ComponentTypes.ctNode: ComponentNodeVM.DrawNode((ComponentNode)c, this, canvas); break;
                case ComponentTypes.ctBinding: IssoBindingVM.DrawDimension((IssoBinding)c, this, canvas); break;
                case ComponentTypes.ctForce: ComponentLoadVM.Draw((ComponentLoad)c, this, canvas); break;
                case ComponentTypes.ctDistributedLoad: ComponentLoadVM.Draw((ComponentLoad)c, this, canvas); break;
            }
        }

        public void Invalidate()
        {
            RModelView.InvalidateSurface();
        }
    }
}
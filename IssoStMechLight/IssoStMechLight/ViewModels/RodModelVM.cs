using IssoStMechLight.Models;
using IssoStMechLight.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using System.Linq;
using System.Text;
using System.IO;

namespace IssoStMechLight.ViewModels
{
    public class RodModelVM: IssoBaseVM
    {
        public RodModel model;
        public ModelViewSurface surface;
        public IssoPoint2D SnapPoint;
        float DistTolerance { get { return (surface.ViewHeight / 80) / surface.ScaleFactor; } }

        private int arrayHorizontalCount = 1;
        public int ArrayHorizontalCount
        {
            get
            {
                return arrayHorizontalCount;
            }
            set
            {
                arrayHorizontalCount = value;
                surface.Invalidate();
            }
        }

        private float arrayHorizontalStep = 0;
        public float ArrayHorizontalStep
        {
            get
            {
                return arrayHorizontalStep;
            }
            set
            {
                arrayHorizontalStep = value;
                surface.Invalidate();
            }
        }

        private int arrayVerticalCount = 1;
        public int ArrayVerticalCount
        {
            get
            {
                return arrayVerticalCount;
            }
            set
            {
                arrayVerticalCount = value;
                surface.Invalidate();
            }
        }

        private float arrayVerticalStep = 0;
        public float ArrayVerticalStep
        {
            get
            {
                return arrayVerticalStep;
            }
            set
            {
                arrayVerticalStep = value;
                surface.Invalidate();
            }
        }

        public IEnumerable<ComponentLinear> ModelBeams
        {
            get
            {
                if (!MultipleSelected)
                    return from ComponentBasic c in model.CompsList where c.CompType == ComponentTypes.ctLinear select (ComponentLinear)c;
                else return new List<ComponentLinear>() { FirstSelectedBeam };
            }
        }

        public bool MultipleSelected
        {
            get { return model.MultipleSelected; }
            set { model.MultipleSelected = value; }
        }

        public int SelectedBeamsCount
        {
            get
            {
                return (from ComponentBasic c in model.CompsList
                       where (c.CompType == ComponentTypes.ctLinear) && (c.CompState == ComponentState.csSelected)
                       select (ComponentLinear)c).Count();
            }
        }

        public IEnumerable<ComponentLinear> SelectedBeams
        {
            get
            {
                return from ComponentBasic c in model.CompsList where (c.CompType == ComponentTypes.ctLinear) && (c.CompState == ComponentState.csSelected)
                       select (ComponentLinear)c;                
            }
        }

        public ComponentLinear FirstSelectedBeam
        {
            get
            {
                return (from ComponentBasic c in model.CompsList
                       where (c.CompType == ComponentTypes.ctLinear) && (c.CompState == ComponentState.csSelected)
                       select (ComponentLinear)c).First();
            }
        }

        public IEnumerable<ComponentNode> NodeList
        {
            get
            {
                return from ComponentBasic c in model.CompsList where c.CompType == ComponentTypes.ctNode select (ComponentNode)c;
            }
        }

        public RodModelVM(RodModel model, SKCanvasView surface)
        {
            this.model = model;
            this.surface = new ModelViewSurface(this, surface);
        }

        public bool CloseEnough(IssoPoint2D pt1, IssoPoint2D pt2)
        {
            float d = IssoDist.PointDst(pt1, pt2);
            return d <= DistTolerance;
        }

        public bool CloseEnough(float dst)
        {
            return dst <= DistTolerance;
        }


        public float NodeDist(IssoPoint2D pt, ComponentNode node)
        {
            // Если точка pt лежит внутри области узла, возвращаем ноль - иначе
            // возвразаем расстояние от pt до Node.Location
            if (ComponentNodeVM.Contains(node, pt, surface)) return 0f; else return IssoDist.PointDst(pt, node.Location);
        }

        internal ComponentBasic GetComponent(IssoPoint2D pt1, out IssoPoint2D pt2, ComponentTypes type = ComponentTypes.ctAny)
        {
            pt2 = new IssoPoint2D() { X = pt1.X, Y = pt1.Y };
            if (model.CompsList.Count == 0) return null;

            int j = 0;
            for (int i = 0; i < model.CompsList.Count; i++)
            {
                switch (model.CompsList[i].CompType)
                {
                    case ComponentTypes.ctNode:
                        {
                            if ((type == ComponentTypes.ctAny) || (type == ComponentTypes.ctNode))
                            {
                                if (ComponentNodeVM.Contains((ComponentNode)model.CompsList[i], pt1, surface))
                                {
                                    pt2 = ((ComponentNode)model.CompsList[i]).Location;
                                    return model.CompsList[i];
                                }
                            }
                            break;
                        }
                    case ComponentTypes.ctLinear:
                        {
                            if ((type == ComponentTypes.ctAny) || (type == ComponentTypes.ctLinear))
                            {
                                if (ComponentLinearVM.Contains((ComponentLinear)model.CompsList[i], pt1, surface))
                                {
                                    pt2 = IssoDist.PurpPoint(pt1, (ComponentLinear)model.CompsList[i]);
                                    return model.CompsList[i];
                                }
                            }
                            break;
                        }
                    case ComponentTypes.ctBinding:
                        {
                            if ((type == ComponentTypes.ctAny) || (type == ComponentTypes.ctBinding))
                            {
                                if (IssoBindingVM.Contains((IssoBinding)model.CompsList[i], pt1, surface))
                                {
                                    pt2 = ((IssoBinding)model.CompsList[i]).LinePlace;
                                    return model.CompsList[i];
                                }
                            }
                            break;
                        }
                    case ComponentTypes.ctForce: case ComponentTypes.ctDistributedLoad:
                        {
                            if ((type == ComponentTypes.ctAny) || (type == ComponentTypes.ctForce) || (type == ComponentTypes.ctDistributedLoad))
                            {
                                if (ComponentLoadVM.Contains((ComponentLoad)model.CompsList[i], pt1, surface))
                                {
                                    pt2 = ((ComponentLoad)model.CompsList[i]).AppNodes[0].Location;
                                    return model.CompsList[i];
                                }
                            }
                            break;
                        }
                }
            }

            return null;
        }
        
        private int GetMinIndex(float[] dst)
        {
            float mv = (from d in dst select d).Min();
            for (int i = 0; i < dst.Length; i++)
            {
                if (dst[i] == mv) return i;
            }
            return -1;
        }

        private void FillMax(float[] dst)
        {
            foreach (int i in dst) dst[i] = float.MaxValue;
        }

        internal void SplitLinearAt(ComponentLinear l, IssoPoint2D pt2)
        {
            if (CloseEnough(pt2, l.Start) || CloseEnough(pt2, l.End)) return;
            ComponentLinear lin = l.SplitAt(pt2);
            model.EnableChangeTracking();
            model.CompsList.Add(lin.StartNode);
            model.CompsList.Add(lin);
            model.DisableChangeTracking();
        }

        internal ComponentNode GetNodeAtPoint(IssoPoint2D pt1)
        {
            return (ComponentNode)GetComponent(pt1, out IssoPoint2D pt2, ComponentTypes.ctNode);            
        }

        internal ComponentLinear GetLinear(IssoPoint2D p1, IssoPoint2D p2)
        {
            // Возвращаем линейный компонент, которому принадлежат обе точки p1 и p2
            for (int i = 0; i < model.CompsList.Count; i++)
            {
                if (model.CompsList[i].CompType == ComponentTypes.ctLinear)
                {
                    ComponentLinear lin = (ComponentLinear)model.CompsList[i];
                    if (IssoDist.PointsOnLine(new IssoPoint2D[] { p1, p2 }, new IssoPoint2D[] { lin.Start, lin.End }))
                        return lin;
                }
            }
            return null;
        }

        internal void DrawDeformedShape(ModelViewSurface modelViewSurface, SKCanvas canvas)
        {
            double maxd = 0;
            for (int i = 0; i < model.Rods.Count; i++)
            {
                // Для того, чтобы подобрать подходящий масштаб, сначала определим макисмальное линейное перемещение
                // (т.е. без учёта углов поворота)
                for (float x = 0; x < model.Rods[i].Length; x += (float)model.Rods[i].Length / 10f)
                {
                    double[] d = model.Rods[i].DeformedShape(x);
                    if (maxd <= Math.Abs(d[0])) maxd = Math.Abs(d[0]);
                    if (maxd <= Math.Abs(d[1])) maxd = Math.Abs(d[1]);
                }
            }
            // Чтобы деформированный вид выглядел информативно, нужно, чтобы максимальное перемещение
            // составляло примерно 10% от высоты окна просмотра в пикселях
            float deformedScale = 1.0f;
            if (maxd > 0) deformedScale = modelViewSurface.ViewHeight * 0.1f / (float)maxd / modelViewSurface.ScaleFactor;
            SKPath deformed = new SKPath();
            for (int i = 0; i < model.Rods.Count; i++)
            {
                float elementLength = (float)model.Rods[i].Length;
                float dx = elementLength / 98;
                // Разбиваем длину элемента на 98 частей
                // Это гарантирует нам 99 точек по длине элемента - что нужно для
                // отображения деформированного вида с помощью кубических кривых
                for (float x = 0; x < elementLength; x += 3 * dx)
                {
                    double[] xyF1 = model.Rods[i].DeformedShapeGlobalXY(x, deformedScale);
                    double[] xyF2 = model.Rods[i].DeformedShapeGlobalXY(x + dx, deformedScale);
                    double[] xyF3 = model.Rods[i].DeformedShapeGlobalXY(x + 2 * dx, deformedScale);

                    SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = (float)xyF1[0], Y = (float)xyF1[1] }, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                    SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = (float)xyF2[0], Y = (float)xyF2[1] }, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                    SKPoint pt3 = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = (float)xyF3[0], Y = (float)xyF3[1] }, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

                    if (x == 0) deformed.MoveTo(pt1);
                    deformed.CubicTo(pt1, pt2, pt3);
                }
            }
        
            for (int i = 0; i < model.Displacements.Length; i+=3)
            {
                float x = model.Nodes[(int)i / 3].Location.X;
                float y = model.Nodes[(int)i / 3].Location.Y;
                SKPoint node = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = x + (float)model.Displacements[i] * deformedScale,
                                                                                    Y = y + (float)model.Displacements[i+1] * deformedScale
                }, 
                surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                deformed.AddCircle(node.X, node.Y, 5);
            }

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Goldenrod.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 3
            };

            canvas.DrawPath(deformed, paint);
        }

        internal void DoMirror()
        {
            // Создаём новые объекты, зеркально отражённые от выбранных
            List<ComponentBasic> newElements = new List<ComponentBasic>();
            foreach (ComponentLinear cl in SelectedBeams)
            {
                ComponentNode nd1 = MirrorNode(cl.Start, newElements);
                ComponentNode nd2 = MirrorNode(cl.End, newElements);
                newElements.Add(new ComponentLinear(nd1, nd2, model));
            }
            foreach (ComponentBasic cl in newElements) model.CompsList.Add(cl);
        }        

        private ComponentNode MirrorNode(IssoPoint2D point, List<ComponentBasic> newElements)
        {
            IssoPoint2D pt = IssoDist.MirrorPoint(point, surface.MirrorAxis[0], surface.MirrorAxis[1]);            
            
            IssoPoint2D ptout;
            ComponentNode nd = (ComponentNode)GetComponent(pt, out ptout, ComponentTypes.ctNode);
            if (nd == null)
            {
                nd = new ComponentNode(pt);
                newElements.Add(nd);
            }
            return nd;
        }

        internal Stream Save()
        {
           return model.Save();
        }

        internal void Load(MemoryStream ms)
        {
            model.Load(ms);
            surface.ZoomAll();
        }

        internal void Redo()
        {
            model.Redo();
            surface.Invalidate();
        }

        internal void Undo()
        {
            model.Undo();
            surface.Invalidate();
        }

        public void DrawSnapLines(ModelViewSurface surface, SKCanvas canvas)
        {
            SKPaint dashPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.GreenYellow.ToSKColor(),
                PathEffect = SKPathEffect.CreateDash(new Single[2] { surface.ViewHeight / 100, surface.ViewHeight / 100 }, surface.ViewHeight / 80),
                IsAntialias = true,
                StrokeWidth = 1
            };
            if (SnapPoint.X != float.MinValue)
            {
                SKPoint pt = IssoConvert.IssoPoint2DToSkPoint(SnapPoint, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                canvas.DrawLine(pt.X, 0, pt.X, surface.ViewHeight, dashPaint);
            }
            if (SnapPoint.Y != float.MinValue)
            {
                SKPoint pt = IssoConvert.IssoPoint2DToSkPoint(SnapPoint, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                canvas.DrawLine(0, pt.Y, surface.ViewWidth, pt.Y, dashPaint);
            }
        }

        internal int SelectElementsByRect(SKRect selectionRect)
        {
            IssoRect2D rect = new IssoRect2D(new IssoPoint2D() { X = selectionRect.Left, Y = selectionRect.Top }, selectionRect.Width, selectionRect.Height);
            int count = 0;
            foreach (ComponentLinear c in ModelBeams)
            {
                if (IssoDist.PointInRect(c.Start, rect) && IssoDist.PointInRect(c.End, rect))
                {
                    c.CompState = ComponentState.csSelected;
                    count++;
                }
            }
            MultipleSelected = count > 1;
            return count;
        }

        internal IssoPoint2D SnapToNodes(IssoPoint2D pt)
        {
            IssoPoint2D res = new IssoPoint2D();

            // Возвращаем точку, находящуюся на одной прямой (или пересечении двух перпендикулярных прямых) 
            // с ближайшими по координатам X и Y
            // 1. Ищем узел N с ближайшей к pt.X координатой X
            // 2. Ищем узел M с ближайшей к pt.Y координатой Y
            // Если близость координат достаточна - возвращаем точку на пересечении прямых - вертикальной, 
            // проходящей через N и горизонтальной, проходящей через M
            ComponentNode N = model.ModelNodes.Find(n => Math.Abs(n.Location.X - pt.X) < DistTolerance);
            ComponentNode M = model.ModelNodes.Find(m => Math.Abs(m.Location.Y - pt.Y) < DistTolerance);

            if ((N != null) && (M != null))
            {
                res.X = N.Location.X;
                res.Y = M.Location.Y;
            }

            if ((N != null) && (M == null))
            {
                res.X = N.Location.X;
                res.Y = float.MinValue;
            }
       
            if ((N == null) && (M != null))
            {
                res.X = float.MinValue;
                res.Y = M.Location.Y;
            }

            if ((N == null) && (M == null))
            {
                res.X = float.MinValue;
                res.Y = float.MinValue;
            }

            return res;
        }

        internal void DrawSelectionRect(SKRect selectionRect, SKCanvas canvas)
        {
            SKPaint dashPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.AliceBlue.ToSKColor(),
                PathEffect = SKPathEffect.CreateDash(new Single[2] { surface.ViewHeight / 100, surface.ViewHeight / 100 }, surface.ViewHeight / 80),
                IsAntialias = true,
                StrokeWidth = 1
            };
            IssoPoint2D topLeft = new IssoPoint2D() { X = selectionRect.Left, Y = selectionRect.Top };
            IssoPoint2D bottomRight = new IssoPoint2D() { X = selectionRect.Right, Y = selectionRect.Bottom };
            SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(topLeft, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(bottomRight, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            canvas.DrawRect(pt1.X, pt1.Y, pt2.X - pt1.X, pt2.Y - pt1.Y, dashPaint);            
        }

        internal void DropSelection()
        {
            foreach (ComponentBasic b in model.CompsList) b.CompState = ComponentState.csNormal;
            MultipleSelected = false;
        }

        internal void DrawMirrorAxis(List<IssoPoint2D> mirrorAxis, SKCanvas canvas)
        {
            SKPaint dashPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.AliceBlue.ToSKColor(),
                PathEffect = SKPathEffect.CreateDash(new Single[2] { surface.ViewHeight / 100, surface.ViewHeight / 100 }, surface.ViewHeight / 80),
                IsAntialias = true,
                StrokeWidth = 1
            };
            SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(mirrorAxis[0], surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(mirrorAxis[1], surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, dashPaint);

            // Рисуем то, как будет выглядеть отражение
            dashPaint.PathEffect = null;
            foreach (ComponentLinear c in SelectedBeams)
            {
                IssoPoint2D p1 = IssoDist.MirrorPoint(c.Start, mirrorAxis[0], mirrorAxis[1]);
                IssoPoint2D p2 = IssoDist.MirrorPoint(c.End, mirrorAxis[0], mirrorAxis[1]);
                pt1 = IssoConvert.IssoPoint2DToSkPoint(p1, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                pt2 = IssoConvert.IssoPoint2DToSkPoint(p2, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

                canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, dashPaint);
            }
        }

        private List<ComponentLinear> ArrayElements()
        {
            // Сначала проверим, есть ли возможность хоть что-то нарисовать
            // Условия: количество элементов больше 1 и шаг больше 0
            List<ComponentLinear> newElements = new List<ComponentLinear>();

            int hc = 1, vc = 1;
            float hs = 0, vs = 0;

            if (arrayHorizontalCount > 0) hc = arrayHorizontalCount;
            if (arrayVerticalCount > 0) vc = arrayVerticalCount;
            if (arrayHorizontalStep != 0) hs = arrayHorizontalStep;
            if (arrayVerticalStep != 0) vs = arrayVerticalStep;

            for (int h = 0; h < hc; h++)
            {
                for (int v = 0; v < vc; v++)
                {
                    // Элемент с индексом 0, 0 пропускаем - поскольку он уже создан
                    if (h + v == 0) continue;

                    foreach (ComponentLinear c in SelectedBeams)
                    {
                        IssoPoint2D point1 = c.Start;
                        point1.X += h * hs; point1.Y += v * vs;

                        IssoPoint2D point2 = c.End;
                        point2.X += h * hs; point2.Y += v * vs;

                        ComponentNode nd1 = new ComponentNode(point1);
                        ComponentNode nd2 = new ComponentNode(point2);

                        newElements.Add(new ComponentLinear(nd1, nd2, model));
                    }
                }
            }
            return newElements;
        }

        internal void CreateArray()
        {
            List<ComponentLinear> basics = ArrayElements();
            IssoPoint2D Point;
            for (int i = 0; i < basics.Count; i++)
            {
                ComponentNode nd1 = (ComponentNode)GetComponent(basics[i].Start, out Point, ComponentTypes.ctNode);
                ComponentNode nd2 = (ComponentNode)GetComponent(basics[i].End, out Point, ComponentTypes.ctNode);

                if (nd1 == null)
                {
                    model.CompsList.Add(basics[i].StartNode);
                    nd1 = basics[i].StartNode;
                }
                if (nd2 == null)
                {
                    model.CompsList.Add(basics[i].EndNode);
                    nd2 = basics[i].EndNode;
                }
                model.CompsList.Add(new ComponentLinear(nd1, nd2, model));
            }
        }

        public void PreviewArray(SKCanvas canvas)
        {
            SKPaint Paint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.AliceBlue.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            };

            List<ComponentLinear> basics = ArrayElements();
            if (basics.Count > 0)
            {
                foreach (ComponentLinear c in basics)
                {
                    SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(c.Start, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                    SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(c.End, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

                    canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, Paint);
                }
            }
        }

        internal void PreviewCopy(SKCanvas canvas, IssoPoint2D copyBasePoint, IssoPoint2D copyTargetPoint)
        {
            SKPaint Paint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.AliceBlue.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            };

            List<ComponentLinear> basics = CopyElements(copyBasePoint, copyTargetPoint);
            if (basics.Count > 0)
            {
                foreach (ComponentLinear c in basics)
                {
                    SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(c.Start, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                    SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(c.End, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

                    canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, Paint);
                }
            }
        }

        private List<ComponentLinear> CopyElements(IssoPoint2D copyBasePoint, IssoPoint2D copyTargetPoint)
        {
            // Сначала проверим, есть ли возможность хоть что-то нарисовать
            // Условия: количество элементов больше 1 и шаг больше 0
            List<ComponentLinear> newElements = new List<ComponentLinear>();
         
            foreach (ComponentLinear c in SelectedBeams)
            {
                IssoPoint2D point1 = c.Start;
                point1.X += copyTargetPoint.X - copyBasePoint.X; point1.Y += copyTargetPoint.Y - copyBasePoint.Y;

                IssoPoint2D point2 = c.End;
                point2.X += copyTargetPoint.X - copyBasePoint.X; point2.Y += copyTargetPoint.Y - copyBasePoint.Y;

                ComponentNode nd1 = new ComponentNode(point1);
                ComponentNode nd2 = new ComponentNode(point2);

                newElements.Add(new ComponentLinear(nd1, nd2, model));
            }
            
            return newElements;
        }

        internal void CreateCopy(IssoPoint2D copyBasePoint, IssoPoint2D copyTargetPoint)
        {
            List<ComponentLinear> basics = CopyElements(copyBasePoint, copyTargetPoint);
            IssoPoint2D Point;
            for (int i = 0; i < basics.Count; i++)
            {
                ComponentNode nd1 = (ComponentNode)GetComponent(basics[i].Start, out Point, ComponentTypes.ctNode);
                ComponentNode nd2 = (ComponentNode)GetComponent(basics[i].End, out Point, ComponentTypes.ctNode);

                if (nd1 == null)
                {
                    model.CompsList.Add(basics[i].StartNode);
                    nd1 = basics[i].StartNode;
                }
                if (nd2 == null)
                {
                    model.CompsList.Add(basics[i].EndNode);
                    nd2 = basics[i].EndNode;
                }
                model.CompsList.Add(new ComponentLinear(nd1, nd2, model));
            }
        }
    }
}

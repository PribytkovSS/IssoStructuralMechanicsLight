using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using StarMathLib;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;

namespace IssoStMechLight.Models
{
    public class RodModel
    {
        public IDictionary<string, ObservableCollection<ComponentBasic>> ChangesHistory = new Dictionary<string, ObservableCollection<ComponentBasic>>();
        public ObservableCollection<ComponentBasic> CompsList { get; }
        public int HistoryPosition = -1;

        public List<ComponentNode> ModelNodes
        {
            get
            {                
                return ModelNodes2(CompsList);
            }
        }

        public List<ComponentNode> ModelNodes2(ObservableCollection<ComponentBasic> Source)
        {
            List<ComponentNode> c = new List<ComponentNode>();
            for (int i = 0; i < Source.Count; i++)
                if (Source[i].CompType == ComponentTypes.ctNode)
                    c.Add((ComponentNode)Source[i]);
            return c;
        }

        public List<ComponentLinear> ModelBeams
        {
            get
            {
                return ModelBeams2(CompsList);
            }
        }

        public List<ComponentLinear> ModelBeams2(ObservableCollection<ComponentBasic> Source)
        {
            List<ComponentLinear> c = new List<ComponentLinear>();
            for (int i = 0; i < Source.Count; i++)
                if (Source[i].CompType == ComponentTypes.ctLinear)
                    c.Add((ComponentLinear)Source[i]);
            return c;
        }

        public List<ComponentLoad> ModelForces
        {
            get
            {
                return ModelForces2(CompsList);
            }
        }

        public List<ComponentLoad> ModelForces2(ObservableCollection<ComponentBasic> Source)
        {
            List<ComponentLoad> c = new List<ComponentLoad>();
            for (int i = 0; i < Source.Count; i++)
                if (Source[i].CompType == ComponentTypes.ctForce)
                    c.Add((ComponentLoad)Source[i]);
            return c;
        }

        public List<ComponentLoad> ModelDistLoads
        {
            get
            {
                return ModelDistLoads2(CompsList);
            }
        }

        public List<ComponentLoad> ModelDistLoads2(ObservableCollection<ComponentBasic> Source)
        {
            List<ComponentLoad> c = new List<ComponentLoad>();
            for (int i = 0; i < Source.Count; i++)
                if (Source[i].CompType == ComponentTypes.ctDistributedLoad)
                    c.Add((ComponentLoad)Source[i]);
            return c;
        }

        #region для расчёта МКЭ
        public List<ElementBeam> Rods { get; }

        public List<ComponentNode> Nodes { get; }
        #endregion

        public ObservableCollection<IssoCrossSection> CrossSections { get; set; }

        public double[] Displacements;
        public double[,] GlobalMatrix;
        public double[] Loads;
        public List<IssoBinding> Bindings
        {
            get
            {           
                List<IssoBinding> bin = new List<IssoBinding>();
                for (int i = 0; i < CompsList.Count; i++)
                    if (CompsList[i].CompType == ComponentTypes.ctBinding)
                        bin.Add((IssoBinding)CompsList[i]);
                return bin;
            }
        }

        public bool MultipleSelected { get; internal set; }

        private float[,] ndX; 
        private float[,] ndY;
        private bool TrackChanges = false;

        public RodModel()
        {
            CompsList = new ObservableCollection<ComponentBasic>();
            CompsList.CollectionChanged += CompsList_CollectionChanged;
            Nodes = new List<ComponentNode>();
            Rods = new List<ElementBeam>();
            CreateCrossSectionsCollection();
        }

        private void CreateCrossSectionsCollection()
        {
            CrossSections = new ObservableCollection<IssoCrossSection>();
            CrossSections.Add(new IssoCrossSection() { SectionArea = 1, SectionInertia = 1, MaterialElasticity = 1, SectionName = "По умолчанию" });
        }

        private void CompsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Тут мы будем запоминать изменения, заключающиеся в добавлении или удалении элементов модели. 
            // Изменения свойств объектов - через другие методы
            IndexElements(e.NewItems);
            if (TrackChanges)
            {
                string changeExplanation = DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString();
                changeExplanation += "; " + e.Action.ToString();

                if ((e.NewItems != null) && (e.NewItems.Count > 0))
                {
                    changeExplanation += " (";
                    for (int i = 0; i < e.NewItems.Count; i++) changeExplanation += e.NewItems[i].ToString() + "; ";
                    changeExplanation += ")";
                }

                if ((e.OldItems != null) && (e.OldItems.Count > 0))
                {
                    changeExplanation += " (";
                    for (int i = 0; i < e.OldItems.Count; i++) changeExplanation += e.OldItems[i].ToString() + "; ";
                    changeExplanation += ")";
                }
                ObservableCollection<ComponentBasic> h = new ObservableCollection<ComponentBasic>();
                Stream ms = Save(CompsList);
                Load(ms, h);
                HistoryPosition++;
                // Если есть записи истории после текущего HistoryPosition,
                // (такое может быть в результате нескольких Undo и внесения после этого изменений в модель)
                // то удаляем всю последующую историю
                for (int i = HistoryPosition; i < ChangesHistory.Count; i++) ChangesHistory.Remove(ChangesHistory.Keys.ElementAt(HistoryPosition));

                ChangesHistory.Add(changeExplanation, h);
            }
        }

        private void IndexElements(IList newItems)
        {
            if (newItems == null) return;

            IEnumerable<int> nodesIndx = from ComponentBasic c in CompsList
                                         where c.CompType == ComponentTypes.ctNode
                                         select (c as ComponentNode).NodeIndex;
            IEnumerable<int> beamsIndx = from ComponentBasic c in CompsList
                                         where c.CompType == ComponentTypes.ctLinear
                                         select (c as ComponentLinear).BeamIndex;
            int nodeIndexMax = 0;
            int beamIndexMax = 0;
            if (nodesIndx.Count() > 0) nodeIndexMax = nodesIndx.Max();
            if (beamsIndx.Count() > 0) beamIndexMax = beamsIndx.Max();
            foreach (ComponentBasic c in newItems)
            {
                switch (c.CompType)
                {
                    case ComponentTypes.ctNode:
                        {
                            (c as ComponentNode).NodeIndex = nodeIndexMax + 1;
                            nodeIndexMax++;
                            break;
                        }
                    case ComponentTypes.ctLinear:
                        {
                            (c as ComponentLinear).BeamIndex = beamIndexMax + 1;
                            beamIndexMax++;
                            break;
                        }
                }
            }
        }

        private void RestoreCurrentHistory()
        {
            if ((HistoryPosition > -1) && (HistoryPosition < ChangesHistory.Count))
            {
                ObservableCollection<ComponentBasic> h = ChangesHistory.ElementAt(HistoryPosition).Value;
                Stream ms = Save(h);
                DisableChangeTracking();
                Load(ms, CompsList);
                EnableChangeTracking();
            };
        }

        public void Undo()
        {
            HistoryPosition--;
            RestoreCurrentHistory();
        }
        
        public void Redo()
        {
            HistoryPosition++;
            RestoreCurrentHistory();
        }

        public void EnableChangeTracking()
        {
            TrackChanges = true;
        }

        public void DisableChangeTracking()
        {
            TrackChanges = false;
        }

        private List<IssoBinding> FindBindings(ComponentNode node, IssoBinding Except)
        {
            List<IssoBinding> result = new List<IssoBinding>();
            for (int i = 0; i < CompsList.Count; i++)
                if ((Except != CompsList[i]) && (CompsList[i].CompType == ComponentTypes.ctBinding))
                {
                    IssoBinding b = (IssoBinding)CompsList[i];
                    if ((node == b.Source) || (node == b.Target))
                    {
                        result.Add(b);
                    }
                }
            return result;
        }

        internal float[] GetBounds()
        {
            List<ComponentNode> nodes = ModelNodes;
            float[] bounds = new float[4];
            bounds[0] = float.MaxValue;
            bounds[1] = bounds[0];
            bounds[2] = float.MinValue;
            bounds[3] = bounds[2];
            for (int i = 0; i < nodes.Count; i++)
            {
                if (bounds[0] > nodes[i].Location.X) bounds[0] = nodes[i].Location.X;
                if (bounds[1] > nodes[i].Location.Y) bounds[1] = nodes[i].Location.Y;
                if (bounds[2] < nodes[i].Location.X) bounds[2] = nodes[i].Location.X;
                if (bounds[3] < nodes[i].Location.Y) bounds[3] = nodes[i].Location.Y;
            }
            return bounds;
        }

        internal bool PreprocessBindingChange(IssoBinding b, float newDX, float newDY)
        {
            // Фактически модель - это система точек на плоскости
            // Между некоторыми из них установлены связи в виде размеров
            // Когда меняется значение отдельного размера или добавляется новый,
            // требуется преобразовать координаты точек (сдвинуть линейно по вертикали или горизонтали)
            // Точки, которые не связаны никакими ограничениями, из рассмотрения исключаются
            List<IssoBinding> bin = Bindings;
            List<ComponentNode> nodes = ModelNodes;
            int nodecnt = ModelNodes.Count;
            ndX = new float[nodecnt, nodecnt];
            ndY = new float[nodecnt, nodecnt];
            for (int y = 0; y < nodecnt; y++) 
                for (int x = 0; x < nodecnt; x++)
                {
                    ndX[x, y] = float.MaxValue;
                    ndY[x, y] = float.MaxValue;
                    if (x == y)
                    {
                        ndX[x, y] = 0;
                        ndY[x, y] = 0;
                    }
                }

            for (int i = 0; i < bin.Count; i++)
            {
                // Пропускаем устанавливаемую связь
                if (b == bin[i]) continue;

                ComponentNode nd1 = bin[i].Source;
                ComponentNode nd2 = bin[i].Target;
                int nd1i = nodes.IndexOf(nd1);
                int nd2i = nodes.IndexOf(nd2);
                switch (bin[i].Type)
                {
                    case IssoBindingType.Horizontal:
                        {
                            ndX[nd1i, nd2i] = Math.Sign(nd2.Location.X - nd1.Location.X) * bin[i].Value; 
                            ndX[nd2i, nd1i] = -ndX[nd1i, nd2i];
                            break;
                        }
                    case IssoBindingType.Vertical:
                        {
                            ndY[nd1i, nd2i] = Math.Sign(nd2.Location.Y - nd1.Location.Y) * bin[i].Value;
                            ndY[nd2i, nd1i] = -ndY[nd1i, nd2i];
                            break;
                        }                   
                }
            }

            // Теперь пробуем изменить расстояние между src и tgt
            if (LocationFixed(b.Type, nodes.IndexOf(b.Source), nodes.IndexOf(b.Target), new List<int>())) return false;
            else
            {
                RecoursiveMove(nodes, b.Source, b.Target, newDX, newDY);
                return true;
            }
        }

        internal bool NodeSingle(ComponentNode n2)
        {
            int cnt = 0;
            for (int i = 0; i < CompsList.Count; i++)
            {
                if (CompsList[i].CompType == ComponentTypes.ctLinear)
                {
                    ComponentLinear l = (ComponentLinear)CompsList[i];
                    if ((l.StartNode == n2) || (l.EndNode == n2)) cnt++;
                }
            }
            return cnt == 1;
        }

        private bool LocationFixed(IssoBindingType bType, int si, int di, List<int> chkd)
        {
            switch (bType)
            {
                case IssoBindingType.Horizontal: if (ndX[si, di] != float.MaxValue) return true; break;
                case IssoBindingType.Vertical: if (ndY[si, di] != float.MaxValue) return true; break;
            }
            chkd.Add(si);

            // Пытаемся обнаружить, есть ли связь между si и di через какие-либо другие узлы
            for (int i = 0; i < Math.Sqrt(ndX.Length); i++)
            {
                if (chkd.IndexOf(i) > -1) continue;

                float dst = 0;
                switch (bType)
                {
                    case IssoBindingType.Horizontal: dst = ndX[si, i]; break;
                    case IssoBindingType.Vertical: dst = ndY[si, i]; break;                        
                }
                if (dst != float.MaxValue)
                {
                    bool fix = LocationFixed(bType, i, di, chkd);
                    if (fix) return true;
                }
            }

            return false;
        }

        public void RecoursiveMove(List<ComponentNode> nodes, ComponentNode src, ComponentNode tgt, float dx, float dy)
        {
            int srci = nodes.IndexOf(src);
            int tgti = nodes.IndexOf(tgt);

            // Теперь пробуем изменить расстояние между src и tgt
            // Изменим координату tgt  
            float newx, newy;
            if (dx == float.MaxValue)
                newx = tgt.Location.X;
            else newx = src.Location.X + dx;

            if (dy == float.MaxValue)
                newy = tgt.Location.Y;
            else newy = src.Location.Y + dy;

            tgt.MoveTo(new IssoPoint2D() { X = newx, Y = newy });

            // Теперь пересчитаем координаты всех точек, связаных с tgt
            for (int x = 0; x < nodes.Count; x++)
            {
                float sdx = ndX[tgti, x]; 
                float sdy = ndY[tgti, x];
                if (sdx == float.MaxValue) sdx = nodes[x].Location.X - tgt.Location.X;
                if (sdy == float.MaxValue) sdy = nodes[x].Location.Y - tgt.Location.Y;
                if ((x != tgti) && (x != srci))
                {
                    if ((nodes[x].Location.X - tgt.Location.X == sdx) &&
                        (nodes[x].Location.Y - tgt.Location.Y == sdy))
                        continue;
                        else RecoursiveMove(nodes, tgt, nodes[x], sdx, sdy);
                }
            }
        }

        internal int getNodeIndex(ComponentNode n)
        {
            return Nodes.FindIndex(x => x == n);
        }

        internal void CalculateStatic()
        {
            // Статический расчёт!

            // 0. Разбиваем модель на элементы:
            //    Незагруженные распределённой нагрузкой стержни берём целиком,
            //    в противном случае - делим стержень на несколько элементов  
            // 1. Создаём список узлов           
            Nodes.Clear();
            for (int i = 0; i < CompsList.Count; i++)
            {
                if (CompsList[i].CompType == ComponentTypes.ctNode) Nodes.Add((ComponentNode)CompsList[i]);
            }
            // В соответствии с количеством узлов создаём матрицу жёсткости системы и заполняем её нулями
            // Для каждого узла - три возможных перемещения
            // 0 - горизонтальное
            // 1 - вертикальное
            // 2 - поворот
            GlobalMatrix = new double[Nodes.Count * 3, Nodes.Count * 3];
            for (int i = 0; i < Nodes.Count; i++)
                for (int j = 0; j < Nodes.Count; j++) GlobalMatrix[i, j] = 0;

            // 2. Создаём балочные элементы и заполняем глобальную матрицу
            Rods.Clear();
            for (int i = 0; i < CompsList.Count; i++)
            {
                if (CompsList[i].CompType == ComponentTypes.ctLinear)
                {
                    ComponentNode n1 = ((ComponentLinear)CompsList[i]).StartNode;
                    ComponentNode n2 = ((ComponentLinear)CompsList[i]).EndNode;
                    int n1i = getNodeIndex(n1);
                    int n2i = getNodeIndex(n2);
                    ElementBeam b = new ElementBeam((ComponentLinear)CompsList[i], this);
                    Rods.Add(b);
                    // Встраиваем матрицу жёсткости элемента в глобальную матрицу жёсткости
                    for (int row = 0; row < 6; row++)
                        for (int col = 0; col < 6; col++)
                        {
                            int r, c;
                            if (row < 3) r = n1i * 3 + row; else r = n2i * 3 + row - 3;
                            if (col < 3) c = n1i * 3 + col; else c = n2i * 3 + col - 3;
                            GlobalMatrix[r, c] += b.ExtMatrix[row, col];
                        }
                }
            }

            // 3. Создаём вектор узловых сил. 
            Loads = new double[Nodes.Count * 3];
            for (int i = 0; i < Loads.Length; i++) Loads[i] = 0;
            for (int i = 0; i < CompsList.Count; i++)
            {
                if (CompsList[i].CompType == ComponentTypes.ctForce)
                {
                    ComponentLoad load = (ComponentLoad)CompsList[i]; 
                    ComponentNode n = load.AppNodes[0];
                    // Сила приложена к узлу, её нужно разложить на 
                    // две составляющие в глобальной системе координат
                    // - горизонтальную и вертикальную
                    // Поскольку в одном узле может быть несколько сил - 
                    // складываем их
                    double force = load.Value;
                    double cosa = Math.Cos(load.Direction / 180 * Math.PI);
                    double sina = Math.Sin(load.Direction / 180 * Math.PI);
                    double forceX = force * cosa;
                    double forceY = force * sina;
                    int ind = getNodeIndex(n) * 3;
                    // При этом, если в узле есть связь, запрещающая перемещения по указанному направлению, то
                    // соответствующую часть силы обнуляем
                    if (ind > -1)
                    {
                        // Сила по X
                        if (!n.DisallowedDisplacements.Contains(NodeDisplacement.X)) Loads[ind] += forceX;
                        // Сила по Y
                        if (!n.DisallowedDisplacements.Contains(NodeDisplacement.Y)) Loads[ind + 1] += forceY;
                    }
                }

                if (CompsList[i].CompType == ComponentTypes.ctDistributedLoad)
                {
                    // Распределённую нагрузку приводим к узловой
                    ComponentLoad load = (ComponentLoad)CompsList[i];
                    ComponentLinear lc = load.Beam;
                    // Найдём элемент, созданный на основе компонента lc и сообщим ему,
                    // что на нём лежит нагрузка load
                    ElementBeam beam = Rods.Find(b => b.Linear == lc);
                    if (beam != null)
                    {
                        beam.CalcEquivalentlReactions(load);
                        int ind = getNodeIndex(lc.StartNode) * 3;
                        if (ind > -1)
                        {
                            // Сила по X
                            if (!lc.StartNode.DisallowedDisplacements.Contains(NodeDisplacement.X)) Loads[ind] += beam.Rx1g;
                            // Сила по Y
                            if (!lc.StartNode.DisallowedDisplacements.Contains(NodeDisplacement.Y)) Loads[ind + 1] += beam.Ry1g;
                            // Момент
                            if (!lc.StartNode.DisallowedDisplacements.Contains(NodeDisplacement.Rotation)) Loads[ind + 2] += beam.M1g;
                        }
                        ind = getNodeIndex(lc.EndNode) * 3;
                        if (ind > -1)
                        {
                            // Сила по X
                            if (!lc.EndNode.DisallowedDisplacements.Contains(NodeDisplacement.X)) Loads[ind] += beam.Rx2g;
                            // Сила по Y
                            if (!lc.EndNode.DisallowedDisplacements.Contains(NodeDisplacement.Y)) Loads[ind + 1] += beam.Ry2g;
                            // Момент
                            if (!lc.EndNode.DisallowedDisplacements.Contains(NodeDisplacement.Rotation)) Loads[ind + 2] += beam.M2g;
                        }
                    }
                }
            }
            
            // 4. Применяем ограничения
            for (int i = 0; i < CompsList.Count; i++)
            {
                if (CompsList[i].CompType == ComponentTypes.ctNode)
                {
                    Block((ComponentNode)CompsList[i]);
                }
            }
            // 5. Получаем перемещения узлов в глобальной системе координат            
            double[,] GlobalInverted = StarMath.inverse(GlobalMatrix);
            Displacements = StarMath.multiply(GlobalInverted, Loads);
            // "Подчистим" перемещения от сверхмалых значений
            CleanupDisplacements();
            // Определим значения реакций в узлах - они же внутренние усилия
            for (int i = 0; i < Rods.Count; i++)
            {
                Rods[i].CalculateForces();
            }
        }

        private void CleanupDisplacements()
        {
            // Все перемещения округляем до 0.0001
            for (int i = 0; i < Displacements.Length; i++)
                Displacements[i] = Math.Round(Displacements[i], 5);            
        }

        public void Block(ComponentNode n)
        {
            int i = getNodeIndex(n) * 3;
            bool X = n.DisallowedDisplacements.Contains(NodeDisplacement.X);
            bool Y = n.DisallowedDisplacements.Contains(NodeDisplacement.Y);
            bool R = n.DisallowedDisplacements.Contains(NodeDisplacement.Rotation);
            for (int row = 0; row < Nodes.Count * 3; row++)
            {
                if (X) GlobalMatrix[row, i] = 0;
                if (Y) GlobalMatrix[row, i + 1] = 0;
                if (R) GlobalMatrix[row, i + 2] = 0;                        
            }
            for (int col = 0; col < Nodes.Count * 3; col++)
            {
                if (X) GlobalMatrix[i, col] = 0;
                if (Y) GlobalMatrix[i + 1, col] = 0;
                if (R) GlobalMatrix[i + 2, col] = 0;
            }

            if (X) GlobalMatrix[i, i] = 1;
            if (Y) GlobalMatrix[i + 1, i + 1] = 1;
            if (R) GlobalMatrix[i + 2, i + 2] = 1;
        }

        public Stream Save(ObservableCollection<ComponentBasic> SourceList)
        {
            MemoryStream result = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(result, new XmlWriterSettings() { CloseOutput = false });

            // Структура файла
            // 1. Метаданные:
            //   1.1 Сечения 
            //   1.2 Материалы
            // 2. Узлы, их ограничения и узловые нагрузки
            // 3. Стержни, их свойства сечений и стержневые (распределённые) нагрузки
            xw.WriteStartElement("IssoRodModel");
            xw.WriteAttributeString("xmlns", "x", null, "isso");
            xw.WriteStartElement("Nodes", "isso");
            List<ComponentNode> modelNodes = ModelNodes2(SourceList);
            for (int i = 0; i < modelNodes.Count; i++)
            {
                xw.WriteStartElement("Node", "isso");
                xw.WriteAttributeString("X", "isso", modelNodes[i].Location.X.ToString());
                xw.WriteAttributeString("Y", "isso", modelNodes[i].Location.Y.ToString());
                foreach (NodeDisplacement d in modelNodes[i].DisallowedDisplacements)
                {
                    string disp = "";
                    switch (d)
                    {
                        case NodeDisplacement.X: disp = "DX"; break;
                        case NodeDisplacement.Y: disp = "DY"; break;
                        case NodeDisplacement.Rotation: disp = "Rotation"; break;
                    }
                    xw.WriteAttributeString(disp, "isso", "true");
                }
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            // Сечения
            xw.WriteStartElement("Sections", "isso");
            for (int i = 0; i < CrossSections.Count; i++)
            {
                xw.WriteStartElement("Section", "isso");
                xw.WriteAttributeString("SectionName", "isso", CrossSections[i].SectionName);
                xw.WriteAttributeString("Area", "isso", CrossSections[i].SectionArea.ToString("G"));
                xw.WriteAttributeString("Inertia", "isso", CrossSections[i].SectionInertia.ToString("G"));
                xw.WriteAttributeString("EModulus", "isso", CrossSections[i].MaterialElasticity.ToString("G"));
                xw.WriteAttributeString("Material", "isso", CrossSections[i].MaterialElasticity.ToString("G"));
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            // Стержни
            xw.WriteStartElement("Beams", "isso");
            List<ComponentLinear> beams = ModelBeams2(SourceList);
            for (int i = 0; i < beams.Count; i++)
            {
                xw.WriteStartElement("Beam", "isso");
                xw.WriteAttributeString("StartNode", "isso", modelNodes.IndexOf(beams[i].StartNode).ToString());
                xw.WriteAttributeString("EndNode", "isso", modelNodes.IndexOf(beams[i].EndNode).ToString());
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            // Силы
            xw.WriteStartElement("Forces", "isso");
            List<ComponentLoad> forces = ModelForces2(SourceList);
            for (int i = 0; i < forces.Count; i++)
            {
                xw.WriteStartElement("Force", "isso");
                xw.WriteAttributeString("Node", "isso", modelNodes.IndexOf(forces[i].AppNodes[0]).ToString());
                xw.WriteAttributeString("Direction", "isso", forces[i].Direction.ToString());
                xw.WriteAttributeString("Value", "isso", forces[i].Value.ToString());
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            // Распределённые нагрузки
            List<ComponentLoad> loads = ModelDistLoads2(SourceList);
            xw.WriteStartElement("DistributedLoads", "isso");
            for (int i = 0; i < loads.Count; i++)
            {
                xw.WriteStartElement("DistributedLoad", "isso");
                xw.WriteAttributeString("Beam", "isso", beams.IndexOf(loads[i].Beam).ToString());
                xw.WriteAttributeString("Direction", "isso", forces[i].Direction.ToString());
                xw.WriteAttributeString("Value", "isso", forces[i].Value.ToString());
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.Close();

            return result;
        }

        public Stream Save()
        {
            return Save(CompsList);
        }

        public void Load(Stream stream, ObservableCollection<ComponentBasic> DestinationList)
        {
            XmlDocument doc = new XmlDocument();
            stream.Seek(0, SeekOrigin.Begin);
            doc.Load(stream);

            DestinationList.Clear();

            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode n = doc.DocumentElement.ChildNodes[i];
                if (n.Name == "x:Nodes")
                {
                    for (int j = 0; j < n.ChildNodes.Count; j++)
                    {
                        XmlNode node = n.ChildNodes[j];
                        float x = float.Parse(GetAttrValue(node.Attributes["x:X"], "0"));
                        float y = float.Parse(GetAttrValue(node.Attributes["x:Y"], "0"));
                        bool dx = bool.Parse(GetAttrValue(node.Attributes["x:DX"], "false"));
                        bool dy = bool.Parse(GetAttrValue(node.Attributes["x:DY"], "false"));
                        bool r = bool.Parse(GetAttrValue(node.Attributes["x:Rotation"], "false"));
                        ComponentNode cn = new ComponentNode(new IssoPoint2D() { X = x, Y = y });
                        if (dx) cn.DisallowedDisplacements.Add(NodeDisplacement.X);
                        if (dy) cn.DisallowedDisplacements.Add(NodeDisplacement.Y);
                        if (r) cn.DisallowedDisplacements.Add(NodeDisplacement.Rotation);
                        DestinationList.Add(cn);
                    }
                }
                List<ComponentNode> modelNodes = ModelNodes2(DestinationList);
                if (n.Name == "x:Beams")
                {
                    for (int j = 0; j < n.ChildNodes.Count; j++)
                    {
                        XmlNode beam = n.ChildNodes[j];
                        int sn = int.Parse(GetAttrValue(beam.Attributes["x:StartNode"], "0"));
                        int en = int.Parse(GetAttrValue(beam.Attributes["x:EndNode"], "0"));

                        ComponentLinear cl = new ComponentLinear(modelNodes[sn], modelNodes[en], this);
                        DestinationList.Add(cl);
                    }
                }
                if (n.Name == "x:Forces")
                {
                    for (int j = 0; j < n.ChildNodes.Count; j++)
                    {
                        XmlNode force = n.ChildNodes[j];
                        int ni = int.Parse(GetAttrValue(force.Attributes["x:Node"], "0"));
                        float d = float.Parse(GetAttrValue(force.Attributes["x:Direction"], "0"));
                        float v = float.Parse(GetAttrValue(force.Attributes["x:Value"], "0"));

                        ComponentLoad cl = new ComponentLoad(ComponentTypes.ctForce, v, modelNodes[ni]);
                        cl.Direction = d;
                        DestinationList.Add(cl);
                    }
                }
                List<ComponentLinear> modelBeams = ModelBeams2(DestinationList);
                if (n.Name == "x:DistributedLoads")
                {
                    for (int j = 0; j < n.ChildNodes.Count; j++)
                    {
                        XmlNode load = n.ChildNodes[j];
                        int li = int.Parse(GetAttrValue(load.Attributes["x:Beam"], "0"));
                        float d = float.Parse(GetAttrValue(load.Attributes["x:Direction"], "0"));
                        float v = float.Parse(GetAttrValue(load.Attributes["x:Value"], "0"));

                        ComponentLoad cl = new ComponentLoad(ComponentTypes.ctDistributedLoad, v, modelBeams[li]);
                        cl.Direction = d;
                        DestinationList.Add(cl);
                    }
                }
            }
        }

        public void Load(Stream stream)
        {
            Load(stream, CompsList);
        }

        private string GetAttrValue(XmlAttribute xmlAttribute, string default_value)
        {
            if (xmlAttribute != null) return xmlAttribute.Value; else return default_value;
        }
    }
}
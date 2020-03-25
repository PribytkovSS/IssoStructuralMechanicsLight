using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;


namespace IssoStMechLight.Models
{
    public class ComponentLinear: ComponentBasic
    {
        private RodModel Model;

        public IssoPoint2D Start
        {
            get
            {
                return node1.Location;
            }
            set
            {
                node1.MoveTo(value);
            }
        }
        public IssoPoint2D End
        {
            get
            {
                return node2.Location;
            }
            set
            {
                node2.MoveTo(value);
            }
        }

        public float Length
        {
            get
            {
                return IssoDist.PointDst(Start, End);
            }
        }

        public float Angle
        {
            get
            {
                float dx = End.X - Start.X;
                float dy = End.Y - Start.Y;

                return (float)Math.Atan2(dy, dx);
            }
        }

        public float AngleD
        {
            get
            {
                float dx = End.X - Start.X;
                float dy = End.Y - Start.Y;

                return (float)(Math.Atan2(dy, dx) / Math.PI * 180);
            }
        }

        private ComponentNode node1, node2;

        public bool OnlySelected { get { return !Model.MultipleSelected; } }

        public string Node1Desc
        {
            get
            {
                return node1.Description; 
            }
        }
        public string Node2Desc
        {
            get
            {
                return node2.Description;
            }
        }

        public IssoCrossSection Section { get; set; }
        public double SectionArea { get { if (Section != null) return Section.SectionArea; else return 1; } }
        public double SectionInertia { get { if (Section != null) return Section.SectionInertia; else return 1; } }
        public double MaterialElasticity { get { if (Section != null) return Section.MaterialElasticity; else return 1; } }
        public int ElementSectionId
        {
            get
            {
                return Model.CrossSections.IndexOf(Section);
            }
            set
            {
                if ((value > -1) && (value < Model.CrossSections.Count))
                    Section = Model.CrossSections[value];
                else Section = null;

                foreach (ComponentLinear c in AllSelected)
                {
                    c.Section = Section;
                }
            }
        }
        public ObservableCollection<IssoCrossSection> CrossSections { get { return Model.CrossSections; } }

        public ComponentNode StartNode { get { return node1;  } }
        public ComponentNode EndNode { get { return node2; } }

        protected ComponentState state;

        public override ComponentTypes CompType { get { return ComponentTypes.ctLinear; } }

        public override ComponentState CompState { get { return state; } set { state = value; } }
        
        private IEnumerable<ComponentLinear> AllSelected
        {
            get
            {
                return (from ComponentBasic cl in Model.CompsList
                        where (cl.CompType == ComponentTypes.ctLinear) && (cl.CompState == ComponentState.csSelected)
                        select (ComponentLinear)cl);
            }
        }

        private bool hStart = false;
        public bool HingeStart
        {
            get { return hStart; }
            set
            {
                // Если есть ещё выбранные элементы, то у них тоже меняем значение этого свойства
                foreach (ComponentLinear c in AllSelected)
                {
                    c.hStart = value;
                }
                hStart = value;
            }
        }
        private bool hEnd = false;
        public bool HingeEnd
        {
            get { return hEnd; }
            set
            {
                // Если есть ещё выбранные элементы, то у них тоже меняем значение этого свойства
                foreach (ComponentLinear c in AllSelected)
                {
                    c.hEnd = value;
                }
                hEnd = value;
            }
        }

        public int BeamIndex { get; set; }

        public ComponentLinear(ComponentNode start, ComponentNode end, RodModel model) : base()
        {
            node1 = start; node2 = end;
            state = ComponentState.csNormal;
            Model = model;
            Section = Model.CrossSections[0];
        }

        public ComponentLinear SplitAt(IssoPoint2D pt)
        {
            ComponentNode node = new ComponentNode(pt);
            ComponentLinear lin = new ComponentLinear(node, node2, Model);
            node2 = node;
            return lin;
        }

        public override string ToString()
        {
            return String.Format("Beam StartNode=({0}), EndNode=({1})", StartNode.ToString(), EndNode.ToString());
        }
    }
}

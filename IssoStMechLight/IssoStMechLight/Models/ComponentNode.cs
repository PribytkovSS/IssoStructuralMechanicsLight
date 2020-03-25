using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IssoStMechLight.Models
{
    /*public enum NodeType
    {
        Hinge, Rigid, Point,
        Pin2d, 
        Roller2d, 
        Spring2d, Fixed2d
    } */

    public enum NodeDisplacement
    {
        [Description("Horizontal Displacement")]
        X,
        [Description("Vertical Displacement")]
        Y,
        [Description("Rotation")]
        Rotation
    }

    public class ComponentNode: ComponentBasic
    {
        protected ComponentState state;
        protected IssoPoint2D location;
        public List<NodeDisplacement> DisallowedDisplacements { get; }

        internal void MoveTo(IssoPoint2D value)
        {            
            location = value;
        }

        public override ComponentTypes CompType { get { return ComponentTypes.ctNode; } }
        public override ComponentState CompState { get { return state; } set { state = value; } }
        public IssoPoint2D Location { get { return location; } }
        public double X { get { return Location.X; } }
        public double Y { get { return Location.Y; } }
        public bool Xrestrained
        {
            get
            {
                return DisallowedDisplacements.Contains(NodeDisplacement.X);
            }
            set
            {
                if (value && !Xrestrained) DisallowedDisplacements.Add(NodeDisplacement.X);
                if (!value) DisallowedDisplacements.Remove(NodeDisplacement.X);
            }
        }
        public bool Yrestrained
        {
            get
            {
                return DisallowedDisplacements.Contains(NodeDisplacement.Y);
            }
            set
            {
                if (value && !Yrestrained) DisallowedDisplacements.Add(NodeDisplacement.Y);
                if (!value) DisallowedDisplacements.Remove(NodeDisplacement.Y);
            }
        }
        public bool Rrestrained
        {
            get
            {
                return DisallowedDisplacements.Contains(NodeDisplacement.Rotation);
            }
            set
            {
                if (value && !Rrestrained) DisallowedDisplacements.Add(NodeDisplacement.Rotation);
                if (!value) DisallowedDisplacements.Remove(NodeDisplacement.Rotation);
            }

        }

        public string Description
        {
            get { return string.Format("№{0} ({1:#0.000}; {2:#0.000})", NodeIndex, location.X, location.Y); }            
        }

        public int NodeIndex
        {
            get; set;
        }

        public ComponentNode()
        {
            DisallowedDisplacements = new List<NodeDisplacement>(); 
            location = new IssoPoint2D() { X = 0, Y = 0 };
            state = ComponentState.csNormal;
        }

        public ComponentNode(IssoPoint2D location) : base()
        {
            DisallowedDisplacements = new List<NodeDisplacement>();
            this.location = location;
            state = ComponentState.csNormal;
        }

        public override string ToString()
        {
            return String.Format("Node X={0}, Y={1}", location.X.ToString("F2"), location.Y.ToString("F2"));
        }
    }
}

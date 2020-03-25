using System;
using System.Collections.Generic;
using System.Text;

namespace IssoStMechLight.Models
{
    public class ComponentLoad: ComponentBasic
    {
        protected ComponentState state;
        public ComponentTypes Type;       
        public override ComponentTypes CompType { get  { return Type; } }
        public override ComponentState CompState { get { return state; } set { state = value; } }

        private float LoadValue;
        private ComponentNode node = null;
        private ComponentLinear linear = null;
        private object Component { get { if (node != null) return node; else return linear; } }
       
        public ComponentNode[] AppNodes
        {
            get
            {
                if (Type == ComponentTypes.ctDistributedLoad) return new ComponentNode[] { linear.StartNode, linear.EndNode };
                else return new ComponentNode[] { node };
            }
        }

        private ComponentNode node2
        {
            get
            {
                if (AppNodes.Length > 1) return AppNodes[1]; else return null;
            }
        }

        public ComponentLinear Beam
        {
            get
            {
                return linear;
            }
        }

        public float Value
        {
            get
            {
                if (Type != ComponentTypes.ctDistributedLoad) return LoadValue;
                if (isOrthogonal)
                {
                    if (isReverse) return Math.Abs(LoadValue);
                    else return -Math.Abs(LoadValue);
                }
                else return LoadValue;
            }
            set
            {
                LoadValue = value;
            }
        }


        // Направление действия нагрузки
        // Игнорируется для изгибающего момента 
        // и в случае, когда isOrthogonal = true
        private float direction;
        public float Direction
        {
            get
            {
                if (Type != ComponentTypes.ctDistributedLoad) return direction;
                if (isOrthogonal && (linear != null))
                {
                    return ((float)Math.PI / 2 + linear.Angle) * 180 / (float)Math.PI;
                }
                else return direction;
            }
            set
            {
                direction = value;
            }
        }

        // Указывает, является ли нагрузка перпендикулярной к компоненту (только для распределённой нагрузки)
        public bool isOrthogonal = false;
        // Также для распределённой нагрузки - 
        // используется вместе с isOrthogonal для обозначения того, направлена ли нагрузка к компоненту или от него 
        public bool isReverse = false;

        public ComponentLoad(ComponentTypes LoadType, float value, ComponentNode node)
        {
            Type = LoadType;
            this.node = node;            
            Value = value;
            Direction = 90;
            isOrthogonal = false;
            isReverse = false;
        }

        public ComponentLoad(ComponentTypes LoadType, float value, ComponentLinear linear)
        {
            Type = LoadType;
            this.linear = linear;
            Value = value;
            Direction = 90;
            isOrthogonal = false;
            isReverse = false;
        }

        internal void SetLastNodeAt(IssoPoint2D pt2)
        {
            if (node2 != null) node2.MoveTo(pt2);
        }

        public override string ToString()
        {
            return String.Format("Load {0}, {1} at {2}", CompType.ToString(), Value.ToString("F2"), Component.ToString());
        }
    }
}

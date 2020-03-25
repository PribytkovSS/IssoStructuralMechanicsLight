using System;
using System.Collections.Generic;
using System.Text;

namespace IssoStMechLight.Models
{
    public enum IssoBindingType
    {
        Horizontal, Vertical, Angular
    }
    // Класс, реализующий взаимную увязку объектов модели -
    // расстояния между компонентами, углы наклона и прочие пространственные ограничения
    public class IssoBinding: ComponentBasic
    {
        private ComponentState state;
        public override ComponentTypes CompType { get { return ComponentTypes.ctBinding; } }
        public override ComponentState CompState { get { return state; } set { state = value; } }
        private float Size; // Абсолютное значение расстояния
        private float DimSign; // Знак, показывающий: "+" - значит target правее (выше) source, "-" - наоборот 

        private ComponentNode source, target;

        public readonly IssoBindingType Type;
        public ComponentNode Source { get { return source; } }
        public ComponentNode Target
        {
            get { return target; }
            set
            {
                if (value != null)
                {
                    target = (ComponentNode)value;
                    float dx = target.Location.X - source.Location.X;
                    float dy = target.Location.Y - source.Location.Y;
                    switch (Type)
                    {
                        case IssoBindingType.Horizontal: Size = Math.Abs(dx); DimSign = Math.Sign(dx);  break;
                        case IssoBindingType.Vertical: Size = Math.Abs(dy); DimSign = Math.Sign(dy); break;
                    }
                }
                else target = null;
            }
        }

        internal ComponentNode UpdateNodeLocation(ComponentNode FixedNode)
        {
            ComponentNode node;
            if (Source == FixedNode) node = Target; else node = Source;

            float x = node.Location.X;
            float y = node.Location.Y;

            switch (Type)
            {
                case IssoBindingType.Horizontal:
                    {
                        node.MoveTo(new IssoPoint2D() { X = FixedNode.Location.X + DimSign * Size, Y = node.Location.Y });
                        break;
                    }
                case IssoBindingType.Vertical:
                    {
                        node.MoveTo(new IssoPoint2D() { X = node.Location.X, Y = FixedNode.Location.Y + DimSign * Size });
                        break;
                    }
            }

            if ((node.Location.X != x) || (node.Location.Y != y)) return node; else return null;
        }

        public IssoPoint2D LinePlace;

        public float Value
        {
            get
            {
                if (target == null)
                {
                    switch (Type)
                    {
                        case IssoBindingType.Horizontal: return Math.Abs(LinePlace.X - source.Location.X); 
                        case IssoBindingType.Vertical: return Math.Abs(LinePlace.Y - source.Location.Y);
                        default: return 0;
                    }
                } else return Size;
            }
            set
            {
                Size = value;
                Redim();
            }
        }

        public void Redim()
        {
            if (target != null)
                switch (Type)
                {
                    case IssoBindingType.Horizontal:
                    {
                        target.MoveTo(new IssoPoint2D() { X = source.Location.X + DimSign * Size, Y = target.Location.Y });
                        break;
                    }
                    case IssoBindingType.Vertical:
                    {
                        target.MoveTo(new IssoPoint2D() { X = target.Location.X, Y = source.Location.Y + DimSign * Size });
                        break;
                    }
                }
        }

        public IssoBinding(IssoBindingType t, ComponentNode source, ComponentNode target, float dist)
        {
            Type = t;
            this.source = source;
            this.target = target;
            Value = dist;
            LinePlace = new IssoPoint2D() { X = source.Location.X, Y = source.Location.Y };
        }
    }
}

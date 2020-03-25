using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IssoStMechLight.Models
{
    public enum ComponentTypes
    {
        [Description("Any component")]
        ctAny,
        [Description("Linear component (beam)")]
        ctLinear,
        [Description("Node")]
        ctNode,
        [Description("Dimension")]
        ctBinding,
        [Description("Force")]
        ctForce,
        [Description("Distributed load")]
        ctDistributedLoad,
        [Description("Moment")]
        ctMoment
    }

    public enum ComponentState
    {
        csEdited, csNormal, csSelected
    }

    public abstract class ComponentBasic
    {
        abstract public ComponentTypes CompType { get; }
        abstract public ComponentState CompState { get; set; }              
    }
}

using System;

namespace ThunderNut.WorldGraph.Attributes {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public abstract class ContextFilterableAttribute : Attribute { }

}
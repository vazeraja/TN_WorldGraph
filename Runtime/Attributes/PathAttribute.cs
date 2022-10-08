﻿using System;

namespace ThunderNut.WorldGraph.Attributes {

    /// <summary>
    /// Use this attribute on classes which inherit from AbstractSceneNode.
    /// The last item in the path must be the same name as the class in order for the WorldGraph to recognize it
    /// </summary>
    /// <example>
    /// [Path("Basic/DefaultNode", "Default")]
    /// public class DefaultNode : AbstractSceneNode {}
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class PathAttribute : ContextFilterableAttribute {
        public readonly string path;
        public readonly string dropdownTitle;
        public readonly bool isVisible;

        public PathAttribute(string path, string dropdownTitle, bool isVisible = true) {
            this.path = path;
            this.dropdownTitle = dropdownTitle;
            this.isVisible = isVisible;
        }
    }

    public class RequiresConstantRepaintAttribute : Attribute {
        
    }

}
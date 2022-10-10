using System;

namespace ThunderNut.WorldGraph.Attributes {

    /// <summary>
    /// Use this attribute on classes which inherit from SceneHandle.
    /// The last item in the path must be the same name as the class in order for WorldGraph to recognize it
    /// The second parameter should be the actual title that you want the node to have in the dropdown
    /// </summary>
    /// <example>
    /// [Path("Basic/DefaultNode", "Default")]
    /// public class DefaultNode : AbstractSceneNode {}
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class PathAttribute : WSGBaseAttribute {
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
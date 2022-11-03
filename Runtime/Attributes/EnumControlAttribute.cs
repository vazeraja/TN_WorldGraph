using System;
using System.Reflection;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Attributes {

    public interface IControlAttribute {
        VisualElement InstantiateControl(SceneHandle node, PropertyInfo propertyInfo);
    }

    [AttributeUsage(AttributeTargets.Property)]
    class EnumControlAttribute : Attribute, IControlAttribute {
        string m_Label;

        public EnumControlAttribute(string label = null) {
            m_Label = label;
        }

        public VisualElement InstantiateControl(SceneHandle node, PropertyInfo propertyInfo) {
            return new EnumControlView(m_Label, node, propertyInfo);
        }
    }

    class EnumControlView : VisualElement {
        SceneHandle m_Node;
        PropertyInfo m_PropertyInfo;

        public EnumControlView(string label, SceneHandle node, PropertyInfo propertyInfo) {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/Controls/EnumControlView"));
            m_Node = node;
            m_PropertyInfo = propertyInfo;
            if (!propertyInfo.PropertyType.IsEnum)
                throw new ArgumentException("Property must be an enum.", "propertyInfo");
            Add(new Label(label ?? ObjectNames.NicifyVariableName(propertyInfo.Name)));
            var enumField = new EnumField((Enum) m_PropertyInfo.GetValue(m_Node, null));
            enumField.RegisterValueChangedCallback(OnValueChanged);
            Add(enumField);
        }

        void OnValueChanged(ChangeEvent<Enum> evt) {
            var value = (Enum) m_PropertyInfo.GetValue(m_Node, null);
            if (!evt.newValue.Equals(value)) {
                m_PropertyInfo.SetValue(m_Node, evt.newValue, null);
            }
        }
    }

}
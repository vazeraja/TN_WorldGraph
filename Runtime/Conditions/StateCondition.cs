using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class Condition {
        [SerializeReference] public ExposedParameter parameter;
        [SerializeReference] public ConditionValueBase value;
    }

    [Serializable]
    public class StateCondition : Condition {
        public Func<bool> FloatIsGreaterThan() => () => ((FloatParameterField) parameter).Value > ((FloatCondition) value).Value;
        public Func<bool> FloatIsLessThan() => () => ((FloatParameterField) parameter).Value < ((FloatCondition) value).Value;

        public Func<bool> BoolIsTrue() => () => ((BoolParameterField) parameter).Value;
        public Func<bool> BoolIsFalse() => () => !((BoolParameterField) parameter).Value;

        public Func<bool> IntIsGreaterThan() => () => ((IntParameterField) parameter).Value > ((IntCondition) value).Value;
        public Func<bool> IntIsLessThan() => () => ((IntParameterField) parameter).Value < ((IntCondition) value).Value;
        public Func<bool> IntIsEqual() => () => ((IntParameterField) parameter).Value == ((IntCondition) value).Value;
        public Func<bool> IntNotEqual() => () => ((IntParameterField) parameter).Value != ((IntCondition) value).Value;

        public Func<bool> StringIsEqual() => () => ((StringParameterField) parameter).Value == ((StringCondition) value).Value;
        public Func<bool> StringNotEqual() => () => ((StringParameterField) parameter).Value != ((StringCondition) value).Value;
    }

}
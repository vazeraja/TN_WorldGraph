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
        public Func<bool> FloatIsGreaterThan() => () => ((FloatParameter) parameter).Value > ((FloatCondition) value).Value;
        public Func<bool> FloatIsLessThan() => () => ((FloatParameter) parameter).Value < ((FloatCondition) value).Value;

        public Func<bool> BoolIsTrue() => () => ((BoolParameter) parameter).Value;
        public Func<bool> BoolIsFalse() => () => !((BoolParameter) parameter).Value;

        public Func<bool> IntIsGreaterThan() => () => ((IntParameter) parameter).Value > ((IntCondition) value).Value;
        public Func<bool> IntIsLessThan() => () => ((IntParameter) parameter).Value < ((IntCondition) value).Value;
        public Func<bool> IntIsEqual() => () => ((IntParameter) parameter).Value == ((IntCondition) value).Value;
        public Func<bool> IntNotEqual() => () => ((IntParameter) parameter).Value != ((IntCondition) value).Value;

        public Func<bool> StringIsEqual() => () => ((StringParameter) parameter).Value == ((StringCondition) value).Value;
        public Func<bool> StringNotEqual() => () => ((StringParameter) parameter).Value != ((StringCondition) value).Value;
    }

}
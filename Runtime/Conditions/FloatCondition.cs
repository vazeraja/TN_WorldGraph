using System;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class FloatCondition : ConditionValue<float> {
        public FloatParamOptions floatOptions;

        public FloatCondition() {
            Value = 0f;
        }
    }

}
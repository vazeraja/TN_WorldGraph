using System;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class BoolCondition : ConditionValue<bool> {
        public BoolParamOptions boolOptions;

        public BoolCondition() {
            Value = true;
        }
    }

}
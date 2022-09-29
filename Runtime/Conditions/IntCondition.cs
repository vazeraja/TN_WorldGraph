using System;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class IntCondition : ConditionValue<int> {
        public IntParamOptions intOptions;

        public IntCondition() {
            Value = 0;
        }
    }

}
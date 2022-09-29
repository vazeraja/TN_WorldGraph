using System;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class StringCondition : ConditionValue<string> {
        public StringParamOptions stringOptions;

        public StringCondition() {
            Value = "string";
        }
    }

}
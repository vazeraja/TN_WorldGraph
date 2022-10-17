using System;

namespace ThunderNut.WorldGraph {

    public enum IntParamOptions {
        GreaterThan,
        LessThan,
        Equals,
        NotEquals,
    }
    public class IntParameter : ParameterField<int> {
        public IntParamOptions options = IntParamOptions.Equals;
        
        private const int DEFAULT_VALUE = 0;
        private const string DEFAULT_NAME = "IntParameter";
        
        public override void Dispose() {
            Value = DEFAULT_VALUE;
            
            base.Dispose();
        }
        
        public IntParameter() {
            GUID = Guid.NewGuid().ToString();
            Name = DEFAULT_NAME;
            Reference = $"_{DEFAULT_NAME}";
            Exposed = true;
            ParameterType = "Int";
            Value = DEFAULT_VALUE;
        }
    }

}
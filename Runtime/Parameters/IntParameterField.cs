using System;

namespace ThunderNut.WorldGraph {

    public enum IntParamOptions {
        GreaterThan,
        LessThan,
        Equals,
        NotEquals,
    }
    public class IntParameterField : ParameterField<int> {
        public IntParamOptions options = IntParamOptions.Equals;
        
        private const int DEFAULT_VALUE = 0;
        private const string DEFAULT_NAME = "FloatParameter";
        
        public override void Dispose() {
            Value = DEFAULT_VALUE;
            
            base.Dispose();
        }
        
        public IntParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = DEFAULT_NAME;
            Reference = $"_{DEFAULT_NAME}";
            Exposed = true;
            ParameterType = "Int";
            Value = DEFAULT_VALUE;
        }
    }

}
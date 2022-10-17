using System;

namespace ThunderNut.WorldGraph {
    
    public enum StringParamOptions {
        Equals,
        NotEquals,
    }
    
    public class StringParameter : ParameterField<string> {
        private const string DEFAULT_VALUE = "Default_Value";
        private const string DEFAULT_NAME = "StringParameter";
        
        public StringParamOptions options = StringParamOptions.Equals;

        public override void Dispose() {
            Value = DEFAULT_VALUE;
            
            base.Dispose();
        }

        public StringParameter() {
            GUID = Guid.NewGuid().ToString();
            Name = DEFAULT_NAME;
            Reference = $"_{DEFAULT_NAME}";
            Exposed = true;
            ParameterType = "String";
            Value = DEFAULT_VALUE;
        }
    }

}
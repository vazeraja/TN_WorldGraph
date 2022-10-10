using System;

namespace ThunderNut.WorldGraph {
    
    public enum BoolParamOptions {
        True,
        False,
    }
    
    public class BoolParameterField : ParameterField<bool> {
        public BoolParamOptions options = BoolParamOptions.True;
        
        private const bool DEFAULT_VALUE = false;
        private const string DEFAULT_NAME = "FloatParameter";
        
        public override void Dispose() {
            Value = DEFAULT_VALUE;
            
            base.Dispose();
        }
        
        public BoolParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = DEFAULT_NAME;
            Reference = $"_{DEFAULT_NAME}";
            Exposed = true;
            ParameterType = "Bool";
            Value = DEFAULT_VALUE;
        }
        
    }

}
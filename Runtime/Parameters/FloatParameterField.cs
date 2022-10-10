using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public enum FloatParamOptions {
        GreaterThan,
        LessThan
    }
    public class FloatParameterField : ParameterField<float> {
        public FloatParamOptions options = FloatParamOptions.GreaterThan;
        
        private const float DEFAULT_VALUE = 0f;
        private const string DEFAULT_NAME = "FloatParameter";
        
        public override void Dispose() {
            Value = DEFAULT_VALUE;
            
            base.Dispose();
        }
        
        public FloatParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = DEFAULT_NAME;
            Reference = $"_{DEFAULT_NAME}";
            Exposed = true;
            ParameterType = "Float";
            Value = DEFAULT_VALUE;
        }
    }

}
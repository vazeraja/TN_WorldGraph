﻿using System;

namespace ThunderNut.WorldGraph {
    
    public enum BoolParamOptions {
        True,
        False,
    }
    
    [Serializable]
    public class BoolParameterField : ParameterField<bool> {
        public BoolParamOptions options = BoolParamOptions.True;
        
        public BoolParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "BoolParameter";
            Reference = "_BoolParameter";
            Exposed = true;
            ParameterType = "Bool";
            Value = true;
        }
        
    }

}
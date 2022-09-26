﻿using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public enum FloatParamOptions {
        GreaterThan,
        LessThan
    }
    [Serializable]
    public class FloatParameterField : ParameterField<float> {
        public FloatParamOptions options = FloatParamOptions.GreaterThan;

        public FloatParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "FloatParameter";
            Reference = "_FloatParameter";
            Exposed = true;
            ParameterType = "Float";
            Value = 69f;
        }
    }

}
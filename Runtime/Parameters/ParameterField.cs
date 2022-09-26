using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public abstract class ParameterField<TValueType> : ExposedParameter {
        public TValueType Value;
    }

}
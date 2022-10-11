using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [Serializable]
    public struct Optional<T> {
        [SerializeField] private bool enabled;
        [SerializeField] private T value;

        public Optional(T initialValue){
            enabled = true;
            value = initialValue;
        }
        public Optional(bool enabled){
            this.enabled = enabled;
            value = default;
        }
        public Optional(T initialValue, bool enabled){
            this.enabled = enabled;
            value = initialValue;
        }

        public bool Enabled { get => enabled; set => enabled = value; }
        public T Value { get => value; set => this.value = value; }
    }
}

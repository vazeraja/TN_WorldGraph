using System;
using UnityEngine.Events;

public interface IInputProvider {
    public event Action<float> onJump;
    public event Action<float> onDash;
    public InputState GetState();
}
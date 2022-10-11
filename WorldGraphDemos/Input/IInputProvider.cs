using UnityEngine.Events;
public interface IInputProvider {
    
    public event UnityAction<float> onJump;
    public event UnityAction<float> onDash;
    public InputState GetState();
}
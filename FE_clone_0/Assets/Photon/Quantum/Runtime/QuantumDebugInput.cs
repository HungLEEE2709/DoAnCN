namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDebugInput : MonoBehaviour {

    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    /// <summary>
    /// Set an empty input when polled by the simulation.
    /// </summary>
    /// <param name="callback"></param>
    public void PollInput(CallbackPollInput callback) {
      Quantum.Input input = new Quantum.Input();

      FP x = 0;
      FP y = 0;

      if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) {
        x = 1;
      } else if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) {
        x = -1;
      }

      if (UnityEngine.Input.GetKey(KeyCode.UpArrow)) {
        y = 1;
      } else if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) {
        y = -1;
      }

      input.Direction = new FPVector2(x, y);
      input.Attack = UnityEngine.Input.GetKey(KeyCode.Space);
      callback.SetInput(input, DeterministicInputFlags.Repeatable);

    }
  }
}

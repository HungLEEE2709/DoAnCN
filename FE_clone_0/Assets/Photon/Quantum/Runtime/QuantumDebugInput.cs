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
#if DEBUG
      if (callback.IsInputSet) {
        Debug.LogWarning($"{nameof(QuantumDebugInput)}.{nameof(PollInput)}: Input was already set by another user script, unsubscribing from the poll input callback. Please delete this component.", this);
        QuantumCallback.UnsubscribeListener(this);
        return;
      }
#endif

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

      callback.SetInput(input, DeterministicInputFlags.Repeatable);

    }
  }
}

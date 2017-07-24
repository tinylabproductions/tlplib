using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.GyroInput {
  public class GyroOutput : MonoBehaviour, IMB_Update {
#pragma warning disable 649
    [SerializeField] Text text;
#pragma warning restore 649

    public void Update() {
      var gyro = Input.gyro;
      text.text =
        $"Gyro attitude {gyro.attitude}\n" +
        $"Gyro rotation rate unbiased {gyro.rotationRateUnbiased}\n" +
        $"Gyro offset {GyroOffset.instance.offset}";
    }
  }
}
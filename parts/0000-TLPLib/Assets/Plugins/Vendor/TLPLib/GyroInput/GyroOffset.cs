using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.GyroInput {
  public class GyroOffset {
    public static readonly GyroOffset instance = new GyroOffset();

    readonly Gyroscope gyro = Input.gyro;
    public Vector2 offset { get; private set; } = Vector2.zero;
    public Vector2 axisLocks = new Vector2(1, 1);
    public float speed = 10;
    public float friction = 0.01f;

    GyroOffset() {
      enabled = true;
      ASync.EveryFrame(() => {
        if (gyro.enabled) calculateOffset(gyro);
        return true;
      });
    }

    public bool enabled {
      get { return gyro.enabled; }
      set {
        gyro.enabled = value;
        if (!value) offset = Vector2.zero;
      }
    }

    void calculateOffset(Gyroscope gyro) {
      // We get gyro rotation rate in radians / sec
      var gyroRate = gyro.rotationRateUnbiased;
      // We sum the offsets as we want them to be applied frame after frame
      // We multiply by timeDelta to make sure it is stable across the time frame
      // We multiply by Rad2Deg as we want to translate radians into degrees for calculations to be correct
      offset += new Vector2(-gyroRate.z, -gyroRate.x) * Time.deltaTime;
      // We clamp the offset to certain values to make sure to lock the camera's paralax effect
      // Only to a certain sphere area
      offset = new Vector2(
        Mathf.Clamp(offset.x, -axisLocks.x, axisLocks.x),
        Mathf.Clamp(offset.y, -axisLocks.y, axisLocks.y)
      );

      offset = Vector2.Lerp(offset, Vector2.zero, friction);
    }
  }
}
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.functional;
using UnityEngine;
using Coroutine = com.tinylabproductions.TLPLib.Concurrent.Coroutine;

namespace com.tinylabproductions.TLPLib.GyroInput {
  public class GyroOffset {
    public static readonly GyroOffset instance = new GyroOffset();

    readonly Gyroscope gyro = Input.gyro;
    public Vector2 offset { get; private set; } = Vector2.zero;
    public Vector2 axisLocks = Vector2.one;
    public float friction = 0.01f;

    Option<Coroutine> updateCoroutine = None._;

    GyroOffset() {
      // Does side effects.
      enabled = true;
    }

    bool _enabled;
    public bool enabled {
      get { return _enabled; }
      set {
        _enabled = value;

        if (value) {
          // Make sure gyro is enabled. It is needed for us to work.
          gyro.enabled = true;
          if (updateCoroutine.isNone) updateCoroutine = ASync.EveryFrame(update).some();
        }
        else {
          offset = Vector2.zero;
          foreach (var c in updateCoroutine) {
            c.stop();
            updateCoroutine = None._;
          }
        }
      }
    }

    bool update() {
      if (gyro.enabled) calculateOffset(gyro);
      return true;
    }

    void calculateOffset(Gyroscope gyro) {
      // We get gyro rotation rate in radians / sec
      var gyroRate = gyro.rotationRateUnbiased;
      // We sum the offsets as we want them to be applied frame after frame
      offset += new Vector2(gyroRate.y, -gyroRate.x) * Time.deltaTime;
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
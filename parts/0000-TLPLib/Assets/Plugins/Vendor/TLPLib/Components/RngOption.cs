using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  /// <summary>
  /// Quick way to add a random number generator with settable seed to a script.
  /// 
  /// Example usage:
  /// 
  /// <code><![CDATA[
  /// [SerializeField] RngOption random;
  /// 
  /// public override void onEvent() => 
  ///   events[random.rng.nextIntInRange(new Range(0, events.Length), out random.__rng)].onEvent();
  /// ]]></code>
  /// </summary>
  [Serializable]
  public class RngOption {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField] UnityOptionULong randomSeed;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    /// <summary>
    /// Only public because we can't use out with properties.
    /// 
    /// Do not get this field directly!
    /// </summary>
    public Rng __rng;

    public Rng rng { get {
      if (!__rng.isInitialized) {
        __rng = randomSeed.isSome
          ? new Rng(new Rng.Seed(randomSeed.__unsafeGetValue))
          : RngSeeder.global.nextRng();
      }
      return __rng;
    } }
  }
}
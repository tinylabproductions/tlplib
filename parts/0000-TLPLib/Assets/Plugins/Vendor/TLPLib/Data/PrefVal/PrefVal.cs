using System;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary><see cref="PrefVal{A}"/> that can be inspected in editor.</summary>
  public interface InspectablePrefVal {
    object valueUntyped { get; set; }
    void forceSave();
  }

  /// <summary>PlayerPrefs backed value.</summary>
  public interface PrefVal<A> : IRxRef<A>, ICachedBlob<A>, InspectablePrefVal {}

  public static class PrefVal {
    public delegate void Base64StorePart(byte[] partData);
    public delegate byte[] Base64ReadPart();

    public enum OnDeserializeFailure { ReturnDefault, ThrowException }

    public static readonly PrefValStorage player = new PrefValStorage(PlayerPrefsBackend.instance);
#if UNITY_EDITOR
    public static readonly PrefValStorage editor = new PrefValStorage(EditorPrefsBackend.instance);
#endif

    public static void trySetUntyped<A>(this PrefVal<A> val, object value) {
      if (value is A a)
        val.value = a;
      else
        throw new ArgumentException(
          $"Can't assign {value} (of type {value.GetType()}) to {val} (of type {typeof(A)}!"
        );
    }
  }
}
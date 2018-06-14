using System;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary><see cref="PrefVal{A}"/> that can be inspected in editor.</summary>
  public interface InspectablePrefVal {
    [PublicAPI] object valueUntyped { get; set; }
    /// <summary>
    /// Writes <see cref="PrefVal{A}"/> to disk upon calling. Normally Unity saves on
    /// application exit, but you can use this to force flushing data to the disk.
    ///
    /// Beware, this is very slow on some platforms (for example iOS).
    /// </summary>
    [PublicAPI] void save();
  }

  /// <summary>PlayerPrefs backed value.</summary>
  public interface PrefVal<A> : IRxRef<A>, ICachedBlob<A>, InspectablePrefVal {}

  public static class PrefVal {
    [PublicAPI] public delegate void Base64StorePart(byte[] partData);
    [PublicAPI] public delegate byte[] Base64ReadPart();

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
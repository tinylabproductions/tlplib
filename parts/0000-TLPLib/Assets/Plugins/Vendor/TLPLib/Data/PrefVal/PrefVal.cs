using System;
using System.Text;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  /* PlayerPrefs backed reactive value. */
  public interface PrefVal<A> : Ref<A>, ICachedBlob<A> {
    void forceSave();
  }
  
  public static class PrefVal {
    public delegate void Base64StorePart(byte[] partData);
    public delegate byte[] Base64ReadPart();

    public enum OnDeserializeFailure { ReturnDefault, ThrowException }
    public enum OnDeserializeCollectionItemFailure { Ignore, ThrowException }

    public static readonly PrefValStorage player = new PrefValStorage(PlayerPrefsBackend.instance);
#if UNITY_EDITOR
    public static readonly PrefValStorage editor = new PrefValStorage(EditorPrefsBackend.instance);
#endif
  }
}
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Data {
  // Should be class (not struct) because .write mutates object.
  class PrefValImpl<A> : PrefVal<A> {
    public readonly bool saveOnEveryWrite;
    public readonly string key;

    readonly IPrefValueBackend backend;
    readonly IPrefValueWriter<A> writer;

    A _value;
    public A value {
      get { return _value; }
      set {
        if (EqComparer<A>.Default.Equals(_value, value)) return;
        _value = persist(value);
      }
    }

    A persist(A value) {
      writer.write(backend, key, value);
      if (saveOnEveryWrite) backend.save();
      return value;
    }

    public PrefValImpl(
      string key, IPrefValueRW<A> rw, A defaultVal,
      IPrefValueBackend backend, bool saveOnEveryWrite
    ) {
      this.key = key;
      writer = rw;
      this.backend = backend;
      this.saveOnEveryWrite = saveOnEveryWrite;
      _value = persist(rw.read(backend, key, defaultVal));
    }

    public void forceSave() => backend.save();

    public override string ToString() => $"{nameof(PrefVal<A>)}({_value})";

    #region ICachedBlob

    public bool cached => true;
    Option<Try<A>> ICachedBlob<A>.read() => F.some(F.scs(value));

    public Try<Unit> store(A data) {
      value = data;
      return F.scs(F.unit);
    }

    public Try<Unit> clear() {
      backend.delete(key);
      return F.scs(F.unit);
    } 

    #endregion
  }
}
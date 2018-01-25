using System;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Data {
  // Should be class (not struct) because .write mutates object.
  class PrefValImpl<A> : PrefVal<A> {
    public readonly bool saveOnEveryWrite;
    public readonly string key;

    readonly IPrefValueBackend backend;
    readonly IPrefValueWriter<A> writer;
    readonly IRxRef<A> rxRef;
    
    // ReSharper disable once NotAccessedField.Local
    // To keep the subscription alive.
    readonly ISubscription persistSubscription;

    public A value {
      get => rxRef.value;
      set => rxRef.value = value;
    }

    public object valueUntyped {
      get => value;
      set => this.trySetUntyped(value);
    }

    void persist(A value) {
      writer.write(backend, key, value);
      if (saveOnEveryWrite) backend.save();
    }

    public PrefValImpl(
      string key, IPrefValueRW<A> rw, A defaultVal,
      IPrefValueBackend backend, bool saveOnEveryWrite
    ) {
      this.key = key;
      writer = rw;
      this.backend = backend;
      this.saveOnEveryWrite = saveOnEveryWrite;
      rxRef = RxRef.a(rw.read(backend, key, defaultVal));
      persistSubscription = rxRef.subscribe(NoOpDisposableTracker.instance, persist);
    }

    public void forceSave() => backend.save();

    public override string ToString() => $"{nameof(PrefVal<A>)}({value})";

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

    #region IRxRef

    public int subscribers => rxRef.subscribers;
    public ISubscription subscribe(
      IDisposableTracker tracker, Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "", 
      [CallerFilePath] string callerFilePath = "", 
      [CallerLineNumber] int callerLineNumber = 0
    ) => 
      rxRef.subscribe(
        tracker: tracker, onEvent: onEvent, 
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );

    public ISubscription subscribeWithoutEmit(
      IDisposableTracker tracker, Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "", 
      [CallerFilePath] string callerFilePath = "", 
      [CallerLineNumber] int callerLineNumber = 0
    ) =>
      rxRef.subscribeWithoutEmit(
        tracker: tracker, onEvent: onEvent, 
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );

    #endregion
  }
}
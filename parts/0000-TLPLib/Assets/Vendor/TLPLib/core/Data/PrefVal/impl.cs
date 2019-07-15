using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.functional;
using pzd.lib.serialization;

namespace com.tinylabproductions.TLPLib.Data {
  // Should be class (not struct) because .write mutates object.
  class PrefValImpl<A> : PrefVal<A> {
    readonly string key;

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

    void persist(A value) => writer.write(backend, key, value);

    public PrefValImpl(
      string key, IPrefValueRW<A> rw, A defaultVal,
      IPrefValueBackend backend
    ) {
      this.key = key;
      writer = rw;
      this.backend = backend;
      rxRef = RxRef.a(rw.read(backend, key, defaultVal));
      persistSubscription = rxRef.subscribe(NoOpDisposableTracker.instance, persist);
    }

    public void save() => backend.save();

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
      IDisposableTracker tracker, Action<A> onEvent,
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

    public void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) =>
      rxRef.subscribe(
        tracker: tracker, onEvent: onEvent, subscription: out subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );

    public ISubscription subscribeWithoutEmit(
      IDisposableTracker tracker, Action<A> onEvent,
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

  class PrefValDictImpl<K, V> : PrefValDictionary<K, V> {
    readonly Dictionary<K, PrefVal<V>> cache = new Dictionary<K, PrefVal<V>>();
    readonly string keyPrefix;
    readonly Func<K, string> keyToString;
    readonly ISerializedRW<V> vRw;
    readonly PrefValStorage storage;
    readonly V defaultValue;
    readonly PrefVal.OnDeserializeFailure onDeserializeFailure;
    readonly ILog log;

    public PrefValDictImpl(
      string keyPrefix, Func<K, string> keyToString, ISerializedRW<V> vRw, PrefValStorage storage, V defaultValue, 
      PrefVal.OnDeserializeFailure onDeserializeFailure, ILog log = null
    ) {
      this.keyPrefix = keyPrefix;
      this.keyToString = keyToString;
      this.vRw = vRw;
      this.storage = storage;
      this.defaultValue = defaultValue;
      this.onDeserializeFailure = onDeserializeFailure;
      this.log = log;
    }

    string stringKey(K key) => $"{keyPrefix}:{keyToString(key)}";

    public bool hasKey(K key) => cache.ContainsKey(key) || storage.hasKey(stringKey(key));

    public PrefVal<V> get(K key) {
      if (cache.TryGetValue(key, out var prefVal)) {
        return prefVal;
      }
      else {
        prefVal = storage.custom(stringKey(key), defaultValue, vRw, onDeserializeFailure, log);
        cache.Add(key, prefVal);
        return prefVal;
      }
    }
  }
}
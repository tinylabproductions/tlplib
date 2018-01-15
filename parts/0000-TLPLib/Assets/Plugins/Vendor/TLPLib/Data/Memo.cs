using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>Memo is a key -> value lazily initialized store.</summary>
  public class Memo<Key, Value> {
    readonly Dictionary<Key, Value> memoized = new Dictionary<Key, Value>();
    readonly Fn<Key, Value> createValue;

    public Memo(Fn<Key, Value> createValue) { this.createValue = createValue; }

    public Value this[Key key] {
      get {
        if (memoized.ContainsKey(key)) return memoized[key];
        else {
          var value = createValue(key);
          memoized.Add(key, value);
          return value;
        }
      }
    }
  }

  public static class Memo {
    public static Memo<Key, Value> a<Key, Value>(Fn<Key, Value> createValue) =>
      new Memo<Key, Value>(createValue);
  }
}
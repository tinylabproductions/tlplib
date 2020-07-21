using System;
using System.Collections.Generic;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Data {
  public class IPrefValueTestBackend : IPrefValueBackend {
    public readonly Dictionary<string, OneOf<string, int, float>> storage =
      new Dictionary<string, OneOf<string, int, float>>();

    public bool hasKey(string name) =>
      storage.ContainsKey(name);

    A get<A>(string name, A defaultValue, Func<OneOf<string, int, float>, Option<A>> select) =>
      storage.get(name).fold(defaultValue, _ => select(_).get);

    public string getString(string name, string defaultValue) =>
      get(name, defaultValue, _ => _.aValue);

    public void setString(string name, string value) =>
      storage[name] = new OneOf<string, int, float>(value);

    public int getInt(string name, int defaultValue) =>
      get(name, defaultValue, _ => _.bValue);

    public void setInt(string name, int value) =>
      storage[name] = new OneOf<string, int, float>(value);

    public float getFloat(string name, float defaultValue) =>
      get(name, defaultValue, _ => _.cValue);

    public void setFloat(string name, float value) =>
      storage[name] = new OneOf<string, int, float>(value);

    public void save() {}

    public void delete(string name) => storage.Remove(name);
  }
}
using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public class IPrefValueTestBackend : IPrefValueBackend {
    public readonly Dictionary<string, OneOf<string, int, float>> storage = 
      new Dictionary<string, OneOf<string, int, float>>();

    A get<A>(string name, A defaultValue, Fn<OneOf<string, int, float>, Option<A>> select) =>
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
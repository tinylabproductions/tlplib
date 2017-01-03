using System;
using System.Reflection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.reflection {
  public static class Type2 {
    public static Either<string, Type> getType(string name) =>
      F.opt(Type.GetType(name)).toRight($"Can't find type '{name}'");
  }
}
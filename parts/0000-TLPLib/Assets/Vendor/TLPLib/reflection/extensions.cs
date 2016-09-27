using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.reflection {
  public static class AssemblyExts {
    public static IEnumerable<Props<A>> implementing<A>(
      this Assembly assembly
    ) where A : class {
      var interfaceType = typeof(A);
      if (!interfaceType.IsInterface)
        throw new ArgumentException($"{interfaceType} is not an interface!");
      return 
        assembly.GetTypes()
        .Where(t => t.GetInterfaces().Contains(interfaceType))
        .Select(t => new Props<A>(t));
    }
  }

  public static class TypeExts {
    public static bool hasEmptyConstructor(this Type t) => 
      t.GetConstructor(Type.EmptyTypes) != null;

    public static Option<A> getCustomAttribute<A>(this Type t) where A : Attribute =>
      F.opt(Attribute.GetCustomAttribute(t, typeof(A))).map(o => (A) o);
  }
}
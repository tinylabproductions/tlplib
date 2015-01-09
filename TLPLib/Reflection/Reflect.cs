using System;
using System.Reflection;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Reflection {
  /* This should only be used for debugging purposes! */

  public struct ReflectMethods<A> {
    readonly A a;

    public ReflectMethods(A a) { this.a = a; }

    public B field<B>(string fieldName) {
      const BindingFlags bindFlags =
        BindingFlags.Instance | BindingFlags.Static 
        | BindingFlags.Public | BindingFlags.NonPublic 
        | BindingFlags.FlattenHierarchy;
      var type = a.GetType();
      FieldInfo field = null;
      while (type != null && field == null) {
        field = type.GetField(fieldName, bindFlags);
        if (field == null) {
          Log.trace(string.Format("Reflect: couldn't find {0} in {1}, trying base class.", fieldName, type));
          type = type.BaseType;
        }
      }

      if (field == null) throw new ArgumentException(
        string.Format("Can't find field {0} in {1}", fieldName, typeof(A))
      );
      else return (B) field.GetValue(a);
    }
  }

  public static class AnyReflectionExts {
    public static ReflectMethods<A> reflection<A>(this A a) {
      return new ReflectMethods<A>(a);
    }
  }
}

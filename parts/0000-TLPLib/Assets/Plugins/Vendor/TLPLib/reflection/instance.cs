using System;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.reflection {
  public static class PrivateFieldAccessor {
    public static Fn<A, B> accessor<A, B>(string fieldName) {
      var type = typeof(A);
      var fieldInfo = type.GetField(
        fieldName, BindingFlags.Instance | BindingFlags.NonPublic
      );
      if (fieldInfo == null) throw new ArgumentException(
        $"Type {type} does not have non public instance field '{fieldName}'!"
      );
      return a => (B) fieldInfo.GetValue(a);
    }
  }

  public static class PrivateConstructor {
    public static Fn<object[], A> creator<A>() {
      var type = typeof(A);
      return args => (A) type.Assembly.CreateInstance(
          type.FullName, false,
          BindingFlags.Instance | BindingFlags.NonPublic,
          null, args, null, null
      );
    }
  }
}
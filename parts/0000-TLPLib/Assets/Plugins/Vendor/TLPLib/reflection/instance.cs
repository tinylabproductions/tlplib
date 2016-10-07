using System;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.reflection {
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
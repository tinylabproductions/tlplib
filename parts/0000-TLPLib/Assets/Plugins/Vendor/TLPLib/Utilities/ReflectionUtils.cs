using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ReflectionUtils {
    public static bool hasAttribute<T>(this MemberInfo mi) {
      return getAttributes<T>(mi).Any();
    }

    public static IEnumerable<T> getAttributes<T>(this MemberInfo mi) {
      return mi.GetCustomAttributes(typeof(T), false).Cast<T>();
    }
  }
}

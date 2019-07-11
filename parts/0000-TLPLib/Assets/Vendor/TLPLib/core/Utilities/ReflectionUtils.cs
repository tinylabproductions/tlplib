using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ReflectionUtils {
    public static bool hasAttribute<T>(this MemberInfo mi) =>
      getAttributes<T>(mi).Any();

    public static IEnumerable<T> getAttributes<T>(this MemberInfo mi, bool inherit = false) =>
      mi.GetCustomAttributes(typeof(T), inherit).Cast<T>();
  }
}

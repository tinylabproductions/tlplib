using System;
using System.Linq;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ReflectionUtils {
    public static bool hasAttribute<T>(this MemberInfo mi) {
      return mi.GetCustomAttributes(typeof(T), false).Any();
    }

    public static bool hasAttributeWithProperty<T>(this MemberInfo mi, Type propertyType, object propertyValue) {
      return CustomAttributeData.GetCustomAttributes(mi).Any(a =>
        a is T &&
        a.ConstructorArguments.Any(ca =>
          ca.ArgumentType == propertyType && ca.Value == propertyValue
        )
      );
    }
  }
}

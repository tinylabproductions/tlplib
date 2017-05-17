using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TypeExts {
    // http://stackoverflow.com/questions/1155529/not-getting-fields-from-gettype-getfields-with-bindingflag-default/1155549#1155549
    public static IEnumerable<FieldInfo> getAllFields(this Type t) {
      if (t == null) return Enumerable.Empty<FieldInfo>();

      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
      return t.GetFields(flags).Concat(getAllFields(t.BaseType));
    }

    // checks if type can be used in GetComponent and friends
    public static bool canBeUnityComponent(this Type type) => 
      type.IsInterface
      || typeof(MonoBehaviour).IsAssignableFrom(type)
      || typeof(Component).IsAssignableFrom(type);
  }
}

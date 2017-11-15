using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TypeExts {
    // Allows getting pretty much any kind of field.
    const BindingFlags FLAGS_ANY_FIELD_TYPE =
      BindingFlags.Public |
      BindingFlags.NonPublic |
      BindingFlags.Instance |
      BindingFlags.DeclaredOnly;

    // http://stackoverflow.com/questions/1155529/not-getting-fields-from-gettype-getfields-with-bindingflag-default/1155549#1155549
    public static IEnumerable<FieldInfo> getAllFields(this Type t) {
      if (t == null) return Enumerable.Empty<FieldInfo>();
      
      return t.GetFields(FLAGS_ANY_FIELD_TYPE).Concat(getAllFields(t.BaseType));
    }

    public static Option<FieldInfo> getFieldByName(this Type t, string fieldName) {
      if (t == null) return Option<FieldInfo>.None;

      return F.opt(t.GetField(fieldName, FLAGS_ANY_FIELD_TYPE));
    }

    // checks if type can be used in GetComponent and friends
    public static bool canBeUnityComponent(this Type type) => 
      type.IsInterface
      || typeof(MonoBehaviour).IsAssignableFrom(type)
      || typeof(Component).IsAssignableFrom(type);
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.exts;
using pzd.lib.functional;
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
    public static IEnumerable<FieldInfo> getAllFields(this Type t) =>
      t.GetFields(FLAGS_ANY_FIELD_TYPE)
      .Concat(t.BaseTypeSafe().map(getAllFields).getOrElse(Enumerable.Empty<FieldInfo>()));

    /// <summary>
    /// Like <see cref="Type.GetField(string,System.Reflection.BindingFlags)"/>
    /// </summary>
    public static Option<FieldInfo> GetFieldInHierarchy(this Type t, string fieldName) =>
      F.opt(t.GetField(fieldName, FLAGS_ANY_FIELD_TYPE))
      || t.BaseTypeSafe().flatMap(baseType => GetFieldInHierarchy(baseType, fieldName));


    /// <summary>
    /// Like <see cref="Type.GetMethod(string,System.Reflection.BindingFlags)"/>
    /// </summary>
    public static Option<MethodInfo> GetMethodInHierarchy(this Type t, string fieldName) =>
      F.opt(t.GetMethod(fieldName, FLAGS_ANY_FIELD_TYPE | BindingFlags.Static))
      || t.BaseTypeSafe().flatMap(baseType => GetMethodInHierarchy(baseType, fieldName));

    public static Option<Type> BaseTypeSafe(this Type t) => F.opt(t.BaseType);

    // checks if type can be used in GetComponent and friends
    public static bool canBeUnityComponent(this Type type) =>
      type.IsInterface
      || typeof(MonoBehaviour).IsAssignableFrom(type)
      || typeof(Component).IsAssignableFrom(type);
  }
}

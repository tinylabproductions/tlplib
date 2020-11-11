using System;
using System.Reflection;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class FieldInfoExts {
    public static bool isSerializable(this FieldInfo fi) =>
      (fi.IsPublic && !fi.hasAttribute<NonSerializedAttribute>())
      || ((fi.IsPrivate || fi.IsFamily) && (fi.hasAttribute<SerializeField>() || fi.hasAttribute<SerializeReference>()));
  }
}
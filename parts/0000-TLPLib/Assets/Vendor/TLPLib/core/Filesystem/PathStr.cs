using System;
using System.Collections.Generic;
using System.IO;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.serialization;
using pzd.lib.typeclasses;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  [
    Serializable, PublicAPI, Record(GenerateConstructor = ConstructorFlags.None, GenerateToString = false)
  ]
  public partial struct PathStr : IComparable<PathStr>, IStr {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField] string _path;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    public string path => _path;

    public PathStr(string path) {
      _path = path.Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
    }
    public static PathStr a(string path) => new PathStr(path);

    #region Comparable

    public int CompareTo(PathStr other) => string.Compare(path, other.path, StringComparison.Ordinal);

    sealed class PathRelationalComparer : Comparer<PathStr> {
      public override int Compare(PathStr x, PathStr y) {
        return string.Compare(x.path, y.path, StringComparison.Ordinal);
      }
    }

    public static Comparer<PathStr> pathComparer { get; } = new PathRelationalComparer();

    #endregion

    public static PathStr operator /(PathStr s1, string s2) => new PathStr(Path.Combine(s1.path, s2));
    public static implicit operator string(PathStr s) => s.path;

    public PathStr dirname => new PathStr(Path.GetDirectoryName(path));
    public PathStr basename => new PathStr(Path.GetFileName(path));
    public string extension => Path.GetExtension(path);
    public PathStr withoutExtension => a(_path.Substring(0, _path.Length - extension.Length));
    public PathStr ensureBeginsWith(PathStr p) => path.StartsWithFast(p.path) ? this : p / path;
    public override string ToString() => asString();
    public string asString() => path;
    public string unixString => ToString().Replace('\\', '/');

    // Use this with Unity Resources, AssetDatabase and PrefabUtility methods
    public string unityPath => Path.DirectorySeparatorChar == '/' ? path : path.Replace('\\' , '/');
    
    public PathStr toAbsolute => a(Path.GetFullPath(path));

    public static readonly ISerializedRW<PathStr> serializedRW =
      SerializedRW.str.mapNoFail(s => new PathStr(s), path => path.path);
  }

  public static class PathStrExts {
    static Option<PathStr> onCondition(this string s, bool condition) =>
      (condition && s != null).opt(new PathStr(s));

    public static Option<PathStr> asFile(this string s) => s.onCondition(File.Exists(s));
    public static Option<PathStr> asDirectory(this string s) => s.onCondition(Directory.Exists(s));
  }
}

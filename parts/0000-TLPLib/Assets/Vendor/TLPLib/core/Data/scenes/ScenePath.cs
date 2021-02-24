using System;
using System.Collections.Generic;
using System.IO;
using com.tinylabproductions.TLPLib.Filesystem;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data.scenes {
  /// <summary>Path to the scene.</summary>
  [Record] public readonly partial struct ScenePath : IComparable<ScenePath> {
    public readonly string path;
    
    #region Comparer
    
    public int CompareTo(ScenePath other) {
      return string.Compare(path, other.path, StringComparison.Ordinal);
    } 

    sealed class PathEqualityComparer : IEqualityComparer<ScenePath> {
      public bool Equals(ScenePath x, ScenePath y) {
        return string.Equals(x.path, y.path);
      }

      public int GetHashCode(ScenePath obj) {
        return (obj.path != null ? obj.path.GetHashCode() : 0);
      }
    }

    public static IEqualityComparer<ScenePath> pathComparer { get; } = new PathEqualityComparer();
    
    #endregion
    
    public SceneName toSceneName => new SceneName(Path.GetFileNameWithoutExtension(path));
    
    public PathStr toPathStr => PathStr.a(path);
    
    public static implicit operator string(ScenePath s) => s.path;
    public static implicit operator SceneName(ScenePath s) => s.toSceneName;
  }
}
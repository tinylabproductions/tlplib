using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3PathBehaviour : MonoBehaviour {
#pragma warning disable 649
    [SerializeField] bool _relative, _closed;
    [SerializeField, Range(50, 1000)] int pathResolution = 250;
    [SerializeField] Vector3Path.InterpolationMethod _method;
    [SerializeField] List<Vector3> _nodes = new List<Vector3>();
#pragma warning restore 649
    Vector3Path _path;
    
    public void invalidate() => _path = null;

    public Vector3Path path => 
      _path ?? (
        _path = new Vector3Path(_method, _closed, _nodes.ToImmutableArray(), _relative ? F.some(transform) : F.none_, pathResolution)
      );
  }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3PathBehaviour : MonoBehaviour {
    
#pragma warning disable 649
// ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
    [SerializeField] bool _relative, _closed;
    [SerializeField, Range(50, 1000)] int pathResolution = 250;
    [SerializeField] Vector3Path.InterpolationMethod _method;
    [SerializeField] List<Vector3> _nodes = new List<Vector3>();
// ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
#pragma warning restore 649

    Vector3Path _path;
    
    public void invalidate() => _path = null;

    public Vector3Path path => 
      _path ?? (
        _path = new Vector3Path(_method, _closed, _nodes.ToImmutableArray(), _relative ? F.some(transform) : F.none_, pathResolution)
      );
  }
}
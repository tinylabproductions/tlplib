using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3PathBehaviour : MonoBehaviour {

    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField] bool _relative, _closed;
    [SerializeField, Range(50, 1000)] int pathResolution = 250;
    [SerializeField] Vector3Path.InterpolationMethod _method;
    [SerializeField, ListDrawerSettings(ShowIndexLabels = true)] List<Vector3> _nodes = new List<Vector3>();
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649
    #endregion

    Vector3Path _path;
    
    public void invalidate() => _path = null;

    public Vector3Path path => 
      _path ??= new Vector3Path(_method, _closed, _nodes.ToImmutableArray(), _relative ? F.some(transform) : F.none_, pathResolution);
  }
}
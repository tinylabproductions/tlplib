using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public class Vector3PathBehaviour : MonoBehaviour, IMB_OnValidate {
#if UNITY_EDITOR
    public bool lockXAxis, lockYAxis, lockZAxis, relative, closed;
    public Vector3Path.InterpolationMethod method;

    [Range(0.01f, 1.5f)] public float nodeHandleSize = 0.6f;
    [Range(10, 500)] public int curveSubdivisions = 100;
    [Range(20, 50)] public int pathResolution = 30;
    public bool showDirection = true;
#endif

    public List<Vector3> nodes = new List<Vector3>();

    Vector3Path _path;
    public Vector3Path path => 
      _path ?? (
        _path = new Vector3Path(method, closed, nodes.ToImmutableArray(), relative ? F.some(transform) : F.none_, pathResolution)
      );

    public void invalidate() => _path = null;

    public void OnValidate() {
      invalidate();  
    }
  }
}
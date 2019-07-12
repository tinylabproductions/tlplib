#if UNITY_EDITOR
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3PathBehaviour : IMB_OnValidate {
    [SerializeField] public bool lockXAxis, lockYAxis, lockZAxis;
    [SerializeField, Range(0.02f, 0.7f)] public float nodeHandleSize = 0.5f;
    [Range(10, 500)] public int curveSubdivisions = 100;
    [SerializeField] public bool showDirection = true;

    public bool relative {
      get => _relative;
      set => _relative = value;
    }
    
    public bool closed {
      get => _closed;
      set => _closed = value;
    }
    
    public List<Vector3> nodes {
      get => _nodes;
      set => _nodes = value;
    }
    
    public Vector3Path.InterpolationMethod method => _method;
    
    public Subject<Unit> onValidate = new Subject<Unit>();
    
    public void OnValidate() {
      invalidate();
      onValidate.push(F.unit);
    }
  }
}
#endif
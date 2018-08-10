using System;
using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class WithValue {
    [PublicAPI] public static readonly Ref<Color>
      gizmosColorRef = new LambdaRef<Color>(() => Gizmos.color, v => Gizmos.color = v);
    [PublicAPI] public static readonly 
      Fn<Color, WithValue<Color>> gizmosColor = v => new WithValue<Color>(gizmosColorRef, v);
    
    [PublicAPI] public static readonly Ref<Matrix4x4>
      gizmosMatrixRef = new LambdaRef<Matrix4x4>(() => Gizmos.matrix, v => Gizmos.matrix = v);
    [PublicAPI] public static readonly 
      Fn<Matrix4x4, WithValue<Matrix4x4>> gizmosMatrix = v => new WithValue<Matrix4x4>(gizmosMatrixRef, v);

#if UNITY_EDITOR
    [PublicAPI]
    public static readonly Ref<Color>
      handlesColorRef = new LambdaRef<Color>(
        () => UnityEditor.Handles.color, v => UnityEditor.Handles.color = v
      );
    [PublicAPI]
    public static readonly 
      Fn<Color, WithValue<Color>> handlesColor = color => new WithValue<Color>(handlesColorRef, color);
#endif
  }
  
  [PublicAPI]
  public struct WithValue<A> : IDisposable {
    public readonly Ref<A> @ref;
    public readonly A oldValue;

    public WithValue(Ref<A> @ref, A value) {
      this.@ref = @ref;
      oldValue = @ref.value;
      @ref.value = value;
    }

    public void Dispose() => @ref.value = oldValue;
  }
}
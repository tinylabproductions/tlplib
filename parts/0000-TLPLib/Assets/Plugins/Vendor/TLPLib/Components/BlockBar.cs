using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public abstract partial class BlockBar<A> : MonoBehaviour where A : MonoBehaviour {

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor] int _elementCount;
    [SerializeField, NotNull] Transform blockParent;
    [SerializeField, NotNull] A barElement;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public delegate void SetBarState(A barElement, bool isEnabled);

    public class Init {
      readonly Fn<SetBarState> setState;
      readonly List<A> items = new List<A>();

      public Init(BlockBar<A> backing, Fn<SetBarState> setState) {
        this.setState = setState;
        for (var i = 0; i < backing.elementCount; i++) {
          items.Add(backing.barElement.clone(parent: backing.blockParent));
        }
      }

      [PublicAPI]
      public void setProgress(float progress) => setEnabledCount(Mathf.FloorToInt(items.Count * progress));

      [PublicAPI]
      public void setEnabledCount(int count) {
        var setBar = setState();
        for (var i = 0; i < items.Count; i++) {
          setBar(items[i], i < count);
        }
      }
    }
  }
}
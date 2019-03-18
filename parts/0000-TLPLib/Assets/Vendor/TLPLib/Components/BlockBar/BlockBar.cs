using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.BlockBar {
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

    public abstract class Init : IDisposable {
      readonly SetBarState setState;
      readonly List<A> items = new List<A>();

      protected Init(BlockBar<A> backing, SetBarState setState) {
        this.setState = setState;
        for (var i = 0; i < backing.elementCount; i++) {
          items.Add(backing.barElement.clone(parent: backing.blockParent));
        }
      }

      [PublicAPI]
      public void setProgress(float progress) => setEnabledCount(Mathf.FloorToInt(items.Count * progress));

      [PublicAPI]
      public void setEnabledCount(int count) {
        for (var i = 0; i < items.Count; i++) {
          setState(items[i], i < count);
        }
      }

      public void Dispose() {
        foreach (var item in items) Destroy(item.gameObject);
      }
    }
  }
}
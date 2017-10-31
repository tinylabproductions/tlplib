using System;
using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [Serializable] public abstract class ClickableRuntimeSceneRef<A> where A : RuntimeSceneRef {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] UIClickForwarder _button;
    [SerializeField, NotNull] A _sceneRef;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public UIClickForwarder button => _button;
    public A sceneRef => _sceneRef;
  }
}

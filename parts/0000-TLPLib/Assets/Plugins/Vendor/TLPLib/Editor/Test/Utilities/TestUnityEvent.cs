using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

class TestUnityEvent : ScriptableObject {
  #region Unity Serialized Fields

#pragma warning disable 649, 169
  // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
  [Serializable] class NestedEvent {
    [SerializeField, UsedImplicitly] UnityEvent unityEvent;
  }

  [SerializeField, UsedImplicitly] UnityEvent unityEvent;
  [SerializeField, UsedImplicitly] NestedEvent nestedEvent;
  [SerializeField, UsedImplicitly] NestedEvent[] nestedEventArray;
  // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649, 169

  #endregion
}

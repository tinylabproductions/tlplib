using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

internal class TestUnityEvent : ScriptableObject {
  [Serializable] class NestedEvent {
    [SerializeField, UsedImplicitly] UnityEvent unityEvent;
  }

  [SerializeField, UsedImplicitly] UnityEvent unityEvent;
  [SerializeField, UsedImplicitly] NestedEvent nestedEvent;
  [SerializeField, UsedImplicitly] NestedEvent[] nestedEventArray;
}

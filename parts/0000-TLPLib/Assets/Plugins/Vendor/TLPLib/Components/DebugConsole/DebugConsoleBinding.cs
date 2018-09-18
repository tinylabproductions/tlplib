using System;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Data;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public partial class DebugConsoleBinding : MonoBehaviour {
    [NotNull] public DebugConsoleListBinding commandGroups, commands;
    [NotNull] public Text commandGroupLabel;
    [NotNull] public ButtonBinding buttonPrefab;
    [NotNull] public Button closeButton;
    [SerializeField, NotNull, PublicAccessor] UIClickForwarder _minimizeButton;
    [SerializeField, NotNull, PublicAccessor] GameObject _minimizableObjectsContainer;
    [NotNull] public DynamicVerticalLayout dynamicLayout;
    [NotNull] public VerticalLayoutLogEntryPrefab logEntry;
    
    public float lineWidth => dynamicLayout.maskRect.rect.width;
  }

  [Serializable]
  public class VerticalLayoutLogEntryPrefab : TagPrefab<VerticalLayoutLogEntry> { }
}

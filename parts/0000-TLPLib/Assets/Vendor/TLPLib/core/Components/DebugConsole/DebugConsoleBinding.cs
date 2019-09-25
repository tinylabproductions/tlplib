using System;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleBinding : MonoBehaviour {
    [NotNull] public DebugConsoleListBinding commandGroups, commands;
    [NotNull] public Text commandGroupLabel;
    [NotNull] public ButtonBinding buttonPrefab;
    [NotNull] public Button closeButton, minimiseButton;
    [NotNull] public DynamicVerticalLayout dynamicLayout;
    [NotNull] public VerticalLayoutLogEntryPrefab logEntry;
    [NotNull] public GameObject logPanel;

    public float lineWidth => dynamicLayout.maskRect.rect.width;
  }

  [Serializable]
  public class VerticalLayoutLogEntryPrefab : TagPrefab<VerticalLayoutLogEntry> { }
}

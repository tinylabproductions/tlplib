using System;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleBinding : MonoBehaviour, IMB_Update {
    [NotNull] public DebugConsoleListBinding commandGroups, commands;
    [NotNull] public Text commandGroupLabel;
    [NotNull] public ButtonBinding buttonPrefab;
    [NotNull] public Button closeButton, minimiseButton;
    [NotNull] public DynamicVerticalLayout dynamicLayout;
    [NotNull] public VerticalLayoutLogEntryPrefab logEntry;
    [NotNull] public GameObject logPanel;

    public float lineWidth => dynamicLayout.maskRect.rect.width;
    
    public bool minimised { get; private set; }

    public void toggleMinimised() {
      var active = minimised;
      minimised = !minimised;
      closeButton.setActiveGO(active);
      commandGroups.setActiveGO(active);
      commands.setActiveGO(active);
      logPanel.SetActive(active);
    }

    public event Action onUpdate;
    public void Update() => onUpdate?.Invoke();
  }

  [Serializable]
  public class VerticalLayoutLogEntryPrefab : TagPrefab<VerticalLayoutLogEntry> { }
}

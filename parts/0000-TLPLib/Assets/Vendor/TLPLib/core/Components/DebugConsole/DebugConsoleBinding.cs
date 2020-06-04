using System;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public sealed partial class DebugConsoleBinding : MonoBehaviour, IMB_Update {
    // ReSharper disable NotNullMemberIsNotInitialized
    [NotNull] public DebugConsoleListBinding commandGroups, commands;
    [NotNull] public Text commandGroupLabel;
    [NotNull] public ButtonBinding buttonPrefab;
    [NotNull] public Button closeButton, minimiseButton;
    [NotNull] public DynamicVerticalLayout dynamicLayout;
    [NotNull] public VerticalLayoutLogEntryPrefab logEntry;
    [NotNull] public GameObject logPanel;
    [NotNull, SerializeField, PublicAccessor] GameObject _modals;
    [NotNull, SerializeField, PublicAccessor] DebugConsoleInputModalBinding _inputModal;
    // ReSharper restore NotNullMemberIsNotInitialized

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

    public void showModal(bool inputModal = false) {
      _modals.SetActive(true);
      _inputModal.setActiveGO(inputModal);
    }

    public void hideModals() => _modals.SetActive(false);

    public event Action onUpdate;
    public void Update() => onUpdate?.Invoke();
  }

  [Serializable]
  public class VerticalLayoutLogEntryPrefab : TagPrefab<VerticalLayoutLogEntry> { }
}

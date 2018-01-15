using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleBinding : MonoBehaviour {
    [NotNull] public DebugConsoleListBinding commandGroups, commands;
    [NotNull] public GameObject logEntriesHolder;
    [NotNull] public Text commandGroupLabel, logEntryPrefab;
    [NotNull] public ButtonBinding buttonPrefab;
    [NotNull] public Button closeButton;
  }
}

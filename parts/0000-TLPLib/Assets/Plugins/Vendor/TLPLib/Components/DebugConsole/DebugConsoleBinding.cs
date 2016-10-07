using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleBinding : MonoBehaviour {
    public GameObject commandGroupsHolder, commandsHolder, logEntriesHolder;
    public Text commandGroupLabel, logEntryPrefab;
    public ButtonBinding buttonPrefab;
    public Button closeButton;
  }
}

using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleBinding : MonoBehaviour {
    public GameObject buttonHolder, logEntriesHolder;
    public ButtonBinding buttonPrefab;
    public Button closeButton;
    public Text logEntryPrefab;
  }
}

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DebugConsoleListBinding : MonoBehaviour {
    [NotNull] public Text label;
    [NotNull] public InputField filterInput;
    [NotNull] public Button clearFilterButton;
    [NotNull] public GameObject holder;
  }
}
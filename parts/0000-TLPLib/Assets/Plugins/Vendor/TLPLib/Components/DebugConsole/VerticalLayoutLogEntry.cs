using com.tinylabproductions.TLPLib.Components.ui;
using JetBrains.Annotations;
using Smooth.Dispose;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class VerticalLayoutLogEntry : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform baseTransform;
    [SerializeField, NotNull] Text text;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public class Init : DynamicVerticalLayout.IElementView {
      readonly Disposable<VerticalLayoutLogEntry> backing;

      public Init(Disposable<VerticalLayoutLogEntry> backing, string text, Color color) {
        this.backing = backing;
        backing.value.text.text = text;
        backing.value.text.color = color;
      }

      public void Dispose() => backing.Dispose();
      public RectTransform rectTransform => backing.value.baseTransform;
    }
  }
}
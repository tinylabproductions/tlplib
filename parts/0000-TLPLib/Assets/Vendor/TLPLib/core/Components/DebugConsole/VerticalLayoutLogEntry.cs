using com.tinylabproductions.TLPLib.Components.ui;
using GenerationAttributes;
using JetBrains.Annotations;
using Smooth.Dispose;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public partial class VerticalLayoutLogEntry : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] RectTransform baseTransform;
    [SerializeField, NotNull] Text text;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    [Record]
    public partial class Data {
      public readonly string text;
      public readonly Color color;
    }

    public class Init : DynamicVerticalLayout.IElementView {
      readonly Disposable<VerticalLayoutLogEntry> backing;

      public Init(Disposable<VerticalLayoutLogEntry> backing, Data data) {
        this.backing = backing;
        backing.value.text.text = data.text;
        backing.value.text.color = data.color;
      }

      public void Dispose() => backing.Dispose();
      public RectTransform rectTransform => backing.value.baseTransform;
    }
  }
}
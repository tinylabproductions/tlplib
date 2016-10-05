using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.errors_in_your_face {
  /**
   * Registers to ```Application.logMessageReceivedThreaded``` and shows a game object
   * in your face when handled message types arrive.
   **/
  public class ErrorsInYourFace : MonoBehaviour {
    [HideInInspector]
    public static readonly IImmutableSet<LogType> DEFAULT_HANDLED_TYPES =
      ImmutableHashSet.Create(
        LogType.Exception, LogType.Assert, LogType.Error, LogType.Warning
      );

    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] Text _errorsText;
    [SerializeField] Button _hideButton;
    [SerializeField] Color 
      _errorColor = Color.red, 
      _exceptionColor = Color.red, 
      _assertColor = Color.cyan, 
      _warningColor = Color.yellow, 
      _logColor = Color.gray;
#pragma warning restore 649
// ReSharper restore FieldCanBeMadeReadOnly.Local

    ErrorsInYourFace() {}

    public class Init {
      readonly IImmutableSet<LogType> handledTypes;
      readonly Application.LogCallback logCallback;
      readonly ErrorsInYourFace binding;
      readonly int queueSize;
      readonly LinkedList<string> entries;

      bool _enabled;
      public bool enabled {
        get { return _enabled; }
        set {
          _enabled = value;
          if (value) {
            Application.logMessageReceivedThreaded += logCallback;
          }
          else {
            Application.logMessageReceivedThreaded -= logCallback;
            hide();
          }
        }
      }

      public Init(
        int queueSize = 10,
        IImmutableSet<LogType> handledTypes = null, ErrorsInYourFace binding = null
      ) {
        this.handledTypes = handledTypes ?? DEFAULT_HANDLED_TYPES;
        this.queueSize = queueSize;
        entries = new LinkedList<string>();
        this.binding = initBinding(binding ?? Resources.Load<ErrorsInYourFace>("ErrorsInYourFaceCanvas"));
        logCallback = logMessageHandlerThreaded;

        setText();
        hide();
      }

      ErrorsInYourFace initBinding(ErrorsInYourFace prefab) {
        var instance = prefab.clone();
        instance._hideButton.onClick.AddListener(hide);
        DontDestroyOnLoad(instance);
        return instance;
      }

      void setVisible(bool visible) {
        binding.gameObject.SetActive(visible);
      }

      public void show() => setVisible(true);
      public void hide() => setVisible(false);

      void logMessageHandlerThreaded(string message, string stackTrace, LogType type) {
        if (!handledTypes.Contains(type)) return;
        lock (this) logMessageHandler(message, type);
      }

      void logMessageHandler(string message, LogType type) {
        enqueue(message, type);
        setText();
        show();
      }

      void enqueue(string message, LogType type) {
        var color = logTypeToColor(type);
        var entry = $"<color=#{color.toHex()}>{message}</color>";
        if (entries.Count == queueSize) entries.RemoveLast();
        entries.AddFirst(entry);
      }

      void setText() => binding._errorsText.text = entries.mkString("\n");

      Color32 logTypeToColor(LogType type) {
        switch (type) {
          case LogType.Assert: return binding._assertColor;
          case LogType.Error: return binding._errorColor;
          case LogType.Exception: return binding._exceptionColor;
          case LogType.Warning: return binding._warningColor;
          case LogType.Log: return binding._logColor;
          default: return Color.white;
        }
      }
    }
  }
}
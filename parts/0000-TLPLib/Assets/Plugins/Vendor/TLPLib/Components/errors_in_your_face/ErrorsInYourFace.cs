using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
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

    // Unity UI fails to render too may characters, because of vertex limit per mesh
    public const int DEFAULT_MAX_SYMBOLS = 10000;

// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] Text _errorsText;
    [SerializeField] Button _hideButton;
    [SerializeField] Color
      _errorColor = Color.red,
      _exceptionColor = Color.red,
      _assertColor = Color.cyan,
      _warningColor = Color.yellow,
      _logColor = Color.gray,
      _stacktraceColor = Color.gray;
    [SerializeField] uint _stacktraceTextSize = 20;
#pragma warning restore 649
// ReSharper restore FieldCanBeMadeReadOnly.Local

    ErrorsInYourFace() {}

    public class Init {
      readonly IImmutableSet<LogType> handledTypes;
      readonly Application.LogCallback logCallback;
      readonly ErrorsInYourFace binding;
      readonly int maxSymbols;
      readonly LinkedList<string> entries;
      int removedMessages;

      bool _enabled;
      public bool enabled {
        get => _enabled;
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
        ErrorsInYourFace binding,
        int maxSymbols = DEFAULT_MAX_SYMBOLS, IImmutableSet<LogType> handledTypes = null
      ) {
        this.handledTypes = handledTypes ?? DEFAULT_HANDLED_TYPES;
        this.maxSymbols = maxSymbols;
        entries = new LinkedList<string>();
        this.binding = binding;
        logCallback = logMessageHandlerThreaded;

        initBinding(binding);
        setText();
        hide();
      }

      public Init(
        int maxSymbols = DEFAULT_MAX_SYMBOLS,
        IImmutableSet<LogType> handledTypes = null
      ) : this(
        Resources.Load<ErrorsInYourFace>("ErrorsInYourFaceCanvas").clone(),
        maxSymbols, handledTypes
      ) {}

      void initBinding(ErrorsInYourFace instance) {
        instance._hideButton.onClick.AddListener(hide);
        DontDestroyOnLoad(instance);
      }

      void setVisible(bool visible) {
        binding.gameObject.SetActive(visible);
      }

      public void show() => setVisible(true);
      public void hide() => setVisible(false);

      void logMessageHandlerThreaded(string message, string stackTrace, LogType type) {
        if (!handledTypes.Contains(type)) return;
        ASync.OnMainThread(() => logMessageHandler(message, stackTrace, type));
      }

      void logMessageHandler(string message, string stackTrace, LogType type) {
        enqueue(message, stackTrace, type);
        setText();
        show();
      }

      void enqueue(string message, string stacktrace, LogType type) {
        var color = logTypeToColor(type);
        var entry = $"<color=#{color.toHex()}>{message}</color>";
        if (binding._stacktraceTextSize != 0) {
          Color32 stacktraceColor = binding._stacktraceColor;
          entry +=
            $"\n<color=#{stacktraceColor.toHex()}><size={binding._stacktraceTextSize}>" +
            stacktrace +
            $"</size></color>";
        }

        while (calculateSymbols() > maxSymbols) {
          entries.RemoveLast();
          removedMessages++;
        }
        entries.AddFirst(entry);
      }

      int calculateSymbols() => entries.Aggregate(0, (sum, entry) => sum + entry.Length);

      void setText() {
        var text = entries.mkString("\n");
        if (removedMessages > 0) text += $"\nRemoved messages: {removedMessages}";
        binding._errorsText.text = text;
      }

      Color32 logTypeToColor(LogType type) {
        switch (type) {
          case LogType.Assert: return binding._assertColor;
          case LogType.Error: return binding._errorColor;
          case LogType.Exception: return binding._exceptionColor;
          case LogType.Warning: return binding._warningColor;
          case LogType.Log: return binding._logColor;
        }
        return Color.white;
      }
    }
  }
}
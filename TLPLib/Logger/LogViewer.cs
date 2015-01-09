using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.tinylabproductions.TLPLib.Binding;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.InputUtils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Logger {
  public class LogViewer {
    const int CHARS_PER_LINE = 200;

    readonly Text text;
    readonly List<string> lines = new List<string>();
    int curLine;
    int linesToShow = 20;
    int leftPadding;

    readonly Future<Unit> onClose;

    static LogViewer currentInstance;

    public static LogViewer instantiate() {
      if (currentInstance == null) {
        currentInstance = new LogViewer(new StreamReader(
          new FileStream(Log.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        ));
        currentInstance.onClose.onSuccess(_ => currentInstance = null);
      }
      return currentInstance;
    }

    public LogViewer(StreamReader stream) {
      var binding = 
        ((GameObject) Object.Instantiate(Resources.Load("TLPLib/Log Canvas")))
        .GetComponent<LogViewerBinding>();

      text = binding.text;
      binding.prev    .onClick.AddListener(() => { curLine -= linesToShow; refresh(); });
      binding.next    .onClick.AddListener(() => { curLine += linesToShow; refresh(); });
      binding.first   .onClick.AddListener(() => { curLine = 0;            refresh(); });
      binding.last    .onClick.AddListener(moveToLast);
      binding.zoomIn  .onClick.AddListener(() => { linesToShow -= 3;       refresh(); });
      binding.zoomOut .onClick.AddListener(() => { linesToShow += 3;       refresh(); });

      while (true) {
        var s = stream.ReadLine();
        if (s == null) break;
        lines.Add(s);
      }

      int scrollState = 0;
      var startPos = new Vector2();
      int startCurLine = 0, startLeftPadding = 0;
      ASync.EveryFrame(binding.gameObject, () => {
        var mp = Pointer.currentPosition;
        var delta = mp - startPos;
        if (scrollState == 0) { // idle
          if (Pointer.isDown) {
            scrollState = 1;
            startPos = mp;
            startCurLine = curLine;
            startLeftPadding = leftPadding;
          }
        }
        else if (scrollState == 1) { // waiting threshold
          if (! Pointer.held) scrollState = 0;
          if (delta.sqrMagnitude > 10 * 10) {
            if (Math.Abs(delta.y) > Math.Abs(delta.x)) {
              scrollState = 2; // vertical
            }
            else {
              scrollState = 3; // horizontal
            }
          }
        }
        else {
          if (scrollState == 2) {
            curLine = startCurLine + Mathf.RoundToInt(delta.y / 10);
          }
          else {
            leftPadding = startLeftPadding - Mathf.RoundToInt(delta.x / 5);
          }
          if (! Pointer.held) scrollState = 0;
          refresh();
        }
        if (Input.mouseScrollDelta.y != 0) {
          curLine -= Mathf.RoundToInt(Input.mouseScrollDelta.y) * 4;
          refresh();
        }
        return true;
      });

      binding.close.onClick.AddListener(() => Object.Destroy(binding.gameObject));
      onClose = binding.close.clicksObservable().map(_ => F.unit).toFuture();

      refresh();
    }

    public void moveToLast() { curLine = lines.Count; refresh(); }

    void refresh() {
      curLine = Mathf.Clamp(curLine, 0, lines.Count - linesToShow);
      if (leftPadding < 0) leftPadding = 0;
      var newText = new StringBuilder();
      var biggesLen = 0;
      for (var i = 0; i < linesToShow; i++) {
        var len = lines[curLine + i].Length;
        biggesLen = Math.Max(biggesLen, len);
        var startIndex = Math.Min(leftPadding, len);
        var count = Math.Min(CHARS_PER_LINE, len - startIndex);
        newText.Append(lines[curLine + i], startIndex, count);
        newText.AppendLine();
      }
      if (biggesLen < leftPadding) {
        leftPadding = biggesLen - 1;
        refresh();
        return;
      }
      text.text = newText.ToString();
    }
  }

  public class LogViewerBinding : MonoBehaviour {
    public Text text;
    public Button prev, next, first, last, zoomIn, zoomOut, close;
  }
}

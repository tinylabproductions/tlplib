using System;
using System.Diagnostics;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using static com.tinylabproductions.TLPLib.Data.typeclasses.Str;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  /// <summary>
  /// Better version of EditorUtility progress bars that does not lag and also logs to log.
  /// </summary>
  public class EditorProgress : IDisposable {
    const string NONE = "none";

    public readonly string title;
    public readonly Duration showAfter;

    readonly Stopwatch sw = new Stopwatch();
    readonly ILog log;

    string current = NONE;
    DateTime 
      lastProgressUIUpdate = DateTime.MinValue,
      lastProgressLogUpdate = DateTime.MinValue;

    public EditorProgress(string title, ILog log = null, Duration showAfter = default) {
      this.title = title;
      this.showAfter = showAfter;
      this.log = (log ?? Log.@default).withScope($"{s(title)}:");
      sw.Start();
    }

    public void execute(string name, Action a) => 
      execute(name, () => { a(); return F.unit; });

    public A execute<A>(string name, Fn<A> f) {
      start(name);
      var ret = f();
      done();
      return ret;
    }

    public void start(string name) {
      sw.Reset();
      sw.Start();
      current = name;
      if (log.isInfo()) log.info($"Running {s(name)}...");
      if (showAfter == Duration.zero)
        EditorUtility.DisplayProgressBar(title, $"Running {s(name)}...", 0);
      lastProgressUIUpdate = lastProgressLogUpdate = DateTime.MinValue;
    }

    bool _progress(int idx, int total, bool cancellable) {
      if (sw.ElapsedMilliseconds < showAfter.millis) return false;
      var now = DateTime.Now;
      var needsUpdate = idx == 0 || idx == total - 1;
      // Updating progress for every call is expensive, only show every X ms.
      var updateUI = needsUpdate || (now - lastProgressUIUpdate).TotalSeconds >= 0.2;
      var updateLog = log.isInfo() && (needsUpdate || (now - lastProgressLogUpdate).TotalSeconds >= 5);

      var canceled = false;
      if (updateUI || updateLog) {
        var item = idx + 1;
        var msg = $"{s(current)} ({s(item)}/{s(total)})...";

        if (updateUI) {
          if(cancellable)
            canceled = EditorUtility.DisplayCancelableProgressBar(title, msg, (float) item / total);
          else
            EditorUtility.DisplayProgressBar(title, msg, (float) item / total);
          lastProgressUIUpdate = now;
        }
        if (updateLog) {
          log.info(msg);
          lastProgressLogUpdate = now;
        }
      }
      return canceled;
    }
    
    public void progress(int idx, int total) => _progress(idx, total, false);

    /// <summary>Wrapper for Unity CancelableProgressBar</summary>
    /// <returns>True if task was canceled</returns>
    public bool progressCancellable(int idx, int total) => _progress(idx, total, true);

    /** A simple method to measure execution times between calls **/
    public void done() {
      var duration = new Duration(sw.ElapsedMilliseconds.toIntClamped());
      if (log.isInfo()) log.info($"{s(current)} done in {s(duration)}");
      EditorUtility.DisplayProgressBar(title, $"{s(current)} done in {s(duration)}.", 1);
      current = NONE;
    }

    public void Dispose() { EditorUtility.ClearProgressBar(); }
  }

}

using System;
using System.Diagnostics;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using static pzd.lib.typeclasses.Str;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  /// <summary>
  /// Better version of EditorUtility progress bars that does not lag and also logs to log.
  /// </summary>
  public class EditorProgress : IDisposable {
    const string NONE = "none";

    public readonly string title;
    public readonly Duration minTimeBeforeShowingUi;

    readonly Stopwatch sw = new Stopwatch();
    readonly ILog log;
    readonly DateTime creationTime = DateTime.Now;

    string current = NONE;
    DateTime
      lastProgressUIUpdate = DateTime.MinValue,
      lastProgressLogUpdate = DateTime.MinValue;

    /// <param name="minTimeBeforeShowingUI">
    /// Duration to wait before allowing to show the progressbar dialog.
    /// Use case: When tasks using <see cref="EditorProgress"/> are called frequently (ex. every time you save project)
    /// and it takes a short amount of time (ex. loading small amount of files), that can only be seen as a flicker.
    /// You can prevent this, by setting <see cref="minTimeBeforeShowingUi"/> to a certain value to not allow
    /// progressbar dialog appear in screen before time after creation passes this value. Default is to show instantly.
    /// </param>
    public EditorProgress(string title, ILog log = null, Duration minTimeBeforeShowingUI = default) {
      this.title = title;
      this.minTimeBeforeShowingUi = minTimeBeforeShowingUI;
      this.log = (log ?? Log.@default).withScope($"{s(title)}:");
    }

    public void execute(string name, Action a) =>
      execute(name, () => { a(); return F.unit; });

    public A execute<A>(string name, Func<A> f) {
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
      _showProgressBar($"Running {s(name)}...", 0);
      lastProgressUIUpdate = lastProgressLogUpdate = DateTime.MinValue;
    }

    bool _progress(int idx, int total, bool cancellable) {
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
          canceled = _showProgressBar(msg, (float) item / total, cancellable);
          lastProgressUIUpdate = now;
        }
        if (updateLog) {
          log.info(msg);
          lastProgressLogUpdate = now;
        }
      }
      return canceled;
    }

    bool _showProgressBar(string msg, float progress, bool cancelable = false) {
      if ((DateTime.Now - creationTime).TotalSeconds >= minTimeBeforeShowingUi.seconds) {
        if (cancelable)
          return EditorUtility.DisplayCancelableProgressBar(title, msg, progress);
        EditorUtility.DisplayProgressBar(title, msg, progress);
      }
      return false;
    }

    public void progress(int idx, int total) => _progress(idx, total, false);

    /// <summary>Wrapper for Unity CancelableProgressBar</summary>
    /// <returns>True if task was canceled</returns>
    public bool progressCancellable(int idx, int total) => _progress(idx, total, true);

    /** A simple method to measure execution times between calls **/
    public void done() {
      var duration = new Duration(sw.ElapsedMilliseconds.toIntClamped());
      if (log.isInfo()) log.info($"{s(current)} done in {s(duration)}");
      _showProgressBar($"{s(current)} done in {s(duration)}.", 1);
      current = NONE;
    }

    public void Dispose() { EditorUtility.ClearProgressBar(); }
  }

}

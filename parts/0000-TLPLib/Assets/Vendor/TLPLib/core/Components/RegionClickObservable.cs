using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.InputUtils;
using com.tinylabproductions.TLPLib.Reactive;
using pzd.lib.data.dispose;
using pzd.lib.functional;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  /* Divides screen into X * Y grid and emits new region index when a pointer
   * moves between different regions.
   *
   * For example, if width=2, height=2, regions would be:
   *
   * /-------\
   * | 2 | 3 |
   * |---+---|
   * | 0 | 1 |
   * \-------/
   */
  public class RegionClickObservable {
    readonly Subject<int> _regionIndex = new Subject<int>();
    public IRxObservable<int> regionIndex => _regionIndex;

    readonly int gridWidth, gridHeight;

    int lastIndex = -1;

    public RegionClickObservable(int gridWidth=2, int gridHeight=2) {
      this.gridWidth = gridWidth;
      this.gridHeight = gridHeight;
      ASync.EveryFrame(() => {
        onUpdate();
        return true;
      });
    }

    struct SeqEntry {
      public readonly float time;
      public readonly int region;

      public SeqEntry(float time, int region) {
        this.time = time;
        this.region = region;
      }
    }

    /// <summary>
    /// Emits event when a particular region index sequence is executed within X seconds.
    /// </summary>
    public IRxObservable<Unit> sequenceWithinTimeframe(
      IDisposableTracker tracker, IList<int> sequence, float timeS,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // Specific implementation to reduce garbage.
      var s = new Subject<Unit>();
      var regions = new Queue<SeqEntry>(sequence.Count);
      bool isEqual() {
        var idx = 0;
        foreach (var entry in regions) {
          if (sequence[idx] != entry.region) return false;
          idx += 1;
        }
        return true;
      }
      regionIndex.subscribe(
        tracker, 
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName,
        callerFilePath: callerFilePath,
        callerLineNumber: callerLineNumber,
        // ReSharper restore ExplicitCallerInfoArgument
        onEvent: region => {
          // Clear up one item if the queue is full.
          if (regions.Count == sequence.Count) regions.Dequeue();
          regions.Enqueue(new SeqEntry(Time.realtimeSinceStartup, region));
          // Emit event if the conditions check out
          if (
            regions.Count == sequence.Count
            && Time.realtimeSinceStartup - regions.Peek().time <= timeS
            && isEqual()
          ) s.push(F.unit);
        }
      );
      return s;
    }

    void onUpdate() {
      var mp =
          Input.touchCount > 0 ? Input.GetTouch(0).position
        : Pointer.held ? Pointer.currentPosition
        : (Vector2?) null;
      if (mp.HasValue) {
        var val = mp.Value;
        var gridId = 0;
        gridId += Mathf.FloorToInt(val.x / (Screen.width / gridWidth));
        gridId += gridWidth * Mathf.FloorToInt(val.y / (Screen.height / gridHeight));
        if (gridId != lastIndex) {
          lastIndex = gridId;
          _regionIndex.push(gridId);
        }
      }
    }
  }
}

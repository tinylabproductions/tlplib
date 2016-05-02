using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.InputUtils;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components
{
  /* Divides screen into X * Y grid and emits new region index when a pointer 
   * moves between different regions. */
  public class RegionClickObservable : MonoBehaviour {
    private readonly Subject<int> _regionIndex = new Subject<int>();
    public IObservable<int> regionIndex => _regionIndex;

    private int lastIndex = -1;

    int gridWidth = 2;
    int gridHeight = 2;

    public RegionClickObservable init(int gridWidth, int gridHeight) {
      this.gridWidth = gridWidth;
      this.gridHeight = gridHeight;
      return this;
    }

    /* Emits event when a particular region index sequence is executed within X seconds */
    public IObservable<Unit> sequenceWithinTimeframe(IList<int> sequence, float timeS) {
      return regionIndex.withinTimeframe(sequence.Count, timeS).collect(list =>
        list.Select(t => t._1).zipWithIndex().Any(t => sequence[t._2] != t._1)
          ? F.none<Unit>() : F.some(F.unit)
      );
    }

    // ReSharper disable once UnusedMember.Local
    void Update() {
      if (Pointer.held) {
        var mp = Pointer.currentPosition;
        int gridId = 0;
        gridId += Mathf.FloorToInt(mp.x / (Screen.width / gridWidth));
        gridId += gridWidth * Mathf.FloorToInt(mp.y / (Screen.height / gridHeight));
        if (gridId != lastIndex) {
          lastIndex = gridId;
          _regionIndex.push(gridId);
        }
      }
    }
  }
}

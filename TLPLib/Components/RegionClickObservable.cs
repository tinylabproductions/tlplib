using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class RegionClickObservable : MonoBehaviour {
    private readonly Subject<int> _regionIndex = new Subject<int>();
    public IObservable<int> regionIndex { get { return _regionIndex; } }


    private int lastIndex = -1;

    int gridWidth = 2;
    int gridHeight = 2;

    public RegionClickObservable init(int gridWidth, int gridHeight) {
      this.gridWidth = gridWidth;
      this.gridHeight = gridHeight;
      return this;
    }

    public IObservable<Unit> sequenceWithinTimeframe(IList<int> sequence, float time) {
      return regionIndex.withinTimeframe(sequence.Count, time).collect(list =>
        list.Select(t => t._1).zipWithIndex().Any(t => sequence[t._2] != t._1)
          ? F.none<Unit>() : F.unit.some()
      );
    } 

    [UsedImplicitly]
    private void Update() {
      if (Input.GetMouseButton(0)) {
        var mp = (Vector2) Input.mousePosition;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinylabproductions.TLPLib.Components.Swiping {
  public struct SwipeData {
    public readonly Vector2 direction;
  }

  public interface ISwipeEventSource {
  }

  public class SwipeObservable : Observable<SwipeData> {
    float distanceThreshold;
    ISwipeEventSource eventSource;
    

    public SwipeObservable(float distanceThreshold, ISwipeEventSource eventSource) {
      this.distanceThreshold = distanceThreshold;
      this.eventSource = eventSource;
    }
  }

  
}
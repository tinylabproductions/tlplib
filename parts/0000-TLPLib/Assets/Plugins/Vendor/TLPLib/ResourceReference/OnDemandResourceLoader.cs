using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public partial class OnDemandResourceLoader<A> : IDisposable where A : Object {

    readonly DisposableTracker tracker = new DisposableTracker();
    readonly IRxRef<Option<ResourceRequest>> request = RxRef.a(F.none<ResourceRequest>());

    const int
      PRIORITY_HIGH = 2,
      PRIORITY_LOW = 1,
      // We can't cancel loading, so we just set to lowest possible priority instead
      // Priority can't be set to negative value - got this info from error message
      PRIORITY_OFF = 0;

    [Record]
    public partial struct IsLoading {
      public readonly bool value;
    }
    
    public readonly IObservable<Either<IsLoading, A>> assetStateChanged; 

    public OnDemandResourceLoader(
      IRxVal<Option<AssetLoader<A>>> currentLoader, IRxVal<bool> enableLoading, IRxVal<bool> highPriority 
    ) {
      assetStateChanged = currentLoader
        .flatMap(opt => enableLoading.map(b => b ? opt : F.none<AssetLoader<A>>()))
        .flatMap(opt => {
          discardPreviousRequest();
          foreach (var binding in opt) {
            var tpl = binding.loadAssetAsync();
            request.value = tpl._1.some();
            var future = tpl._2;
            return future.toRxVal().map(csOpt => csOpt.toRight(new IsLoading(true)));
          }
          return RxVal.cached(F.left<IsLoading, A>(new IsLoading(false)));
        }).asObservable();

      assetStateChanged.subscribe(tracker, e => {
        if (e.isRight) discardPreviousRequest();
      });
      
      enableLoading.zip(highPriority, request, 
        (show, highPrior, req) => F.t(show ? (highPrior ? PRIORITY_HIGH : PRIORITY_LOW) : PRIORITY_OFF, req)
      ).subscribe(tracker, tpl => {
        var priority = tpl._1;
        var req = tpl._2;
        foreach (var r in req) {
          r.priority = priority;
        }
      });
    }

    void discardPreviousRequest() {
      foreach (var r in request.value) r.priority = PRIORITY_OFF;
      request.value = F.none<ResourceRequest>();
    }

    public void Dispose() {
      tracker.Dispose();
    }
  }
}
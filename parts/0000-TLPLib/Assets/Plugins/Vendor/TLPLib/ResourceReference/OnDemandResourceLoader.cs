using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public class OnDemandResourceLoader<A> : IDisposable where A : Object {

    readonly DisposableTracker tracker = new DisposableTracker();
    readonly IRxRef<Option<ResourceRequest>> request = RxRef.a(F.none<ResourceRequest>());

    const int
      PRIORITY_HIGH = 2,
      PRIORITY_LOW = 1,
      // We can't cancel loading, so we just set to lowest possible priority instead
      // Priority can't be set to negative value - got this info from error message
      PRIORITY_OFF = 0;

    public delegate void OnAssetStateChanged(Option<A> currentAsset, bool isLoading);

    public OnDemandResourceLoader(
      IRxVal<Option<AssetLoader<A>>> currentLoader, IRxVal<bool> enableLoading, IRxVal<bool> highPriority, 
      OnAssetStateChanged onAssetStateChanged
    ) {

      var requestNumber = 0;
      tracker.track(
        currentLoader
          .flatMap(opt => enableLoading.map(b => b ? opt : F.none<AssetLoader<A>>()))
          .flatMap(opt => {
            requestNumber++;
            if (requestNumber == 2) {
              // BUG: workaround for flatMap bug
              // TODO: remove this when bug fixed
              // somehow it matters what this value is
              return RxVal.cached(F.left<bool, A>(true));
            }
            discardPreviousRequest();
            foreach (var binding in opt) {
              var tpl = binding.loadAssetAsync();
              request.value = tpl._1.some();
              var future = tpl._2;
              return future.toRxVal().map(csOpt => csOpt.toRight(true));
            }
            return RxVal.cached(F.left<bool, A>(false));
          })
          .subscribe(either => {
            var isLoading = either.fold(_ => _, _ => false);
            var assetOpt = either.rightValue;
            if (assetOpt.isSome) discardPreviousRequest();
            onAssetStateChanged(assetOpt, isLoading);
          })
      );
      tracker.track(enableLoading.zip(highPriority, request).subscribe(tpl => {
        var show = tpl._1;
        var highPrior = tpl._2;
        var request = tpl._3;
        foreach (var r in request) {
          r.priority = show ? (highPrior ? PRIORITY_HIGH : PRIORITY_LOW) : PRIORITY_OFF;
        }
      }));
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
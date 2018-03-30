using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public enum LoadPriority : byte { Low, High }

  /// <summary>
  /// A loader that only loads one item. If you set a new item to be loaded, old item
  /// is stopped loading.
  /// 
  /// Exposes <see cref="itemState"/>, which indicates the state of current load. 
  /// </summary>
  public partial class SingleItemLoader<A> : IDisposable {
    readonly DisposableTracker tracker = new DisposableTracker();
    readonly IRxRef<Option<IAsyncOperation>> request = RxRef.a(F.none<IAsyncOperation>());

    // At this moment we could use Fn<Tpl<ResourceRequest, Future<A>>> instead of IAssetLoader<A>,
    // but there is no gain in refactoring this now. Just a note.
    public readonly IRxRef<Option<ILoader<A>>> currentLoader = RxRef.a(Option<ILoader<A>>.None);
    public readonly IRxRef<LoadPriority> priority = RxRef.a(LoadPriority.High);
    public readonly IRxVal<Either<IsLoading, A>> itemState;

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

    public SingleItemLoader() {
      itemState = currentLoader.flatMap(opt => {
        discardPreviousRequest();
        foreach (var bindingLoader in opt) {
          var (_request, assetFtr) = bindingLoader.loadASync();
          request.value = _request.some();
          return assetFtr.toRxVal().map(csOpt => csOpt.toRight(new IsLoading(true)));
        }

        return RxVal.cached(F.left<IsLoading, A>(new IsLoading(false)));
      });

      itemState.subscribe(tracker, e => {
        if (e.isRight) discardPreviousRequest();
      });

      currentLoader.zip(priority, request, (show, _priority, req) =>
        F.t(show.isSome ? (_priority == LoadPriority.High ? PRIORITY_HIGH : PRIORITY_LOW) : PRIORITY_OFF, req)
      ).subscribe(tracker, tpl => {
        var (_priority, req) = tpl;
        foreach (var r in req) {
          r.priority = _priority;
        }
      });
    }

    void discardPreviousRequest() {
      foreach (var r in request.value) r.priority = PRIORITY_OFF;
      request.value = request.value.none;
    }

    public void Dispose() {
      tracker.Dispose();
    }
  }
}
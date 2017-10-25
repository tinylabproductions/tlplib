using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using ALog = com.tinylabproductions.TLPLib.Android.Bindings.android.util.Log;
#endif

namespace com.tinylabproductions.TLPLib.Android {
  public class AppAbandonedListener {
    readonly Subject<Unit> _onAppAbandonedViaPause = new Subject<Unit>();
    public readonly IObservable<Unit> onAppAbandoned;

    public AppAbandonedListener(Duration timeout) {
      var lastFlag = F.none<Ref<bool>>();
      if (Log.isInfo) Log.info($"Creating {nameof(onAppAbandoned)} with {timeout}.");
      ASync.onAppPause.subscribe(paused => {
        const string TAG = nameof(AppAbandonedListener);
        if (paused) {
          var r = Ref.a(true);
          lastFlag = r.some();
#if UNITY_ANDROID
          ALog.i(TAG, $"App paused, going to register abandon in {timeout}.");
          new JThread(() => {
            ALog.i(TAG, $"App paused from {nameof(JThread)}.");
            JThread.sleep(timeout);
            ALog.i(TAG, $"App paused wakeup after {timeout}, was not resumed = {r.value}.");
            if (r.value) _onAppAbandonedViaPause.push(F.unit);
          }).start();
#endif
        }
        else {
          foreach (var r in lastFlag) {
#if UNITY_ANDROID
            ALog.i(TAG, $"App resumed, clearing last flag.");
#endif
            r.value = false;
            lastFlag = lastFlag.none;
          }
        }
      });
      onAppAbandoned = _onAppAbandonedViaPause.join(ASync.onAppQuit);
      if (Log.isInfo) Log.info($"{nameof(onAppAbandoned)} with {timeout} created.");
    }
  }
}
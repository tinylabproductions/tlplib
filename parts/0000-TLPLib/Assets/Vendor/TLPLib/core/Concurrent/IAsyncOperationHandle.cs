using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.concurrent;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public interface IAsyncOperationHandle<A> {
    AsyncOperationStatus Status { get; }
    bool IsDone { get; }
    /// <summary>
    /// Combined progress of downloading from internet and loading from disk
    /// </summary>
    float PercentComplete { get; }
    /// <summary>
    /// Status about bytes that are downloaded from the internet
    /// </summary>
    DownloadStatus downloadStatus { get; }
    Future<Try<A>> asFuture { get; }
    void release();
  }

  public static class DownloadStatusExts {
    public static DownloadStatus join(this DownloadStatus a, DownloadStatus b) =>
      new DownloadStatus {
        DownloadedBytes = a.DownloadedBytes + b.DownloadedBytes,
        TotalBytes = a.TotalBytes + b.TotalBytes,
        IsDone = a.IsDone && b.IsDone
      };

    public static string debugStr(this DownloadStatus ds) =>
      $"{ds.DownloadedBytes}/{ds.TotalBytes} bytes ({ds.Percent * 100} %), isDone = {ds.IsDone}";
  }

  [PublicAPI] public static class IASyncOperationHandle_ {
    public static IAsyncOperationHandle<Unit> delayFrames(uint durationInFrames) => 
      new DelayAsyncOperationHandle<Unit>(durationInFrames, Unit._);
    
    public static IAsyncOperationHandle<A> delayFrames<A>(uint durationInFrames, A a) => 
      new DelayAsyncOperationHandle<A>(durationInFrames, a);

    public static IAsyncOperationHandle<Unit> done => DoneAsyncOperationHandle.instance;

    /// <summary>Launches given async operation, retrying it if it fails.</summary>
    /// <param name="launch"></param>
    /// <param name="tryCount">
    /// If None will retry forever. How many times we should try the operation? If lower than 1 will still try at least
    /// 1 time.
    /// </param>
    /// <param name="retryInterval"></param>
    /// <param name="timeContext"></param>
    public static IAsyncOperationHandle<A> withRetries<A>(
      Func<IAsyncOperationHandle<A>> launch, Option<uint> tryCount, Duration retryInterval, ITimeContext timeContext
    ) => new RetryingAsyncOperationHandle<A>(launch, tryCount, retryInterval, timeContext);
  }
  
  [PublicAPI] public static class IAsyncOperationHandleExts {
    public static IAsyncOperationHandle<A> wrap<A>(
      this AsyncOperationHandle<A> handle,
      Action<AsyncOperationHandle<A>> release
    ) => new WrappedAsyncOperationHandle<A>(handle, release);
    
    public static IAsyncOperationHandle<A> wrap<A>(
      this AsyncOperationHandle<A> handle,
      Action<A> onSuccess,
      Action<AsyncOperationHandle<A>> release
    ) {
      var result = new WrappedAsyncOperationHandle<A>(handle, release);
      result.asFuture.onSuccess(onSuccess);
      return result;
    }

    public static IAsyncOperationHandle<object> wrap(
      this AsyncOperationHandle handle, 
      Action<AsyncOperationHandle> release
    ) => new WrappedAsyncOperationHandle(handle, release);
    
    public static IAsyncOperationHandle<B> map<A, B>(
      this IAsyncOperationHandle<A> handle, Func<A, IAsyncOperationHandle<A>, B> mapper
    ) => new MappedAsyncOperationHandle<A, B>(handle, a => mapper(a, handle));
    
    public static IAsyncOperationHandle<B> flatMap<A, B>(
      this IAsyncOperationHandle<A> handle, Func<A, IAsyncOperationHandle<A>, IAsyncOperationHandle<B>> mapper, 
      float aHandleProgressPercentage=0.5f
    ) => new FlatMappedAsyncOperationHandle<A, B>(handle, a => mapper(a, handle), aHandleProgressPercentage);

    public static IAsyncOperationHandle<A> delayedFrames<A>(
      this IAsyncOperationHandle<A> handle, uint durationInFrames, 
      float aHandleProgressPercentage=0.5f
    ) => handle.flatMap(
      (a, h) => new DelayAsyncOperationHandle<A>(durationInFrames, a, h.release),
      aHandleProgressPercentage: aHandleProgressPercentage
    );

    public static IAsyncOperationHandle<ImmutableArrayC<A>> sequenceNonFailing<A>(
      this IReadOnlyCollection<IAsyncOperationHandle<A>> collection
    ) => new SequencedNonFailingAsyncOperationHandle<A>(collection);

    public static IAsyncOperationHandle<ImmutableArrayC<Try<A>>> sequence<A>(
      this IReadOnlyCollection<IAsyncOperationHandle<A>> collection
    ) => new SequencedAsyncOperationHandle<A>(collection);
    
    public static Try<A> toTry<A>(this IAsyncOperationHandle<A> handle) {
      if (handle.asFuture.value.valueOut(out var val)) return val;
      return Try<A>.failed(new Exception("Handle is not completed!"));
    }
  }
  
#region implementations
  [Record] sealed partial class HandleStatusOnRelease {
    public readonly AsyncOperationStatus status;
    public readonly float percentComplete;
    public readonly DownloadStatus downloadStatus;

    public bool isDone => status != AsyncOperationStatus.None;
  }

  public sealed partial class WrappedAsyncOperationHandle<A> : IAsyncOperationHandle<A> {
    readonly AsyncOperationHandle<A> handle;
    readonly Action<AsyncOperationHandle<A>> _release;

    Option<HandleStatusOnRelease> released = None._;
    
    public WrappedAsyncOperationHandle(
      AsyncOperationHandle<A> handle, Action<AsyncOperationHandle<A>> release
    ) {
      this.handle = handle;
      _release = release;
    }

    public AsyncOperationStatus Status => released.valueOut(out var r) ? r.status : handle.Status;
    public bool IsDone => released.valueOut(out var r) ? r.isDone : handle.IsDone;
    public float PercentComplete => released.valueOut(out var r) ? r.percentComplete : handle.PercentComplete;
    public DownloadStatus downloadStatus => released.valueOut(out var r) ? r.downloadStatus : handle.GetDownloadStatus();
    [LazyProperty] public Future<Try<A>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      var data = new HandleStatusOnRelease(handle.Status, handle.PercentComplete, handle.GetDownloadStatus());
      _release(handle);
      released = Some.a(data);
    }
  }
  
  public sealed class WrappedAsyncOperationHandle : IAsyncOperationHandle<object> {
    readonly AsyncOperationHandle handle;
    readonly Action<AsyncOperationHandle> _release;

    Option<HandleStatusOnRelease> released = None._;
    
    public WrappedAsyncOperationHandle(
      AsyncOperationHandle handle, Action<AsyncOperationHandle> release
    ) {
      this.handle = handle;
      _release = release;
    }

    public AsyncOperationStatus Status => released.valueOut(out var r) ? r.status : handle.Status;
    public bool IsDone => released.valueOut(out var r) ? r.isDone : handle.IsDone;
    public float PercentComplete => released.valueOut(out var r) ? r.percentComplete : handle.PercentComplete;
    public DownloadStatus downloadStatus => released.valueOut(out var r) ? r.downloadStatus : handle.GetDownloadStatus();
    public Future<Try<object>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      var data = new HandleStatusOnRelease(handle.Status, handle.PercentComplete, handle.GetDownloadStatus());
      _release(handle);
      released = Some.a(data);
    }
  }

  public sealed class MappedAsyncOperationHandle<A, B> : IAsyncOperationHandle<B> {
    readonly IAsyncOperationHandle<A> handle;
    readonly Func<A, B> mapper;

    public MappedAsyncOperationHandle(IAsyncOperationHandle<A> handle, Func<A, B> mapper) {
      this.handle = handle;
      this.mapper = mapper;
    }

    public AsyncOperationStatus Status => handle.Status;
    public bool IsDone => handle.IsDone;
    public float PercentComplete => handle.PercentComplete;
    public DownloadStatus downloadStatus => handle.downloadStatus;
    [LazyProperty] public Future<Try<B>> asFuture => handle.asFuture.map(try_ => try_.map(mapper));
    public void release() => handle.release();
  }

  public sealed class FlatMappedAsyncOperationHandle<A, B> : IAsyncOperationHandle<B> {
    readonly IAsyncOperationHandle<A> aHandle;
    readonly Future<Try<IAsyncOperationHandle<B>>> bHandleF;
    readonly float aHandleProgressPercentage;
    float bHandleProgressPercentage => 1 - aHandleProgressPercentage;

    public FlatMappedAsyncOperationHandle(
      IAsyncOperationHandle<A> handle, Func<A, IAsyncOperationHandle<B>> mapper, float aHandleProgressPercentage
    ) {
      aHandle = handle;
      if (aHandleProgressPercentage < 0 || aHandleProgressPercentage > 1)
        Log.d.error($"{aHandleProgressPercentage.echo()} not within [0..1], clamping");
      this.aHandleProgressPercentage = Mathf.Clamp01(aHandleProgressPercentage);
      bHandleF = handle.asFuture.map(try_ => try_.map(mapper));
    }

    public AsyncOperationStatus Status => 
      bHandleF.value.valueOut(out var b) ? b.fold(h => h.Status, e => AsyncOperationStatus.Failed) : aHandle.Status;

    public bool IsDone => bHandleF.value.valueOut(out var b) && b.fold(h => h.IsDone, e => true);
    
    public float PercentComplete => 
      bHandleF.value.valueOut(out var b) 
        ? aHandleProgressPercentage + b.fold(h => h.PercentComplete, e => 1) * bHandleProgressPercentage 
        : aHandle.PercentComplete * aHandleProgressPercentage;

    public DownloadStatus downloadStatus {
      get {
        if (bHandleF.value.valueOut(out var b)) {
          return b.fold(h => h.downloadStatus, e => aHandle.downloadStatus);
        }
        return aHandle.downloadStatus;
      }
    }

    public Future<Try<B>> asFuture => bHandleF.flatMapT(bHandle => bHandle.asFuture);

    public void release() {
      { if (bHandleF.value.valueOut(out var b) && b.valueOut(out var h)) h.release(); }
      aHandle.release();
    }
  }

  public sealed class DelayAsyncOperationHandle<A> : IAsyncOperationHandle<A> {
    public readonly uint startedAtFrame, endAtFrame, durationInFrames;
    readonly Option<Action> onRelease;
    readonly A value;

    public DelayAsyncOperationHandle(uint durationInFrames, A value, Action onRelease=null) {
      this.durationInFrames = durationInFrames;
      startedAtFrame = Time.frameCount.toUIntClamped();
      endAtFrame = startedAtFrame + durationInFrames;
      this.value = value;
      this.onRelease = onRelease.opt();
    }

    long framesPassed => Time.frameCount - startedAtFrame;
    long framesLeft => endAtFrame - Time.frameCount;

    public override string ToString() => 
      $"{nameof(DelayAsyncOperationHandle<A>)}({startedAtFrame.echo()}, {endAtFrame.echo()})";

    public AsyncOperationStatus Status => IsDone ? AsyncOperationStatus.Succeeded : AsyncOperationStatus.None;
    public bool IsDone => Time.frameCount >= endAtFrame;
    public float PercentComplete => Mathf.Clamp01(framesPassed / (float) durationInFrames);
    public DownloadStatus downloadStatus => new DownloadStatus { IsDone = IsDone };

    public Future<Try<A>> asFuture {
      get {
        var left = framesLeft;
        return left <= 0 
          ? Future.successful(Try.value(value)) : FutureU.delayFrames(left.toIntClamped(), Try.value(value));
      }
    }

    public void release() { if (onRelease.valueOut(out var action)) action(); }
  }

  [Singleton] public sealed partial class DoneAsyncOperationHandle : IAsyncOperationHandle<Unit> {
    public AsyncOperationStatus Status => AsyncOperationStatus.Succeeded;
    public bool IsDone => true;
    public float PercentComplete => 1;
    public DownloadStatus downloadStatus => new DownloadStatus { IsDone = IsDone };
    public Future<Try<Unit>> asFuture => Future.successful(Try.value(Unit._));
    public void release() {}
  }

  public sealed class SequencedAsyncOperationHandle<A> : IAsyncOperationHandle<ImmutableArrayC<Try<A>>> {
    public readonly IReadOnlyCollection<IAsyncOperationHandle<A>> handles;

    public SequencedAsyncOperationHandle(IReadOnlyCollection<IAsyncOperationHandle<A>> handles) => 
      this.handles = handles;

    public AsyncOperationStatus Status {
      get {
        foreach (var handle in handles) {
          switch (handle.Status) {
            case AsyncOperationStatus.None: return AsyncOperationStatus.None;
            case AsyncOperationStatus.Failed: return AsyncOperationStatus.Failed;
            case AsyncOperationStatus.Succeeded: break;
            default: throw new ArgumentOutOfRangeException();
          }
        }

        return AsyncOperationStatus.Succeeded;
      }
    }
    public bool IsDone => handles.Count == 0 || handles.All(_ => _.IsDone);
    public float PercentComplete => handles.Count == 0 ? 1 : handles.Average(_ => _.PercentComplete);
    public DownloadStatus downloadStatus => handles.Aggregate(
      new DownloadStatus { IsDone = true }, 
      (a, b) => a.join(b.downloadStatus)
    ); 

    public Future<Try<ImmutableArrayC<Try<A>>>> asFuture =>
      handles.Select(h => h.asFuture).parallel().map(arr => Try.value(ImmutableArrayC.move(arr)));
    public void release() { foreach (var handle in handles) handle.release(); }
  }

  public sealed class SequencedNonFailingAsyncOperationHandle<A> : IAsyncOperationHandle<ImmutableArrayC<A>> {
    public readonly IReadOnlyCollection<IAsyncOperationHandle<A>> handles;

    public SequencedNonFailingAsyncOperationHandle(IReadOnlyCollection<IAsyncOperationHandle<A>> handles) => 
      this.handles = handles;

    public AsyncOperationStatus Status {
      get {
        foreach (var handle in handles) {
          switch (handle.Status) {
            case AsyncOperationStatus.None: return AsyncOperationStatus.None;
            case AsyncOperationStatus.Failed: return AsyncOperationStatus.Failed;
            case AsyncOperationStatus.Succeeded: break;
            default: throw new ArgumentOutOfRangeException();
          }
        }

        return AsyncOperationStatus.Succeeded;
      }
    }
    public bool IsDone => handles.Count == 0 || handles.All(_ => _.IsDone);
    public float PercentComplete => handles.Count == 0 ? 1 : handles.Average(_ => _.PercentComplete);
    public DownloadStatus downloadStatus => handles.Aggregate(
      new DownloadStatus { IsDone = true }, 
      (a, b) => a.join(b.downloadStatus)
    ); 

    public Future<Try<ImmutableArrayC<A>>> asFuture =>
      handles.Select(h => h.asFuture).parallel().map(arr => arr.sequence().map(_ => _.toImmutableArrayC()));
    public void release() { foreach (var handle in handles) handle.release(); }
  }

  public sealed class RetryingAsyncOperationHandle<A> : IAsyncOperationHandle<A> {
    enum State : byte { Launched, WaitingToRetry, Finished, Released }
    
    readonly Func<IAsyncOperationHandle<A>> launchRaw;
    readonly Option<uint> tryCount;
    readonly Duration retryInterval;
    readonly ITimeContext timeContext;
    readonly Future<IAsyncOperationHandle<A>> finalHandleFuture;
    readonly Promise<IAsyncOperationHandle<A>> finalHandlePromise;

    uint retryNo = 1;
    State state;
    IAsyncOperationHandle<A> current;
    IDisposable currentRetryWait = F.emptyDisposable;

    public RetryingAsyncOperationHandle(
      Func<IAsyncOperationHandle<A>> launch, Option<uint> tryCount, Duration retryInterval, ITimeContext timeContext
    ) {
      launchRaw = launch;
      this.tryCount = tryCount;
      this.retryInterval = retryInterval;
      this.timeContext = timeContext;
      finalHandleFuture = Future.async(out finalHandlePromise);

      this.launch();
    }

    public AsyncOperationStatus Status => current.Status;
    public bool IsDone => current.IsDone;
    public float PercentComplete => current.PercentComplete;
    public DownloadStatus downloadStatus => current.downloadStatus;
    public Future<Try<A>> asFuture => finalHandleFuture.flatMap(h => h.asFuture);

    public void release() {
      current.release();
      state = State.Released;
      currentRetryWait.Dispose();
    }

    void launch() {
      var handle = current = launchRaw();
      state = State.Launched;
      handle.asFuture.onComplete(try_ => {
        if (state == State.Released) return;
        
        try_.voidFold(
          // Success!
          a => {
            state = State.Finished;
            finalHandlePromise.complete(handle);
          },
          err => {
            if (!tryCount.valueOut(out var count) || retryNo < count) {
              // Retry
              retryNo++;
              state = State.WaitingToRetry;
              currentRetryWait = timeContext.after(
                retryInterval, name: nameof(RetryingAsyncOperationHandle<A>), act: launch
              );
            }
            else {
              // We've run out of retries, complete with what we had last.
              state = State.Finished;
              finalHandlePromise.complete(handle);
            }
          }
        );
      });
    }
  }
#endregion
}
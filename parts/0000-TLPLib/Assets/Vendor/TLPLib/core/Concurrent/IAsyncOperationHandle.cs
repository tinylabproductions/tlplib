using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinylabproductions.TLPLib.Concurrent {
  [PublicAPI] public interface IAsyncOperationHandle<A> {
    AsyncOperationStatus Status { get; }
    bool IsDone { get; }
    float PercentComplete { get; }
    Future<Try<A>> asFuture { get; }
    Try<A> toTry();
    void release();
  }

  [PublicAPI] public static class IASyncOperationHandle_ {
    public static IAsyncOperationHandle<Unit> delayFrames(uint durationInFrames) => 
      new DelayAsyncOperationHandle<Unit>(durationInFrames, Unit._);
    
    public static IAsyncOperationHandle<A> delayFrames<A>(uint durationInFrames, A a) => 
      new DelayAsyncOperationHandle<A>(durationInFrames, a);

    public static IAsyncOperationHandle<Unit> done => DoneAsyncOperationHandle.instance;
  }
  
  [PublicAPI] public static class IAsyncOperationHandleExts {
    public static IAsyncOperationHandle<A> wrap<A>(
      this AsyncOperationHandle<A> handle,
      Action<AsyncOperationHandle<A>> release
    ) => new WrappedAsyncOperationHandle<A>(handle, release);
    
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
      this IAsyncOperationHandle<A> handle, uint durationInFrames
    ) => handle.flatMap((a, h) => new DelayAsyncOperationHandle<A>(durationInFrames, a, h.release));
  }
  
#region implementations
  [Record] sealed partial class HandleStatusOnRelease {
    public readonly AsyncOperationStatus status;
    public readonly float percentComplete;

    public bool isDone => status != AsyncOperationStatus.None;
  }

  public sealed class WrappedAsyncOperationHandle<A> : IAsyncOperationHandle<A> {
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
    public Try<A> toTry() => handle.toTry();
    [LazyProperty] public Future<Try<A>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      var data = new HandleStatusOnRelease(handle.Status, handle.PercentComplete);
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
    public Try<object> toTry() => handle.toTry();
    [LazyProperty] public Future<Try<object>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      var data = new HandleStatusOnRelease(handle.Status, handle.PercentComplete);
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
    [LazyProperty] public Future<Try<B>> asFuture => handle.asFuture.map(try_ => try_.map(mapper));
    public Try<B> toTry() => handle.toTry().map(mapper);
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

    [LazyProperty] public Future<Try<B>> asFuture => bHandleF.flatMapT(bHandle => bHandle.asFuture);

    public Try<B> toTry() =>
      bHandleF.value.valueOut(out var b) 
        ? b.flatMap(h => h.toTry()) 
        : Try<B>.failed(new Exception($"{aHandle} hasn't completed yet!"));

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

    [LazyProperty] public Future<Try<A>> asFuture {
      get {
        var left = framesLeft;
        return left <= 0 
          ? Future.successful(Try.value(value)) : Future.delayFrames(left.toIntClamped(), Try.value(value));
      }
    }

    public Try<A> toTry() => 
      IsDone ? Try.value(value) : Try<A>.failed(new Exception($"{ToString()} isn't finished yet!"));

    public void release() { if (onRelease.valueOut(out var action)) action(); }
  }

  [Singleton] public sealed partial class DoneAsyncOperationHandle : IAsyncOperationHandle<Unit> {
    public AsyncOperationStatus Status => AsyncOperationStatus.Succeeded;
    public bool IsDone => true;
    public float PercentComplete => 1;
    public Future<Try<Unit>> asFuture => Future.successful(toTry());
    public Try<Unit> toTry() => Try.value(Unit._);
    public void release() {}
  }
#endregion
}
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
  }
  
#region implementations
  public sealed class WrappedAsyncOperationHandle<A> : IAsyncOperationHandle<A> {
    readonly AsyncOperationHandle<A> handle;
    readonly Action<AsyncOperationHandle<A>> _release;

    bool released;
    
    public WrappedAsyncOperationHandle(
      AsyncOperationHandle<A> handle, Action<AsyncOperationHandle<A>> release
    ) {
      this.handle = handle;
      _release = release;
    }

    public AsyncOperationStatus Status => handle.Status;
    public bool IsDone => handle.IsDone;
    public float PercentComplete => handle.PercentComplete;
    public Try<A> toTry() => handle.toTry();
    [LazyProperty] public Future<Try<A>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      _release(handle);
      released = true;
    }
  }
  
  public sealed class WrappedAsyncOperationHandle : IAsyncOperationHandle<object> {
    readonly AsyncOperationHandle handle;
    readonly Action<AsyncOperationHandle> _release;

    bool released;
    
    public WrappedAsyncOperationHandle(
      AsyncOperationHandle handle, Action<AsyncOperationHandle> release
    ) {
      this.handle = handle;
      _release = release;
    }

    public AsyncOperationStatus Status => handle.Status;
    public bool IsDone => handle.IsDone;
    public float PercentComplete => handle.PercentComplete;
    public Try<object> toTry() => handle.toTry();
    [LazyProperty] public Future<Try<object>> asFuture => handle.toFuture().map(h => h.toTry());

    public void release() {
      if (released) return;
      _release(handle);
      released = true;
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
#endregion
}
using System;
using JetBrains.Annotations;
using Smooth.Pools;

namespace Smooth.Dispose {
	// Unity runtime does not like variant interfaces. They occasionally crash.
	// ReSharper disable once TypeParameterCanBeVariant
	public interface IDisposable<A> : IDisposable {
		A value { get; }
	}
	
	/// <summary>
	/// Wrapper around a value that uses the IDisposable interface to dispose of the value.
	///
	/// On IOS, this is a value type to avoid compute_class_bitmap errors.
	///
	/// On other platforms, it is a pooled object to avoid boxing when disposed by a using block with the Unity compiler.
	/// </summary>
	public class Disposable<T> : IDisposable<T> {
		static readonly Pool<Disposable<T>> pool = new Pool<Disposable<T>>(
			() => new Disposable<T>(),
			wrapper => {
				wrapper.dispose(wrapper.value);
				wrapper.dispose = t => {};
				wrapper.value = default;
			}
		);

		/// <summary>
		/// Borrows a wrapper for the specified value and disposal delegate.
		/// </summary>
		public static Disposable<T> Borrow(T value, Act<T> dispose) {
			var wrapper = pool.Borrow();
			wrapper.value = value;
			wrapper.dispose = dispose;
			return wrapper;
		}

		Act<T> dispose;

		/// <summary>
		/// The wrapped value.
		/// </summary>
		public T value { get; private set; }

		Disposable() {}

		public override string ToString() => $"{nameof(Disposable<T>)}({value})";

		/// <summary>
		/// Relinquishes ownership of the wrapper, disposes the wrapped value, and returns the wrapper to the pool.
		/// </summary>
		public void Dispose() => pool.Release(this);

		/// <summary>
		/// Relinquishes ownership of the wrapper and adds it to the disposal queue.
		/// </summary>
		public void DisposeInBackground() => DisposalQueue.Enqueue(this);
	}

	class UnpooledDisposable<A> : IDisposable<A> {
		public A value { get; }
		readonly Act<A> dispose;

		public UnpooledDisposable(A value, Act<A> dispose) {
			this.dispose = dispose;
			this.value = value;
		}

		public void Dispose() => dispose(value);
	}
	
	public static class Disposable {
		[PublicAPI]
		public static IDisposable<A> pooled<A>(A value, Act<A> dispose) =>
			Disposable<A>.Borrow(value, dispose);
		
		[PublicAPI]
		public static IDisposable<A> unpooled<A>(A value, Act<A> dispose) =>
			new UnpooledDisposable<A>(value, dispose);
	}
}
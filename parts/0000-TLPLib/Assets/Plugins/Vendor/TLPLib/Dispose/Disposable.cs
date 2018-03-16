using System;
using JetBrains.Annotations;
using Smooth.Pools;

namespace Smooth.Dispose {
	/// <summary>
	/// Wrapper around a value that uses the IDisposable interface to dispose of the value.
	///
	/// On IOS, this is a value type to avoid compute_class_bitmap errors.
	///
	/// On other platforms, it is a pooled object to avoid boxing when disposed by a using block with the Unity compiler.
	/// </summary>
	public class Disposable<T> : IDisposable {
		static readonly Pool<Disposable<T>> pool = new Pool<Disposable<T>>(
			() => new Disposable<T>(),
			wrapper => {
				wrapper.dispose(wrapper.value);
				wrapper.dispose = t => {};
				wrapper.value = default(T);
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
		
		[PublicAPI]
		public static Disposable<T> createUnpooled(T value, Act<T> dispose) =>
			new Disposable<T> { value = value, dispose = dispose };

		/// <summary>
		/// Relinquishes ownership of the wrapper, disposes the wrapped value, and returns the wrapper to the pool.
		/// </summary>
		public void Dispose() => pool.Release(this);

		/// <summary>
		/// Relinquishes ownership of the wrapper and adds it to the disposal queue.
		/// </summary>
		public void DisposeInBackground() => DisposalQueue.Enqueue(this);
	}

	public static class Disposable {
		[PublicAPI]
		public static Disposable<T> createUnpooled<T>(T value, Act<T> dispose) =>
			Disposable<T>.createUnpooled(value, dispose);
	}
}
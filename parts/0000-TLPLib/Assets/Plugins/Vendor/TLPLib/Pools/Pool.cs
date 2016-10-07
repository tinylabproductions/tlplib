using System;
using System.Collections.Generic;
using Smooth.Dispose;

namespace Smooth.Pools {
	/// <summary>
	/// Pool that lends values of type T.
	/// </summary>
	public class Pool<T> {
		private readonly Stack<T> values = new Stack<T>();

		private readonly Fn<T> create;
		private readonly Act<T> reset;
		private readonly Act<T> release;

		private Pool() {}
		
		/// <summary>
		/// Creates a new pool with the specified value creation and reset delegates.
		/// </summary>
		public Pool(Fn<T> create, Act<T> reset) {
			this.create = create;
			this.reset = reset;
			this.release = Release;
		}

		/// <summary>
		/// Borrows a value from the pool.
		/// </summary>
		public T Borrow() {
			lock (values) {
				return values.Count > 0 ? values.Pop() : create();
			}
		}
		
		/// <summary>
		/// Relinquishes ownership of the specified value and returns it to the pool.
		/// </summary>
		public void Release(T value) {
			reset(value);
			lock (values) {
				values.Push(value);
			}
		}

		/// <summary>
		/// Borrows a wrapped value from the pool.
		/// </summary>
		public Disposable<T> BorrowDisposable() {
			return Disposable<T>.Borrow(Borrow(), release);
		}
	}
}
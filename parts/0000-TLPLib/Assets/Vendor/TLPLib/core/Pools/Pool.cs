using System;
using System.Collections.Generic;
using pzd.lib.dispose;
using Smooth.Dispose;

namespace Smooth.Pools {
	/// <summary>
	/// Pool that lends values of type T.
	/// </summary>
	public class Pool<T> {
		private readonly Stack<T> values = new Stack<T>();

		private readonly Func<T> create;
		private readonly Action<T> reset;
		private readonly Action<T> release;

		private Pool() {}

		/// <summary>
		/// Creates a new pool with the specified value creation and reset delegates.
		/// </summary>
		public Pool(Func<T> create, Action<T> reset) {
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
			return new Disposable<T>(Borrow(), release);
		}
	}
}
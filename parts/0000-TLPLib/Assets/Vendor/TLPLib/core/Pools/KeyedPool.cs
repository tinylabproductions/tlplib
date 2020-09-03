using System;
using System.Collections.Generic;
using pzd.lib.dispose;

namespace Smooth.Pools {
	/// <summary>
	/// Pool that lends values of type T with an associated key of type K.
	/// </summary>
	public class KeyedPool<K, T> {
		private readonly Dictionary<K, Stack<T>> keyToValues = new Dictionary<K, Stack<T>>();

		private readonly Func<K, T> create;
		private readonly Func<T, K> reset;
		private readonly Action<T> release;

		private KeyedPool() {}

		/// <summary>
		/// Creates a new keyed pool with the specified value creation and reset delegates.
		/// </summary>
		public KeyedPool(Func<K, T> create, Func<T, K> reset) {
			this.create = create;
			this.reset = reset;
			this.release = Release;
		}

		/// <summary>
		/// Borrows a value with the specified key from the pool.
		/// </summary>
		public T Borrow(K key) {
			lock (keyToValues) {
				Stack<T> values;
				return keyToValues.TryGetValue(key, out values) && values.Count > 0 ? values.Pop() : create(key);
			}
		}

		/// <summary>
		/// Relinquishes ownership of the specified value and returns it to the pool.
		/// </summary>
		public void Release(T value) {
			var key = reset(value);
			lock (keyToValues) {
				Stack<T> values;
				if (!keyToValues.TryGetValue(key, out values)) {
					values = new Stack<T>();
					keyToValues[key] = values;
				}
				values.Push(value);
			}
		}

		/// <summary>
		/// Borrows a wrapped value with the specified key from the pool.
		/// </summary>
		public Disposable<T> BorrowDisposable(K key) {
			return new Disposable<T>(Borrow(key), release);
		}
	}
}
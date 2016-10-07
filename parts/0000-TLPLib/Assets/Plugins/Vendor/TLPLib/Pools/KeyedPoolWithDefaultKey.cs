using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;

using Smooth.Dispose;

namespace Smooth.Pools {
	/// <summary>
	/// Pool that lends values of type T with an associated key of type K and defines a default key.
	/// </summary>
	public class KeyedPoolWithDefaultKey<K, T> : KeyedPool<K, T> {
		private readonly Either<K, Fn<K>> defaultKey;

		/// <summary>
		/// Creates a new keyed pool with the specified creation delegate, reset delegate, and default key.
		/// </summary>
		public KeyedPoolWithDefaultKey(Fn<K, T> create, Fn<T, K> reset, K defaultKey) : base (create, reset) {
			this.defaultKey = Either<K, Fn<K>>.Left(defaultKey);
		}

		/// <summary>
		/// Creates a new keyed pool with the specified creation delegate, reset delegate, and default key.
		/// </summary>
		public KeyedPoolWithDefaultKey(Fn<K, T> create, Fn<T, K> reset, Fn<K> defaultKeyFunc) : base (create, reset) {
			this.defaultKey = Either<K, Fn<K>>.Right(defaultKeyFunc);
		}

		/// <summary>
		/// Borrows a value with the default key from the pool.
		/// </summary>
		public T Borrow() {
			return Borrow(defaultKey.fold(_ => _, _ => _()));
		}

		/// <summary>
		/// Borrows a wrapped value with the default key from the pool.
		/// </summary>
		public Disposable<T> BorrowDisposable() {
			return BorrowDisposable(defaultKey.fold(_ => _, _ => _()));
		}
	}
}
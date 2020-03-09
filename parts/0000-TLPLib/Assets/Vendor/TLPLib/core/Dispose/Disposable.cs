using System;

namespace Smooth.Dispose {
	public class Disposable<A> : IDisposable {
		public readonly A value;
		readonly Action<A> dispose;

		public Disposable(A value, Action<A> dispose) {
			this.dispose = dispose;
			this.value = value;
		}

		public void Dispose() => dispose(value);
	}
}
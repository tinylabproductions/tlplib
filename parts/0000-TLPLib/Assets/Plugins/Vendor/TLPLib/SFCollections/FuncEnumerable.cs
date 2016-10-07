using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;


namespace Smooth.Collections {
	/// <summary>
	/// Enumerable that contains the elements defined by a seed value and step function.
	/// </summary>
	public class FuncEnumerable<T> : IEnumerable<T> {
		private readonly T seed;
		private readonly Either<Fn<T, T>, Fn<T, Option<T>>> step;

		private FuncEnumerable() {}

		public FuncEnumerable(T seed, Fn<T, T> step) {
			this.seed = seed;
			this.step = Either<Fn<T, T>, Fn<T, Option<T>>>.Left(step);
		}
		
		public FuncEnumerable(T seed, Fn<T, Option<T>> step) {
			this.seed = seed;
			this.step = Either<Fn<T, T>, Fn<T, Option<T>>>.Right(step);
		}

		public IEnumerator<T> GetEnumerator() {
			if (step.isLeft) {
				var current = seed;
				while (true) {
					yield return current;
					current = step.leftValue.get(current);
				}
			} else {
				var current = new Option<T>(seed);
				while (current.isSome) {
					yield return current.get;
					current = step.rightValue.get(current.get);
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// Enumerable that contains the elements defined by a seed value and step function.
	/// </summary>
	public class FuncEnumerable<T, P> : IEnumerable<T> {
		private readonly T seed;
		private readonly Either<Fn<T, P, T>, Fn<T, P, Option<T>>> step;
		private readonly P parameter;
		
		private FuncEnumerable() {}
		
		public FuncEnumerable(T seed, Fn<T, P, T> step, P parameter) {
			this.seed = seed;
			this.step = Either<Fn<T, P, T>, Fn<T, P, Option<T>>>.Left(step);
			this.parameter = parameter;
		}
		
		public FuncEnumerable(T seed, Fn<T, P, Option<T>> step, P parameter) {
			this.seed = seed;
			this.step = Either<Fn<T, P, T>, Fn<T, P, Option<T>>>.Right(step);
			this.parameter = parameter;
		}
		
		public IEnumerator<T> GetEnumerator() {
			if (step.isLeft) {
				var current = seed;
				while (true) {
					yield return current;
					current = step.leftValue.get(current, parameter);
				}
			} else {
				var current = new Option<T>(seed);
				while (current.isSome) {
					yield return current.get;
					current = step.rightValue.get(current.get, parameter);
				}
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

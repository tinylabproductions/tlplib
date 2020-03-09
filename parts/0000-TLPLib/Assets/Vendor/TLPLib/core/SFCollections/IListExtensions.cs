using System;
using System.Collections.Generic;
using Smooth.Comparisons;

namespace Smooth.Collections {

	/// <summary>
	/// Extension methods for IList<>s.
	/// </summary>
	public static class IListExtensions {
		#region Sort

		/// <summary>
		/// Sorts the specified list using an insertion sort algorithm and the default sort comparer for T.
		/// </summary>
		/// <remarks>
		/// Insertion sort is a O(n²) time complexity algorithm and should not be used on arbitrary lists.
		/// However, it has a best case time complexity of O(n) for lists that are already sorted and is quite fast when used on nearly sorted input.
		/// </remarks>
		public static void InsertionSort<T>(this IList<T> ts) {
			InsertionSort(ts, Comparer<T>.Default);
		}

		/// <summary>
		/// Sorts the specified list using an insertion sort algorithm and the specified comparer.
		/// </summary>
		/// <remarks>
		/// Insertion sort is a O(n²) time complexity algorithm and should not be used on arbitrary lists.
		/// However, it has a best case time complexity of O(n) for lists that are already sorted and is quite fast when used on nearly sorted input.
		/// </remarks>
		public static void InsertionSort<T>(this IList<T> ts, IComparer<T> comparer) {
			InsertionSort(ts, Comparisons<T>.ToComparison(comparer));
		}

		/// <summary>
		/// Sorts the specified list using an insertion sort algorithm and the specified comparison.
		/// </summary>
		/// <remarks>
		/// Insertion sort is a O(n²) time complexity algorithm and should not be used on arbitrary lists.
		/// However, it has a best case time complexity of O(n) for lists that are already sorted and is quite fast when used on nearly sorted input.
		/// </remarks>
		public static void InsertionSort<T>(this IList<T> ts, Comparison<T> comparison) {
			for (int right = 1; right < ts.Count; ++right) {
				var insert = ts[right];
				var left = right - 1;
				while (left >= 0 && comparison(ts[left], insert) > 0) {
					ts[left + 1] = ts[left];
					--left;
				}
				ts[left + 1] = insert;
			}
		}

		#endregion
	}
}

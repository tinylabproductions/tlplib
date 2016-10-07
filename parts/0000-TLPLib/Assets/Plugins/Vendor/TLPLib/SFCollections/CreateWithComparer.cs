using System;
using System.Collections.Generic;

namespace Smooth.Collections {

	#region Dictionary

	/// <summary>
	/// Helper class for instantiating Dictionary<>s without specifying a comparer.
	/// </summary>
	public static class Dictionary {
		
		/// <summary>
		/// Creates a new dictionary with the default comparer.
		/// </summary>
		public static Dictionary<K, V> Create<K, V>() {
			return new Dictionary<K, V>(Smooth.Collections.EqComparer<K>.Default);
		}
		
		/// <summary>
		/// Creates a new dictionary with the default comparer and the specified initial capacity.
		/// </summary>
		public static Dictionary<K, V> Create<K, V>(int capacity) {
			return new Dictionary<K, V>(capacity, Smooth.Collections.EqComparer<K>.Default);
		}
		
		/// <summary>
		/// Creates a new dictionary with the default comparer and elements copied from the specified dictionary.
		/// </summary>
		public static Dictionary<K, V> Create<K, V>(IDictionary<K, V> dictionary) {
			return new Dictionary<K, V>(dictionary, Smooth.Collections.EqComparer<K>.Default);
		}
		
	}

	#endregion

	#region HashSet
	
	/// <summary>
	/// Helper class for instantiating HashSet<>s without specifying a comparer.
	/// </summary>
	public static class HashSet {
		
		/// <summary>
		/// Creates a new hash set with the default comparer.
		/// </summary>
		public static HashSet<T> Create<T>() {
			return new HashSet<T>(Smooth.Collections.EqComparer<T>.Default);
		}
		
		/// <summary>
		/// Creates a new hash set with the default comparer and elements copied from the specified collection.
		/// </summary>
		public static HashSet<T> Create<T>(IEnumerable<T> collection) {
			return new HashSet<T>(collection, Smooth.Collections.EqComparer<T>.Default);
		}

	}
	
	#endregion

	#region SortedDictionary
	
	/// <summary>
	/// Helper class for instantiating SortedDictionary<>s without specifying a comparer.
	/// </summary>
	public static class SortedDictionary {
		
		/// <summary>
		/// Creates a new sorted dictionary with the default comparer.
		/// </summary>
		public static SortedDictionary<K, V> Create<K, V>() {
			return new SortedDictionary<K, V>(Smooth.Collections.Comparer<K>.Default);
		}
		
		/// <summary>
		/// Creates a new sorted dictionary with the default comparer and elements copied from the specified dictionary.
		/// </summary>
		public static SortedDictionary<K, V> Create<K, V>(IDictionary<K, V> dictionary) {
			return new SortedDictionary<K, V>(dictionary, Smooth.Collections.Comparer<K>.Default);
		}

	}
	
	#endregion
	
	#region SortedList

#if ! UNITY_WP8

	/// <summary>
	/// Helper class for instantiating SortedList<>s without specifying a comparer.
	/// </summary>
	public static class SortedList {

		/// <summary>
		/// Creates a new sorted list with the default comparer.
		/// </summary>
		public static SortedList<K, V> Create<K, V>() {
			return new SortedList<K, V>(Smooth.Collections.Comparer<K>.Default);
		}
		
		/// <summary>
		/// Creates a new sorted list with the default comparer and the specified initial capacity.
		/// </summary>
		public static SortedList<K, V> Create<K, V>(int capacity) {
			return new SortedList<K, V>(capacity, Smooth.Collections.Comparer<K>.Default);
		}
		
		/// <summary>
		/// Creates a new sorted list with the default comparer and elements copied from the specified dictionary.
		/// </summary>
		public static SortedList<K, V> Create<K, V>(IDictionary<K, V> dictionary) {
			return new SortedList<K, V>(dictionary, Smooth.Collections.Comparer<K>.Default);
		}
	}

#endif
	
	#endregion

}

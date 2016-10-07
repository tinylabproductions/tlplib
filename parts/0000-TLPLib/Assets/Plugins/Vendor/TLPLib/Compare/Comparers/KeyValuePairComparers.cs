using System;
using System.Collections.Generic;
using Smooth.Collections;

namespace Smooth.Compare.Comparers {
	/// <summary>
	/// Allocation free sort order comparer for KeyValuePair<K,V>s.
	/// </summary>
	public class KeyValuePairComparer<K, V> : Smooth.Collections.Comparer<KeyValuePair<K, V>> {
		public override int Compare(KeyValuePair<K, V> l, KeyValuePair<K, V> r) {
			var c = Smooth.Collections.Comparer<K>.Default.Compare(l.Key, r.Key);
			return c == 0 ? Smooth.Collections.Comparer<V>.Default.Compare(l.Value, r.Value) : c;
		}
	}

	/// <summary>
	/// Allocation free equality comparer for KeyValuePair<K,V>s.
	/// </summary>
	public class KeyValuePairEqComparer<K, V> : Smooth.Collections.EqComparer<KeyValuePair<K, V>> {
		public override bool Equals(KeyValuePair<K, V> l, KeyValuePair<K, V> r) {
			return Smooth.Collections.EqComparer<K>.Default.Equals(l.Key, r.Key) &&
				Smooth.Collections.EqComparer<V>.Default.Equals(l.Value, r.Value);
		}
		
		public override int GetHashCode(KeyValuePair<K, V> kvp) {
			unchecked {
				int hash = 17;
				hash = 29 * hash + Smooth.Collections.EqComparer<K>.Default.GetHashCode(kvp.Key);
				hash = 29 * hash + Smooth.Collections.EqComparer<V>.Default.GetHashCode(kvp.Value);
				return hash;
			}
		}
	}
}

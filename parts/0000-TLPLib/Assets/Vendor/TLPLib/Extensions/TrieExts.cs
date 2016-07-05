using Sasa.Collections;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TrieExts {
    // ReSharper disable once UseMethodAny.2
    public static bool isEmpty<K, V>(this Trie<K, V> trie) => trie.Count() == 0;

    public static bool nonEmpty<K, V>(this Trie<K, V> trie) => trie.Count() != 0;
  }
}
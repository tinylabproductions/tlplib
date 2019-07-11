using System.Collections.Immutable;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class ImmutableDictionaryBuilderExts {
    public static ImmutableDictionary<K, V>.Builder Add2<K, V>(
      this ImmutableDictionary<K, V>.Builder builder, K k, V v
    ) {
      builder.Add(k, v);
      return builder;
    }
  }
}
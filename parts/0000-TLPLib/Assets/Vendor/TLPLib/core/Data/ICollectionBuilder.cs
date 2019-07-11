using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public delegate ICollectionBuilder<A, C> CollectionBuilderKnownSizeFactory<A, C>(int count);
  [PublicAPI] public static class CollectionBuilderKnownSizeFactory<A> {
    public static readonly CollectionBuilderKnownSizeFactory<A, A[]> array =
      count => new ArrayBuilder<A>(new A[count]);
    
    public static readonly CollectionBuilderKnownSizeFactory<A, ImmutableArray<A>> immutableArray =
      count => new ImmutableArrayBuilder<A>(ImmutableArray.CreateBuilder<A>(count));
    
    public static readonly CollectionBuilderKnownSizeFactory<A, ImmutableList<A>> immutableList =
      count => new ImmutableListBuilder<A>(ImmutableList.CreateBuilder<A>());
    
    public static readonly CollectionBuilderKnownSizeFactory<A, ImmutableHashSet<A>> immutableHashSet =
      count => new ImmutableHashSetBuilder<A>(ImmutableHashSet.CreateBuilder<A>());
  }

  [PublicAPI] public static class CollectionBuilderKnownSizeFactoryKV<Key, Value> {
    public static readonly CollectionBuilderKnownSizeFactory<
      KeyValuePair<Key, Value>, ImmutableDictionary<Key, Value>
    > immutableDictionary =
      count => new ImmutableDictionaryBuilder<Key, Value>(ImmutableDictionary.CreateBuilder<Key, Value>());
    
    public static readonly CollectionBuilderKnownSizeFactory<KeyValuePair<Key, Value>, Dictionary<Key, Value>> 
      dictionary = count => new DictionaryBuilder<Key, Value>(new Dictionary<Key, Value>(count));
  }

  // Unity runtime does not like variance and crashes.
  // ReSharper disable TypeParameterCanBeVariant
  public interface ICollectionBuilder<A, C> {
    [PublicAPI] void add(A a);
    [PublicAPI] C build();
    /// <summary>
    /// When you do not intend to use the builder anymore after building.
    /// 
    /// Allows optimizations in some cases.
    /// </summary>
    [PublicAPI] C buildAndDispose();
  }
  // ReSharper restore TypeParameterCanBeVariant

  class ArrayBuilder<A> : ICollectionBuilder<A, A[]> {
    readonly A[] builder;

    int currentIdx;
    
    public ArrayBuilder(A[] builder) { this.builder = builder; }

    public void add(A a) {
      builder[currentIdx] = a;
      currentIdx++;
    }

    public A[] build() {
      var ret = new A[builder.Length];
      builder.CopyTo(ret, 0);
      return ret;
    }
    
    public A[] buildAndDispose() => builder;
  }

  class ImmutableArrayBuilder<A> : ICollectionBuilder<A, ImmutableArray<A>> {
    readonly ImmutableArray<A>.Builder builder;
    
    public ImmutableArrayBuilder(ImmutableArray<A>.Builder builder) { this.builder = builder; }

    public void add(A a) => builder.Add(a);
    public ImmutableArray<A> build() => builder.ToImmutable();
    public ImmutableArray<A> buildAndDispose() => builder.MoveToImmutable();
  }

  class ImmutableListBuilder<A> : ICollectionBuilder<A, ImmutableList<A>> {
    readonly ImmutableList<A>.Builder builder;
    
    public ImmutableListBuilder(ImmutableList<A>.Builder builder) { this.builder = builder; }

    public void add(A a) => builder.Add(a);
    public ImmutableList<A> build() => builder.ToImmutableList();
    public ImmutableList<A> buildAndDispose() => build();
  }

  class ImmutableHashSetBuilder<A> : ICollectionBuilder<A, ImmutableHashSet<A>> {
    readonly ImmutableHashSet<A>.Builder builder;
    
    public ImmutableHashSetBuilder(ImmutableHashSet<A>.Builder builder) { this.builder = builder; }

    public void add(A a) => builder.Add(a);
    public ImmutableHashSet<A> build() => builder.ToImmutableHashSet();
    public ImmutableHashSet<A> buildAndDispose() => build();
  }

  class ImmutableDictionaryBuilder<K, V> : ICollectionBuilder<KeyValuePair<K, V>, ImmutableDictionary<K, V>> {
    readonly ImmutableDictionary<K, V>.Builder builder;
    
    public ImmutableDictionaryBuilder(ImmutableDictionary<K, V>.Builder builder) { this.builder = builder; }

    public void add(KeyValuePair<K, V> a) => builder.Add(a);
    public ImmutableDictionary<K, V> build() => builder.ToImmutable();
    public ImmutableDictionary<K, V> buildAndDispose() => build();
  }

  class DictionaryBuilder<K, V> : ICollectionBuilder<KeyValuePair<K, V>, Dictionary<K, V>> {
    readonly Dictionary<K, V> builder;
    
    public DictionaryBuilder(Dictionary<K, V> builder) { this.builder = builder; }

    public void add(KeyValuePair<K, V> a) => builder.Add(a.Key, a.Value);

    public Dictionary<K, V> build() => new Dictionary<K, V>(builder);
    public Dictionary<K, V> buildAndDispose() => builder;
  }
}
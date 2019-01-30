using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Pools {
  /// <example>
  /// <![CDATA[
  ///   readonly DictionaryGameObjectPool<ObjectHitParticlesPrefab, ObjectHitParticles> objectHitParticlesPools =
  ///     DictionaryGameObjectPool.a(
  ///       (ObjectHitParticlesPrefab prefab) => GameObjectPool.a(GameObjectPool.Init.noReparenting(
  ///         prefab.ToString(),
  ///         () => prefab.prefab.clone() 
  ///       )) 
  ///     );
  /// ]]>
  /// </example>
  /// 
  public static class DictionaryGameObjectPool {
    [PublicAPI]
    public static DictionaryGameObjectPool<K, V> a<K, V>(
      Fn<K, GameObjectPool<V>> createPool
    ) => new DictionaryGameObjectPool<K, V>(createPool);
  }
  
  public class DictionaryGameObjectPool<K, V> {
    readonly Dictionary<K, GameObjectPool<V>> dictionary = new Dictionary<K, GameObjectPool<V>>();
    readonly Fn<K, GameObjectPool<V>> createPool;

    public DictionaryGameObjectPool(Fn<K, GameObjectPool<V>> createPool) {
      this.createPool = createPool;
    }

    public GameObjectPool<V> get(K key) {
      if (!dictionary.TryGetValue(key, out var pool)) {
        pool = createPool(key);
        dictionary.Add(key, pool);
      }

      return pool;
    }
  }
}
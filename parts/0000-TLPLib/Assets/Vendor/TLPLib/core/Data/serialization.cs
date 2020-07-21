using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.serialization;
using pzd.lib.utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [PublicAPI] public static class SerializedRWU {
    [PublicAPI] public static readonly ISerializedRW<Vector2> vector2 =
      SerializedRW.flt.and(SerializedRW.flt, (x, y) => new Vector2(x, y), _ => _.x, _ => _.y);

    [PublicAPI] public static readonly ISerializedRW<Vector3> vector3 =
      vector2.and(SerializedRW.flt, (v2, z) => new Vector3(v2.x, v2.y, z), _ => _, _ => _.z);

    [PublicAPI] public static readonly ISerializedRW<Url> url = 
      SerializedRW.str.map<string, Url>(_ => new Url(_), _ => _.url);

    [PublicAPI] public static readonly ISerializedRW<TextureFormat> textureFormat =
      SerializedRW.integer.map(
        i => 
          EnumUtils.GetValues<TextureFormat>().find(_ => (int) _ == i)
          .toRight($"Can't find texture format by {i}"),
        tf => (int) tf
      );

    [PublicAPI] public static readonly ISerializedRW<Color32> color32 =
      BytePair.rw.and(BytePair.rw, 
        (bp1, bp2) => {
          var (r, g) = bp1;
          var (b, a) = bp2;
          return new Color32(r, g, b, a);
        },
        c => new BytePair(c.r, c.g),
        c => new BytePair(c.b, c.a)
      );
    
    // RWs for library or user defined types go as static fields of those types. 

#if UNITY_EDITOR
    [PublicAPI] public static ISerializedRW<A> unityObjectSerializedRW<A>() where A : Object =>
      PathStr.serializedRW.map<PathStr, A>(
        path => {
          try {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<A>(path);
          }
          catch (Exception e) {
            return $"loading {typeof(A).FullName} from '{path}' threw {e}";
          }
        },
        module => module.editorAssetPath()
      );
#endif
    
    public static ISerializedRW<Tpl<A, B>> tpl<A, B>(
      this ISerializedRW<A> aRW, ISerializedRW<B> bRW
    ) => aRW.and(bRW, F.t, t => t._1, t => t._2);

    public static ISerializedRW<Tpl<A, B, C>> tpl<A, B, C>(
      this ISerializedRW<A> aRW, ISerializedRW<B> bRW, ISerializedRW<C> cRW
    ) => aRW.and(bRW, cRW, F.t, t => t._1, t => t._2, t => t._3);
  }
}
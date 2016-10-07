using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TransformExts {
    public static void positionBetween(
      this Transform t, Vector3 start, Vector3 end, float width
    ) {
      var offset = end - start;
      var scale = new Vector3(width, offset.magnitude / 2.0f, width);
      var position = start + (offset / 2.0f);

      t.position = position;
      t.up = offset;
      t.localScale = scale;
    }

    public static void setPosition(
      this Transform t,
#if ENABLE_IL2CPP
      Option<float> x=null, 
      Option<float> y=null, 
      Option<float> z=null
#else
      Option<float> x=new Option<float>(), 
      Option<float> y=new Option<float>(), 
      Option<float> z=new Option<float>()
#endif
) {
      t.position = t.position.with3(x, y, z);
    }

    public static void setScale(
      this Transform t,
#if ENABLE_IL2CPP
      Option<float> x = null,
      Option<float> y = null,
      Option<float> z = null
#else
      Option<float> x=new Option<float>(), 
      Option<float> y=new Option<float>(), 
      Option<float> z=new Option<float>()
#endif
    ) {
      t.localScale = t.localScale.with3(x, y, z);
    }

    public static IEnumerable<Transform> children(this Transform parent) {
      for (var idx = 0; idx < parent.childCount; idx++)
        yield return parent.GetChild(idx);
    }

    public static A addChild<A>(this Transform self, A child) 
    where A : Component {
      child.transform.parent = self;
      return child;
    }

    public static GameObject addChild(this Transform self, GameObject child) {
      self.addChild(child.transform);
      return child;
    }

    public static void doRecursively(this Transform t, Act<Transform> act) {
      act(t);
      for (var idx = 0; idx < t.childCount; idx++)
        t.GetChild(idx).doRecursively(act);
    }

    public static void resetLocalScalePosition(this Transform t) {
      t.localScale = Vector3.one;
      t.localPosition = Vector3.zero;
    }
  }
}

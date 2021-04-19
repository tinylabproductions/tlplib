using System;
using System.Collections.Generic;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class TransformExts {
    public static string debugPath(this Transform transform) {
      var t = transform;
      var s = "";
      while (t) {
        s = $"{t.name}/{s}";
        t = t.parent;
      }

      return s;
    }
    
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
      Option<float> x = default(Option<float>),
      Option<float> y = default(Option<float>),
      Option<float> z = default(Option<float>)
    ) {
      t.position = t.position.with3(x, y, z);
    }

    public static void setScale(
      this Transform t,
      Option<float> x = default(Option<float>),
      Option<float> y = default(Option<float>),
      Option<float> z = default(Option<float>)
    ) {
      t.localScale = t.localScale.with3(x, y, z);
    }

    public static void setLocalScaleX(this Transform t, float x) => t.localScale = t.localScale.withX(x);
    public static void setLocalScaleY(this Transform t, float y) => t.localScale = t.localScale.withY(y);
    public static void setLocalScaleZ(this Transform t, float z) => t.localScale = t.localScale.withZ(z);
    
    public static void setLocalPositionX(this Transform t, float x) => t.localPosition = t.localPosition.withX(x);
    public static void setLocalPositionY(this Transform t, float y) => t.localPosition = t.localPosition.withY(y);
    public static void setLocalPositionZ(this Transform t, float z) => t.localPosition = t.localPosition.withZ(z);
    
    public static void setPositionX(this Transform t, float x) => t.position = t.position.withX(x);
    public static void setPositionY(this Transform t, float y) => t.position = t.position.withY(y);
    public static void setPositionZ(this Transform t, float z) => t.position = t.position.withZ(z);

    public static IEnumerable<Transform> children(this Transform parent) {
      for (var idx = 0; idx < parent.childCount; idx++)
        yield return parent.GetChild(idx);
    }

    public static IEnumerable<Transform> andAllChildrenRecursive(this Transform transform) {
      yield return transform;
      for (var i = 0; i < transform.childCount; i++) {
        foreach (var child in transform.GetChild(i).andAllChildrenRecursive()) {
          yield return child;
        }
      }
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

    public static void doRecursively(this Transform t, Action<Transform> act) {
      act(t);
      for (var idx = 0; idx < t.childCount; idx++)
        t.GetChild(idx).doRecursively(act);
    }

    public static void resetLocalScalePosition(this Transform t) {
      t.localScale = Vector3.one;
      t.localPosition = Vector3.zero;
    }

    public static void resetLocalAll(this Transform t) {
      t.localScale = Vector3.one;
      t.localPosition = Vector3.zero;
      t.localRotation = Quaternion.identity;
    }
  }
}

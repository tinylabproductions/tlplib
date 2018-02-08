using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class VectorExts {
    public static Vector2 withX(this Vector2 v, float x) => new Vector2(x, v.y);
    public static Vector2 withY(this Vector2 v, float y) => new Vector2(v.x, y);

    public static Vector2 addX(this Vector2 v, float x) => new Vector2(v.x + x, v.y);
    public static Vector2 addY(this Vector2 v, float y) => new Vector2(v.x, v.y + y);

    public static Vector2 multiply(this Vector2 v, Vector2 v2) => new Vector2(v.x * v2.x, v.y * v2.y);
    public static Vector2 divide(this Vector2 v, Vector2 v2) => new Vector2(v.x / v2.x, v.y / v2.y);

    public static Vector3 withX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
    public static Vector3 withY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    public static Vector3 withZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

    public static Vector3 addX(this Vector3 v, float x) => new Vector3(v.x + x, v.y, v.z);
    public static Vector3 addY(this Vector3 v, float y) => new Vector3(v.x, v.y + y, v.z);
    public static Vector3 addZ(this Vector3 v, float z) => new Vector3(v.x, v.y, v.z + z);

    public static Vector3 multiply(this Vector3 v, Vector3 v2) => new Vector3(v.x * v2.x, v.y * v2.y, v.z * v2.z);
    public static Vector3 divide(this Vector3 v, Vector3 v2) => new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);

    static string logFormat(float f) => $"{f,10:0.000}";
    public static string logFormat(this Vector2 v) => $"({logFormat(v.x)},{logFormat(v.y)})";
    public static string logFormat(this Vector3 v) => $"({logFormat(v.x)},{logFormat(v.y)},{logFormat(v.z)})";

    public static Vector2 with2(
      this Vector2 v,
      Option<float> x = default(Option<float>), 
      Option<float> y = default(Option<float>)
    ) {
      Option.ensureValue(ref x);
      Option.ensureValue(ref y);
      return new Vector3(x.getOrElse(v.x), y.getOrElse(v.y));
    }

    public static Vector3 with3(
      this Vector3 v,
      Option<float> x = default(Option<float>), 
      Option<float> y = default(Option<float>), 
      Option<float> z = default(Option<float>)
    ) {
      Option.ensureValue(ref x);
      Option.ensureValue(ref y);
      Option.ensureValue(ref z);
      return new Vector3(x.getOrElse(v.x), y.getOrElse(v.y), z.getOrElse(v.z));
    }

    public static Vector2 rotate90(this Vector2 v) => new Vector2(-v.y, v.x);
    public static Vector2 rotate180(this Vector2 v) => new Vector2(-v.x, -v.y);
    public static Vector2 rotate270(this Vector2 v) => new Vector2(v.y, -v.x);

    public static float cross(this Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    public static Vector2 rotate(this Vector2 v, float degrees) {
      var radians = degrees * Mathf.Deg2Rad;
      var sin = Mathf.Sin(radians);
      var cos = Mathf.Cos(radians);
         
      var tx = v.x;
      var ty = v.y;
 
      return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }
    
    public static readonly ISerializedRW<Vector2> rw2 =
      SerializedRW.flt.and(SerializedRW.flt).map(
        tpl => new Vector2(tpl._1, tpl._2).some(),
        p => F.t(p.x, p.y)
      );
    
    public static readonly ISerializedRW<Vector3> rw3 =
      SerializedRW.flt.and(SerializedRW.flt).and(SerializedRW.flt).map(
        tpl => new Vector3(tpl._1._1, tpl._1._2, tpl._2).some(),
        p => F.t(F.t(p.x, p.y), p.z)
      );
  }
}

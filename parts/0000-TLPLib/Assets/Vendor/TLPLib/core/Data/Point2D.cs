using System;
using System.Collections.Generic;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.serialization;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false), Serializable]
  public partial struct Point2D {
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor] int _x, _y;
    // ReSharper restore FieldCanBeMadeReadOnly.Local

    [PublicAPI] public Point2D withX(int x) => new Point2D(x, _y);
    [PublicAPI] public Point2D withY(int y) => new Point2D(_x, y);

    [PublicAPI] public Point2D up => new Point2D(_x, _y+1);
    [PublicAPI] public Point2D down => new Point2D(_x, _y-1);
    [PublicAPI] public Point2D left => new Point2D(_x-1, _y);
    [PublicAPI] public Point2D right => new Point2D(_x+1, _y);

    public static implicit operator Vector2(Point2D p) => new Vector2(p._x, p._y);
    public static implicit operator Vector3(Point2D p) => new Vector3(p._x, p._y);
    public static Point2D operator +(Point2D a, Point2D b) => new Point2D(a._x + b._x, a._y + b._y);
    public static Point2D operator -(Point2D a, Point2D b) => new Point2D(a._x - b._x, a._y - b._y);
    public static Point2D operator -(Point2D a) => new Point2D(-a._x, -a._y);

    [PublicAPI]
    public IEnumerable<Point2D> around(uint radius) {
      for (var i = -radius; i <= radius; i++)
      for (var j = -radius; j <= radius; j++) {
        yield return this + new Point2D((int) i, (int) j);
      }
    }

    public override string ToString() => $"({_x},{_y})";

    [PublicAPI]
    public static readonly ISerializedRW<Point2D> rw =
      SerializedRW.integer.and(SerializedRW.integer, (x, y) => new Point2D(x, y), _ => _._x, _ => _._y);
  }
}

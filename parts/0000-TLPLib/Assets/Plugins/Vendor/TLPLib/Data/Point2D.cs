using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false)]
  public partial struct Point2D {
    public readonly int x, y;

    public Point2D copy(int? x = null, int? y = null) => new Point2D(x ?? this.x, y ?? this.y);

    public Point2D up => new Point2D(x, y+1);
    public Point2D down => new Point2D(x, y-1);
    public Point2D left => new Point2D(x-1, y);
    public Point2D right => new Point2D(x+1, y);

    public static implicit operator Vector2(Point2D p) => new Vector2(p.x, p.y);
    public static implicit operator Vector3(Point2D p) => new Vector3(p.x, p.y);

    public override string ToString() => $"({x},{y})";

    public static ISerializedRW<Point2D> rw =
      SerializedRW.integer.and(SerializedRW.integer).map(
        tpl => new Point2D(tpl._1, tpl._2).some(),
        p => F.t(p.x, p.y)
      );
  }
}

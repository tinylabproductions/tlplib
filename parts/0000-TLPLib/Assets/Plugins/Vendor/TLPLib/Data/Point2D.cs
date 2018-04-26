using System.Collections.Generic;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false)]
  public partial struct Point2D {
    public readonly int x, y;

    [PublicAPI] public Point2D withX(int x) => new Point2D(x, y);
    [PublicAPI] public Point2D withY(int y) => new Point2D(x, y);

    [PublicAPI] public Point2D up => new Point2D(x, y+1);
    [PublicAPI] public Point2D down => new Point2D(x, y-1);
    [PublicAPI] public Point2D left => new Point2D(x-1, y);
    [PublicAPI] public Point2D right => new Point2D(x+1, y);
    
    public static implicit operator Vector2(Point2D p) => new Vector2(p.x, p.y);
    public static implicit operator Vector3(Point2D p) => new Vector3(p.x, p.y);
    public static Point2D operator +(Point2D a, Point2D b) => new Point2D(a.x + b.x, a.y + b.y);
    public static Point2D operator -(Point2D a, Point2D b) => new Point2D(a.x - b.x, a.y - b.y);
    public static Point2D operator -(Point2D a) => new Point2D(-a.x, -a.y);

    [PublicAPI]
    public IEnumerable<Point2D> around(uint radius) {
      for (var i = -radius; i <= radius; i++)
      for (var j = -radius; j <= radius; j++) {
        yield return this + new Point2D((int) i, (int) j);
      }
    }

    public override string ToString() => $"({x},{y})";

    [PublicAPI]
    public static readonly ISerializedRW<Point2D> rw =
      SerializedRW.integer.and(SerializedRW.integer, (x, y) => new Point2D(x, y), _ => _.x, _ => _.y);
  }
}

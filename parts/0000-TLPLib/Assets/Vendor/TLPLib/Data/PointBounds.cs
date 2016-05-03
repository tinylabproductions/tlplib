using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Data {
  struct PointBounds : IEnumerable<Point2D> {
    public readonly Point2D center, extents;

    public PointBounds(Point2D center, Point2D extents) {
      this.center = center;
      this.extents = extents;
    }

    public IEnumerator<Point2D> GetEnumerator() {
      for (var xDiff = -extents.x; xDiff < extents.x; xDiff++)
        for (var yDiff = -extents.y; yDiff < extents.y; yDiff++)
          yield return new Point2D(center.x + xDiff, center.y + yDiff);
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}

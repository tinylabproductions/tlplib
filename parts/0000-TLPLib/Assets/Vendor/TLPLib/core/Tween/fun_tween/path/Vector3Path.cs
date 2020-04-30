using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3Path {
    public enum InterpolationMethod : byte { Linear, CatmullRom, Cubic, Hermite }

    /// <summary>
    /// If a path uses non-linear <see cref="InterpolationMethod"/>, the path is curved, as you can
    /// see in this very inaccurate ASCII art depiction. 
    /// 
    /// point 0     point 2
    /// ()          ()
    ///  |          |
    ///  |          |
    ///   \        /
    ///    \      /     
    ///     \_()_/  
    ///       point 1
    /// 
    /// You can find a better image in https://en.wikipedia.org/wiki/Spline_(mathematics).
    ///
    /// This is somewhat a problem, because to evaluate percentage on a path (for example where is 16%
    /// of the whole path?) we need to know the total distance of the path.
    ///
    /// In linear paths it is quite simple, as we can just take distances between points and add them
    /// up, but in curved paths we need to subdivide the path into a lot of smaller segments, where each
    /// segment is effectively approximated by a straight line. Then we can calculate lengths of all those
    /// lines and end up with a total length of the path.
    ///
    /// We create constant speed table <see cref="ConstantSpeedTable"/> where each entry percentage is increased by fixed size.
    /// Each entry represents a point on a curve and holds percentage and approximated length to this point.
    /// We use this information to normalize raw percentage <see cref="Vector3Path.recalculatePercentage"/>, in order to
    /// achieve constant speed moving along the spline.
    /// </summary>
    [Record]
    partial struct ConstantSpeedTable {
      [Record]
      public partial struct Entry {
        /// <summary>
        /// Percentage as [0, 1] which passed to <see cref="Vector3Path.calculate"/> would
        /// give a point on the path. 
        /// </summary>
        public readonly float percentageOfPath;

        /// <summary>
        /// Distance from the start of the path which is calculated by adding up
        /// lengths of subdivided path segments from percentage 0 up until <see cref="percentageOfPath"/>. 
        /// </summary>
        public readonly float summedDistanceFromPathStart;
      }

      public readonly ImmutableArray<Entry> entries;

      public ConstantSpeedTable(int subdivisions, Func<float, Vector3> getPointOnPath) {
        var lengthAccumulator = 0f;
        var increment = 1f / subdivisions;
        var builder = ImmutableArray.CreateBuilder<Entry>(subdivisions);
        var prevPoint = getPointOnPath(0);
        for (var idx = 1; idx < subdivisions + 1; idx++) {
          var percentage = increment * idx;
          var currentPoint = getPointOnPath(percentage);
          lengthAccumulator += Vector3.Distance(currentPoint, prevPoint);
          prevPoint = currentPoint;
          builder.Add(new Entry(percentageOfPath: percentage, summedDistanceFromPathStart: lengthAccumulator));
        }
        
        entries = builder.MoveToImmutable();
      }
    }

    [Record]
    public partial struct Point {
      public readonly Vector3 point;
      
      /// <summary>
      /// How much of the path in % [0, 1] have we traveled from the path start
      /// if we are at this point currently.
      /// </summary>
      public readonly float percentageOfPathTraveled;

      /// <summary>
      /// Distance between start of the path and point at <see cref="percentageOfPathTraveled"/>.
      /// </summary>
      public readonly float realDistanceToThisPoint;
    }

    public readonly InterpolationMethod method;
    public readonly bool closed;
    public readonly ImmutableArray<Point> points;
    public readonly Option<Transform> relativeTo;
    public readonly int resolution;
    readonly ConstantSpeedTable constantSpeedTable;
    readonly float realLength;

    public Vector3Path(
      InterpolationMethod method, bool closed, ImmutableArray<Vector3> positions, Option<Transform> relativeTo,
      int pathResolution
    ) {
      this.method = method;
      this.closed = closed;
      this.relativeTo = relativeTo;
      resolution = pathResolution;
      points = segmentLengthRatios();

      //Distance to the last node is the whole path distance
      realLength = points[points.Length - 1].realDistanceToThisPoint;
      constantSpeedTable = new ConstantSpeedTable(resolution, calculate);
      
      // Returns list of Points with Vector3 coordinates,
      // length and segment length ratios added to prev element ratio and
      // distance from the start to this point
      ImmutableArray<Point> segmentLengthRatios() {
        switch (method) {
          case InterpolationMethod.Linear: {
            var builder = ImmutableArray.CreateBuilder<Point>(positions.Length);
            var length = positions.Aggregate(0f, (node, current, idx) =>
              idx == 0
                ? current
                : current + Vector3.Distance(positions[idx - 1], node)
            );
            builder.Add(new Point(positions[0], 0f, 0f));
            for (var idx = 1; idx < positions.Length; idx++) {
              var distanceBetweenPositions = Vector3.Distance(positions[idx - 1], positions[idx]);
              var previousPoint = builder[idx - 1];
              builder.Add(new Point(
                positions[idx],
                distanceBetweenPositions / length + previousPoint.percentageOfPathTraveled,
                distanceBetweenPositions + previousPoint.realDistanceToThisPoint
              ));
            }

            return builder.MoveToImmutable();
          }
          case InterpolationMethod.Hermite: {
            return getSegmentRatios(
              positions,
              (segmentIndex, percentageOfPath) => InterpolationUtils.hermiteGetPt(
                idx => positions[idx], positions.Length, segmentIndex, percentageOfPath, closed
              ));
          }
          case InterpolationMethod.Cubic: {
            return getSegmentRatios(
              positions,
              (segmentIndex, percentageOfPath) => InterpolationUtils.cubicGetPt(
                idx => positions[idx], positions.Length, segmentIndex, percentageOfPath, closed
              ));
          }
          case InterpolationMethod.CatmullRom: {
            return getSegmentRatios(
              positions,
              (segmentIndex, percentageOfPath) => InterpolationUtils.catmullRomGetPt(
                idx => positions[idx], positions.Length, segmentIndex, percentageOfPath, closed
            ));
          }
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    public delegate float GetSegmentLength(int index);

    static ImmutableArray<Point> getSegmentRatios(GetSegmentLength getSegmentLength, ImmutableArray<Vector3> nodes) {
      var builder = ImmutableArray.CreateBuilder<Point>(nodes.Length);
      
      var length = 0f;
      var lengthsCount = nodes.Length - 1;
      var lengths = new float[lengthsCount];
      for (var idx = 0; idx < lengthsCount; idx++) {
        var segLength = getSegmentLength(idx);
        length += segLength;
        lengths[idx] = segLength;
      }
      
      // Creating points
      builder.Add(new Point(nodes[0], 0f, 0f));
      for (var idx = 1; idx < nodes.Length; idx++) {
        var previousNodeLength = lengths[idx - 1];
        var previousPoint = builder[idx - 1];
        builder.Add(new Point(
          point: nodes[idx],
          percentageOfPathTraveled: previousNodeLength / length + previousPoint.percentageOfPathTraveled,
          realDistanceToThisPoint: previousNodeLength + previousPoint.realDistanceToThisPoint
        ));
      }    

      return builder.MoveToImmutable();
    }

    delegate Vector3 GetSegmentPoint(int segmentIndex, float percentageOfPath);
    delegate Vector3 GetPoint(float percentageInPath);
    
    ImmutableArray<Point> getSegmentRatios(ImmutableArray<Vector3> positions, GetSegmentPoint getSegmentPoint) =>
      getSegmentRatios(
        segmentIndex => getApproxSegmentLength(
          resolution,
          percentageOfPath => getSegmentPoint(segmentIndex, percentageOfPath)
        ),
        positions
      );
    
    static float getApproxSegmentLength(int resolution, GetPoint getPt) {
      var oldPoint = getPt(0f);
      var splineLength = 0f;
      for (var i = 1; i <= resolution; i++) {
        var percentage = (float) i / resolution;
        var newPoint = getPt(percentage);
        var dist = Vector3.Distance(oldPoint, newPoint);
        splineLength += dist;
        oldPoint = newPoint;
      }
      
      return splineLength;
    }

    float recalculatePercentage(float percentage) {
      if (method == InterpolationMethod.Linear) return percentage;
      if (percentage > 0 && percentage < 1) {
        var tLen = realLength * percentage;
        var count = constantSpeedTable.entries.Length;
        //Binary search to find lower bound
        var L = 0;
        var R = count - 2;
        while (L < R) {
          var m = (L + R) / 2;
          var currDist = constantSpeedTable.entries[m].summedDistanceFromPathStart;
          if (currDist < tLen) {
            L = m + 1;
          }
          else if (currDist > tLen) {
            R = m - 1;
          }
          else break;
        }

        float
          t0  = constantSpeedTable.entries[L].percentageOfPath, //Lower percentage table bound
          t1  = constantSpeedTable.entries[L + 1].percentageOfPath, //Upper percentage table bound
          le0 = constantSpeedTable.entries[L].summedDistanceFromPathStart, //Lower length table bound
          le1 = constantSpeedTable.entries[L + 1].summedDistanceFromPathStart; //Higher length table bound

        percentage = t0 + (tLen - le0) / (le1 - le0) * (t1 - t0);
      }

      percentage = Mathf.Clamp(percentage, 0, 1);

      return percentage;
    }

    /// <summary>
    /// Evaluate a point on a path given a percentage from [0, 1]
    /// </summary>
    /// <param name="percentage"></param>
    /// <param name="constantSpeed"></param>
    /// <returns></returns>
    public Vector3 evaluate(float percentage, bool constantSpeed) {
      // Recalculating percentage to achieve constant movement speed
      if (constantSpeed) percentage = recalculatePercentage(percentage);
      
      return relativeTo.valueOut(out var transform)
        ? transform.TransformPoint(calculate(percentage))
        : calculate(percentage);
    }

    Vector3 calculate(float percentage) {
      var low = 0;
      var high = points.Length - 2;
      while (low < high) {
        var mid = (low + high) / 2;
        if (points[mid + 1].percentageOfPathTraveled < percentage) {
          low = mid + 1;
        }
        else {
          high = mid;
        }
      }

      var segmentPercentage =
        (percentage - points[low].percentageOfPathTraveled)
        / (points[low + 1].percentageOfPathTraveled - points[low].percentageOfPathTraveled);

      switch (method) {
        case InterpolationMethod.Linear:
          return Vector3.Lerp(
            points[low].point, points[low + 1].point,
            segmentPercentage
          );
        case InterpolationMethod.Cubic:
          return InterpolationUtils.cubicGetPt(
            idx => points[idx].point, points.Length, low,
            segmentPercentage,
            closed
          );
        case InterpolationMethod.Hermite:
          return InterpolationUtils.hermiteGetPt(
            idx => points[idx].point, points.Length, low,
            segmentPercentage,
            closed
          );
        case InterpolationMethod.CatmullRom:
          return InterpolationUtils.catmullRomGetPt(
            idx => points[idx].point, points.Length, low,
            segmentPercentage,
            closed
          );
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
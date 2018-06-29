using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public partial class Vector3Path {
    public enum InterpolationMethod : byte {
      Linear,
      CatmullRom,
      Cubic,
      Hermite
    }
    
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
    /// TODO
    ///  
    /// 
    /// While evaluating fixed sized precentage points on a spline,
    /// we get different distances between them. If we want to achieve constant speed, we have to
    /// normalise the percentage we want to evaluate.
    ///
    /// Constant speed table is used to calculate normalised percentage,
    /// to achieve constant speed moving along the spline
    /// </summary>
    [Record] partial struct ConstantSpeedTable {
      [Record] public partial struct Entry {
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
      
      public ConstantSpeedTable(int subdivisions, Fn<float, Vector3> calculate) {
        var lengthAccumulator = 0f;
        var increment = 1f / subdivisions;
        var builder = ImmutableArray.CreateBuilder<Entry>(subdivisions);
        var oldPoint = calculate(0);
        for (var idx = 1; idx < subdivisions + 1; idx++) {
          var percentage = increment * idx;
          var newPoint = calculate(percentage);
          lengthAccumulator += Vector3.Distance(newPoint, oldPoint);
          oldPoint = newPoint;
          builder.Add(new Entry(percentageOfPath: percentage, summedDistanceFromPathStart: lengthAccumulator));
        }

        return new ConstantSpeedTable(builder.MoveToImmutable());
      }
    }

    [Record] public partial struct Point {
      public readonly Vector3 point;
      /// <summary>
      /// How much of the path in % [0, 1] have we traveled from the path start
      /// if we are at this point currently.
      /// </summary>
      public readonly float percentageOfPathTraveled;
    }
    
    public readonly InterpolationMethod method;
    public readonly bool closed;
    public readonly ImmutableArray<Point> points;
    public readonly Option<Transform> relativeTo;
    public readonly int resolution;

    readonly ConstantSpeedTable constantSpeedTable;
    readonly float realLength;

    readonly InterpolationUtils.GetPoint getPoint;
    readonly Option<Fn<float, int, float>> getSegmentLenFn;

    public Vector3Path(
      InterpolationMethod method, bool closed, ImmutableArray<Vector3> points, Option<Transform> relativeTo,
      int pathResolution
    ) {
      getPoint = idx => this.points[idx].point;
      this.method = method;
      this.closed = closed;
      this.relativeTo = relativeTo;
      resolution = pathResolution;
      var t = segmentLengthRatios();
      this.points = t._1;
      realLength = t._2;
      constantSpeedTable = new ConstantSpeedTable(resolution, calculate);

      // Returns list of whole length and segment length ratios added to prev element
      Tpl<ImmutableArray<Point>, float> segmentLengthRatios() {
        switch (method) {
          case InterpolationMethod.Linear: {
            var builder = ImmutableArray.CreateBuilder<Point>(points.Length);
            var length = points.Aggregate(0f, (node, current, idx) =>
              idx == 0
                ? current
                : current + Vector3.Distance(points[idx - 1], node)
            );
            builder.Add(new Point(points[0], 0));
            for (var idx = 1; idx < points.Length; idx++) {
              builder.Add(new Point(
                points[idx],
                Vector3.Distance(points[idx - 1], points[idx]) / length 
                  + builder[idx - 1].percentageOfPathTraveled
              ));
            }

            return F.t(builder.MoveToImmutable(), length);
          }
          case InterpolationMethod.Hermite:
            return getSegmentsRatios(
              index => getApproxSegmentLength(
                resolution, 
                percentageOfPath => InterpolationUtils.hermiteGetPt(
                  getPoint, points.Length, index, percentageOfPath, closed
                )
              )
            );
          case InterpolationMethod.Cubic:
            return getSegmentsRatios(
              index => getApproxSegmentLength(
                resolution,
                percentageOfPath => InterpolationUtils.cubicGetPt(
                  getPoint, points.Length, index, percentageOfPath, closed
                )
              )
            );
          case InterpolationMethod.CatmullRom: {
            return getSegmentsRatios(
              index => getApproxSegmentLength(
                resolution,
                percentageOfPath => InterpolationUtils.catmullRomGetPt(
                  getPoint, points.Length, index, percentageOfPath, closed
                )
              )
            );
          }
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    public delegate float GetSegmentLength(int index);

    public Tpl<ImmutableArray<float>, float> getSegmentsRatios(GetSegmentLength getSegmentLength) {
      var builder = ImmutableArray.CreateBuilder<float>(points.Length);
      var length = 0f;
      var lengths = new List<float>();
      for (var idx = 0; idx < points.Length - 1; idx++) {
        var segLength = getSegmentLength(idx);
        length += segLength;
        lengths.Add(segLength);
      }
      
      builder.Add(0);
      for (var idx = 1; idx < points.Length; idx++) {
        builder.Add(lengths[idx - 1] / length + builder[idx - 1]);
      }

      return F.t(builder.MoveToImmutable(), length);
    }

    public delegate Vector3 GetPoint(float percentageInPath);
    
    float getApproxSegmentLength(int resolution, GetPoint getPt) {
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
      var fractionsTable = constantSpeedTable._1;
      var lengthsTable = constantSpeedTable._2;
      if (method == InterpolationMethod.Linear) return percentage;
      if (percentage > 0 && percentage < 1) {
        var tLen = realLength * percentage;
        float t0 = 0, le0 = 0, t1 = 0, le1 = 0;
        var count = lengthsTable.Length;
        //TODO: Optimize search
        for (var idx = 0; idx < count; ++idx) {
          if (lengthsTable[idx] > tLen) {
            t1 = fractionsTable[idx];
            le1 = lengthsTable[idx];
            if (idx > 0) le0 = lengthsTable[idx - 1];
            break;
          }

          t0 = fractionsTable[idx];
        }

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

      Vector3 returnValue;
      switch (method) {
        case InterpolationMethod.Linear:
          returnValue = Vector3.Lerp(
            points[low].point, points[low + 1].point,
            segmentPercentage
          );
          break;
        case InterpolationMethod.Cubic:
          returnValue = InterpolationUtils.cubicGetPt(
            getPoint, points.Length, low,
            segmentPercentage,
            closed
          );
          break;
        case InterpolationMethod.Hermite:
          returnValue = InterpolationUtils.hermiteGetPt(
            getPoint, points.Length, low,
            segmentPercentage,
            closed
          );
          break;
        case InterpolationMethod.CatmullRom:
          returnValue = InterpolationUtils.catmullRomGetPt(
            getPoint, points.Length, low,
            segmentPercentage,
            closed
          );
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return returnValue;
    }
  }
}
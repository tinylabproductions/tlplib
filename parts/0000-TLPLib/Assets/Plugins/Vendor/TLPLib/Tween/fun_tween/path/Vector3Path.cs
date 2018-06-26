using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using GoogleMobileAds.Api;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.path {
  public class Vector3Path {
    public enum InterpolationMethod : byte {
      Linear,
      CatmullRom,
      Cubic,
      Hermite
    }

    public readonly InterpolationMethod method;
    public readonly bool closed;
    public readonly ImmutableArray<Vector3> points;
    public readonly Option<Transform> relativeTo;
    public readonly int resolution;
    public readonly ImmutableArray<float> segmentLengthRatios;

    //Fraction and length tables
    readonly Tpl<ImmutableArray<float>, ImmutableArray<float>> constantSpeedTables;
    readonly float realLength;

    readonly Fn<int, Vector3> getPoint;
    readonly Option<Fn<float, int, float>> getSegmentLenFn;

    public Vector3Path(
      InterpolationMethod method, bool closed, ImmutableArray<Vector3> points, Option<Transform> relativeTo,
      int pathResolution
    ) {
      getPoint = idx => this.points[idx];
      this.method = method;
      this.closed = closed;
      this.points = points;
      this.relativeTo = relativeTo;
      resolution = pathResolution;
      var t = segmentLengthRatios();
      this.segmentLengthRatios = t._1;
      realLength = t._2;
      constantSpeedTables = setTimeToLengthTables(resolution);


      //Returns list of whole length and segment length ratios added to prev element
      Tpl<ImmutableArray<float>, float> segmentLengthRatios() {
        switch (method) {
          case InterpolationMethod.Linear: {
            var builder = ImmutableArray.CreateBuilder<float>(points.Length);
            var length = points.Aggregate(0f, (node, current, idx) =>
              idx == 0
                ? current
                : current + Vector3.Distance(points[idx - 1], node)
            );
            builder.Add(0);
            for (var idx = 0; idx < points.Length - 1; idx++) {
              builder.Add(Vector3.Distance(points[idx], points[idx + 1]) / length + builder[idx]);
            }

            return F.t(builder.MoveToImmutable(), length);
          }
          case InterpolationMethod.Hermite:
            return getSegmentsRatios((x, y) => getApproxSegmentLength(x, y,
              (_, b, c, d, e) => InterpolationUtils.hermiteGetPt(getPoint, b, c, d, e))
            );
          case InterpolationMethod.Cubic:
            return getSegmentsRatios((x, y) => getApproxSegmentLength(x, y,
              (_, b, c, d, e) => InterpolationUtils.cubicGetPt(getPoint, b, c, d, e))
            );
          case InterpolationMethod.CatmullRom: {
            return getSegmentsRatios((x, y) => getApproxSegmentLength(x, y,
              (_, b, c, d, e) => InterpolationUtils.catmullRomGetPt(getPoint, b, c, d, e))
            );
          }
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

    }

    public Tpl<ImmutableArray<float>, float> getSegmentsRatios(Fn<int, int, float> getSegmentLength) {
      var builder = ImmutableArray.CreateBuilder<float>(points.Length);
      var length = 0f;
      var lengths = new List<float>();
      for (var idx = 0; idx < points.Length - 1; idx++) {
        var segLength = getSegmentLength(resolution, idx);
        length += segLength;
        lengths.Add(segLength);
      }
      
      builder.Add(0);
      for (var idx = 1; idx < points.Length; idx++) {
        builder.Add(lengths[idx - 1] / length + builder[idx - 1]);
      }

      return F.t(builder.MoveToImmutable(), length);
    }

    float getApproxSegmentLength(
      int resolution, int id, Fn<Fn<int, Vector3>, int, int, float, bool, Vector3> getPt) {
      var oldPoint = getPt(getPoint, points.Length, id, 0f, closed);
      var splineLength = 0f;  
      for (var i = 1; i <= resolution; i++) {
        var percentage = (float) i / resolution;
        var newPoint = getPt(getPoint, points.Length, id, percentage, closed);
        var dist = Vector3.Distance(oldPoint, newPoint);
        splineLength += dist;
        oldPoint = newPoint;
      }
      return splineLength;
    }

    Tpl<ImmutableArray<float>, ImmutableArray<float>> 
      setTimeToLengthTables(int subdivisions) {
        var lengthAcc = 0f;
        var incr = 1f / subdivisions;
        var fracBuilder = ImmutableArray.CreateBuilder<float>(subdivisions);
        var lenBuilder = ImmutableArray.CreateBuilder<float>(subdivisions);
        var oldPoint = calculate(0);
        for (var idx = 1; idx < subdivisions + 1; idx++) {
          var perc = incr * idx;
          var newPoint = calculate(perc);
          lengthAcc += Vector3.Distance(newPoint, oldPoint);
          oldPoint = newPoint;
          fracBuilder.Add(perc);
          lenBuilder.Add(lengthAcc);
        }
  
        return F.t(fracBuilder.MoveToImmutable(), lenBuilder.MoveToImmutable());
    }
    
    float recalculatePercentage(float percentage) {
      var fractionsTable = constantSpeedTables._1;
      var lengthsTable = constantSpeedTables._2;
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

    public Vector3 evaluate(float perc, bool constantSpeed) {
      //Recalculating percentage to achieve constant movement speed
      if (constantSpeed) perc = recalculatePercentage(perc); 
      
      return relativeTo.valueOut(out var transform)
        ? transform.TransformPoint(calculate(perc))
        : calculate(perc);
    }

    Vector3 calculate(float percentage) {
        var low = 0;
        var high = segmentLengthRatios.Length - 2;
        while (low < high) {
          var mid = (low + high) / 2;
          if (segmentLengthRatios[mid + 1] < percentage) {
            low = mid + 1;
          }
          else {
            high = mid;
          }
        }

        var segmentPercentage = (percentage - segmentLengthRatios[low]) /
          (segmentLengthRatios[low + 1] - segmentLengthRatios[low]);

        Vector3 returnValue;
        switch (method) {
          case InterpolationMethod.Linear:
            returnValue = Vector3.Lerp(
              points[low], points[low + 1],
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
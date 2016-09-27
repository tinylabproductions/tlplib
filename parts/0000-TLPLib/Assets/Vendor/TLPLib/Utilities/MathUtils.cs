using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class MathUtils {
    public static Option<Vector2> LineIntersectionPoint(
      Vector2 ps1, Vector2 pe1, Vector2 ps2, Vector2 pe2
    ) {
      // Get A,B,C of first line - points : ps1 to pe1
      float A1 = pe1.y - ps1.y;
      float B1 = ps1.x - pe1.x;
      float C1 = A1 * ps1.x + B1 * ps1.y;

      // Get A,B,C of second line - points : ps2 to pe2
      float A2 = pe2.y - ps2.y;
      float B2 = ps2.x - pe2.x;
      float C2 = A2 * ps2.x + B2 * ps2.y;

      // Get delta and check if the lines are parallel
      float delta = A1 * B2 - A2 * B1;
      if (delta == 0) return F.none<Vector2>();

      // now return the Vector2 intersection point
      return F.some(new Vector2(
        (B2 * C1 - B1 * C2) / delta,
        (A1 * C2 - A2 * C1) / delta
      ));
    }

    //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
    //to each other. This function finds those two points. If the lines are not parallel, the function 
    //outputs true, otherwise false.
    public static bool ClosestPointsOnTwoLines(
      out Vector3 closestPointLine1, out Vector3 closestPointLine2, 
      Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2
    ) {
      closestPointLine1 = Vector3.zero;
      closestPointLine2 = Vector3.zero;

      var a = Vector3.Dot(lineVec1, lineVec1);
      var b = Vector3.Dot(lineVec1, lineVec2);
      var e = Vector3.Dot(lineVec2, lineVec2);

      var d = a * e - b * b;

      //lines are not parallel
      if (d != 0.0f) {

        Vector3 r = linePoint1 - linePoint2;
        float c = Vector3.Dot(lineVec1, r);
        float f = Vector3.Dot(lineVec2, r);

        float s = (b * f - c * e) / d;
        float t = (a * f - c * b) / d;

        closestPointLine1 = linePoint1 + lineVec1 * s;
        closestPointLine2 = linePoint2 + lineVec2 * t;

        return true;
      }

      else {
        return false;
      }
    }

    //This function finds out on which side of a line segment the point is located.
    //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
    //the line segment, project it on the line using ProjectPointOnLine() first.
    //Returns 0 if point is on the line segment.
    //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
    //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
    public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {

      Vector3 lineVec = linePoint2 - linePoint1;
      Vector3 pointVec = point - linePoint1;

      float dot = Vector3.Dot(pointVec, lineVec);

      //point is on side of linePoint2, compared to linePoint1
      if (dot > 0) {

        //point is on the line segment
        if (pointVec.magnitude <= lineVec.magnitude) {

          return 0;
        }

        //point is not on the line segment and it is on the side of linePoint2
        else {

          return 2;
        }
      }

      //Point is not on side of linePoint2, compared to linePoint1.
      //Point is not on the line segment and it is on the side of linePoint1.
      else {
        return 1;
      }
    }

    //Returns true if line segment made up of pointA1 and pointA2 is crossing line segment made up of
    //pointB1 and pointB2. The two lines are assumed to be in the same plane.
    public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2) {

      Vector3 closestPointA;
      Vector3 closestPointB;
      int sideA;
      int sideB;

      Vector3 lineVecA = pointA2 - pointA1;
      Vector3 lineVecB = pointB2 - pointB1;

      bool valid = ClosestPointsOnTwoLines(
        out closestPointA, out closestPointB, pointA1, 
        lineVecA.normalized, pointB1, lineVecB.normalized
      );

      //lines are not parallel
      if (valid) {

        sideA = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointA);
        sideB = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointB);

        if ((sideA == 0) && (sideB == 0)) {

          return true;
        }

        else {

          return false;
        }
      }

      //lines are parallel
      else {

        return false;
      }
    }

    /// <summary>
    /// Re-maps a number from one range to another
    /// </summary>
    public static float remap(this float value, float from1, float to1, float from2, float to2) {
      return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
  }
}

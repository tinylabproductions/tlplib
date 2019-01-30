using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class InterpolationUtils {
    public delegate Vector3 GetPoint(int index);

    [PublicAPI]
    public static float catmullRom(float v1, float v2, float v3, float v4, float percentage) {
      var percentageSquared = percentage * percentage;
      var a1 = (-v1 + 3f * v2 - 3f * v3 + v4) * percentage * percentageSquared;
      var a2 = (v1 * 2f - v2 * 5f + v3 * 4f - v4) * percentageSquared;
      var a3 = (-v1 + v3) * percentage;
      var a4 = v2 * 2f;

      return (a1 + a2 + a3 + a4) /  2;
    }

    public static Vector3 catmullRomGetPt(
      GetPoint getFromCollection, int collectionLength, int i, float percentage, bool pathClosed
    ) {
      if (collectionLength < 4) pathClosed = false;
      var maxASegmentIndex = collectionLength - 1;
      var a1 = pathClosed ? i - 1 < 0 ? maxASegmentIndex : i - 1 : Mathf.Clamp(i - 1, 0, maxASegmentIndex);
      var a2 = i;
      var a3 = pathClosed ? (i + 1) % maxASegmentIndex : Mathf.Clamp(i + 1, 0, maxASegmentIndex);
      var a4 = pathClosed ? (i + 2) % maxASegmentIndex : Mathf.Clamp(i + 2, 0, maxASegmentIndex);
      if (i == collectionLength - 2 && pathClosed) {
        a1 = collectionLength - 3;
        a2 = i;
        a3 = 0; 
        a4 = 1;
      }
      if (i == 0 && pathClosed) {
        a1 = collectionLength - 2;
        a2 = 0;
        a3 = 1;
        a4 = 2;
      }
      
      return new Vector3(
        catmullRom(getFromCollection(a1).x, getFromCollection(a2).x, getFromCollection(a3).x, getFromCollection(a4).x, percentage),
        catmullRom(getFromCollection(a1).y, getFromCollection(a2).y, getFromCollection(a3).y, getFromCollection(a4).y, percentage),
        catmullRom(getFromCollection(a1).z, getFromCollection(a2).z, getFromCollection(a3).z, getFromCollection(a4).z, percentage)
      );
      
    }
  
    [PublicAPI]
    public static float cubic(float v1, float v2, float v3, float v4, float aPercentage) {
      var percentageSquared = aPercentage * aPercentage;
      var a1 = v4 - v3 - v1 + v2;
      var a2 = v1 - v2 - a1;
      var a3 = v3 - v1;
      var a4 = v2;
  
      return a1 * aPercentage * percentageSquared + a2 * percentageSquared + a3 * aPercentage + a4;
    }
  
    [PublicAPI]
    public static Vector3 cubicGetPt(
      GetPoint getFromCollection, int collectionLength, int i, float percentage, bool pathClosed
    ) {
      //Closed path has last node in the same position as first one
      if (collectionLength < 4) pathClosed = false;
      var maxASegmentIndex = collectionLength - 1;
      var a1 = pathClosed ? i - 1 < 0 ? maxASegmentIndex : i - 1 : Mathf.Clamp(i - 1, 0, maxASegmentIndex);
      var a2 = i;
      var a3 = pathClosed ? (i + 1) % maxASegmentIndex : Mathf.Clamp(i + 1, 0, maxASegmentIndex);
      var a4 = pathClosed ? (i + 2) % maxASegmentIndex : Mathf.Clamp(i + 2, 0, maxASegmentIndex);
      if (i == collectionLength - 2 && pathClosed) {
        a1 = collectionLength - 3;
        a2 = i;
        a3 = 0; 
        a4 = 1;
      }
      if (i == 0 && pathClosed) {
        a1 = collectionLength - 2;
        a2 = 0;
        a3 = 1;
        a4 = 2;
      }
  
      return new Vector3(
        cubic(getFromCollection(a1).x, getFromCollection(a2).x, getFromCollection(a3).x, getFromCollection(a4).x, percentage),
        cubic(getFromCollection(a1).y, getFromCollection(a2).y, getFromCollection(a3).y, getFromCollection(a4).y, percentage),
        cubic(getFromCollection(a1).z, getFromCollection(a2).z, getFromCollection(a3).z, getFromCollection(a4).z, percentage)
      );
    }
  
    [PublicAPI]
    public static Vector3 hermiteGetPt(
      GetPoint getFromCollection, int collectionLength, int i, float aPercentage, bool aClosed, float aTension = 0, float aBias = 0
    ) {
      if (collectionLength < 4) aClosed = false;
      
      var a1 = aClosed ? i - 1 < 0 ? collectionLength - 2 : i - 1 : Mathf.Clamp(i - 1, 0, collectionLength - 1);
      var a2 = i;
      var a3 = aClosed ? (i + 1) % collectionLength : Mathf.Clamp(i + 1, 0, collectionLength - 1);
      var a4 = aClosed ? (i + 2) % collectionLength : Mathf.Clamp(i + 2, 0, collectionLength - 1);
      
      if (i == collectionLength - 2 && aClosed) {
        a1 = collectionLength - 3;
        a2 = i;
        a3 = 0; 
        a4 = 1;
      }
      if (i == 0 && aClosed) {
        a1 = collectionLength - 2;
        a2 = 0;
        a3 = 1;
        a4 = 2;
      }
  
      return new Vector3(
        hermite(getFromCollection(a1).x, getFromCollection(a2).x, getFromCollection(a3).x, getFromCollection(a4).x, aPercentage, aTension, aBias),
        hermite(getFromCollection(a1).y, getFromCollection(a2).y, getFromCollection(a3).y, getFromCollection(a4).y, aPercentage, aTension, aBias),
        hermite(getFromCollection(a1).z, getFromCollection(a2).z, getFromCollection(a3).z, getFromCollection(a4).z, aPercentage, aTension, aBias));
    }
  
    [PublicAPI]
    public static float hermite(float v1, float v2, float v3, float v4, float aPercentage, float aTension, float aBias) {
      var mu2 = aPercentage * aPercentage;
      var mu3 = mu2 * aPercentage;
      var m0 = (v2 - v1) * (1 + aBias) * (1 - aTension) / 2 + (v3 - v2) * (1 - aBias) * (1 - aTension) / 2;
      var m1 = (v3 - v2) * (1 + aBias) * (1 - aTension) / 2 + (v4 - v3) * (1 - aBias) * (1 - aTension) / 2;
      var a0 = 2 * mu3 - 3 * mu2 + 1;
      var a1 = mu3 - 2 * mu2 + aPercentage;
      var a2 = mu3 - mu2;
      var a3 = -2 * mu3 + 3 * mu2;
  
      return a0 * v2 + a1 * m0 + a2 * m1 + a3 * v3;
    }
  }
}
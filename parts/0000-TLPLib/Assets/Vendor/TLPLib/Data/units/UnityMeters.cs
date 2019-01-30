using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data.units {
  [Record]
  public partial struct UnityMeters {
    [PublicAPI] public readonly float meters;

    [PublicAPI] public static UnityMeters a(float meters) => new UnityMeters(meters);
    
    public static implicit operator float(UnityMeters um) => um.meters;
    
    public static UnityMeters operator +(UnityMeters a1, UnityMeters a2) => new UnityMeters(a1.meters + a2.meters);
    public static UnityMeters operator +(UnityMeters a1, float a2) => new UnityMeters(a1.meters + a2);
    
    public static UnityMeters operator -(UnityMeters a1, UnityMeters a2) => new UnityMeters(a1.meters - a2.meters);
    public static UnityMeters operator -(UnityMeters a1, float a2) => new UnityMeters(a1.meters - a2);
    
    public static UnityMeters operator *(UnityMeters a1, UnityMeters a2) => new UnityMeters(a1.meters * a2.meters);
    public static UnityMeters operator *(UnityMeters a1, float a2) => new UnityMeters(a1.meters * a2);
    
    public static UnityMeters operator /(UnityMeters a1, UnityMeters a2) => new UnityMeters(a1.meters / a2.meters);
    public static UnityMeters operator /(UnityMeters a1, float a2) => new UnityMeters(a1.meters / a2);
  }
}
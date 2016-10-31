namespace com.tinylabproductions.TLPLib.Data {
  public interface Numeric<A> {
    A add(A a1, A a2);
    A subtract(A a1, A a2);
    A fromInt(int i);
  }

  public static class Numeric {
    public static readonly Numeric<int> integer = new Int();
    public static readonly Numeric<uint> uInteger = new UInt();
    public static readonly Numeric<float> flt = new Float();
    public static readonly Numeric<double> dbl = new Double();

    class Int : Numeric<int> {
      public int add(int a1, int a2) => a1 + a2;
      public int subtract(int a1, int a2) => a1 - a2;
      public int fromInt(int i) => i;
    }

    class UInt : Numeric<uint> {
      public uint add(uint a1, uint a2) => a1 + a2;
      public uint subtract(uint a1, uint a2) => a1 - a2;
      public uint fromInt(int i) => (uint) i;
    }

    class Float : Numeric<float> {
      public float add(float a1, float a2) => a1 + a2;
      public float subtract(float a1, float a2) => a1 - a2;
      public float fromInt(int i) => i;
    }

    class Double : Numeric<double> {
      public double add(double a1, double a2) => a1 + a2;
      public double subtract(double a1, double a2) => a1 - a2;
      public double fromInt(int i) => i;
    }
  }
}
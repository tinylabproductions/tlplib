namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public interface Numeric<A> : Comparable<A> {
    A add(A a1, A a2);
    A subtract(A a1, A a2);
    A fromInt(int i);
  }

  public static class NumericOps {
    public static A add<A>(this Numeric<A> n, A a1, int a2) => n.add(a1, n.fromInt(a2));
    public static A subtract<A>(this Numeric<A> n, A a1, int a2) => n.subtract(a1, n.fromInt(a2));
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
      public bool eql(int a1, int a2) => a1 == a2;
      public CompareResult compare(int a1, int a2) => a1.CompareTo(a2).asCmpRes();
    }

    class UInt : Numeric<uint> {
      public uint add(uint a1, uint a2) => a1 + a2;
      public uint subtract(uint a1, uint a2) => a1 - a2;
      public uint fromInt(int i) => (uint) i;
      public bool eql(uint a1, uint a2) => a1 == a2;
      public CompareResult compare(uint a1, uint a2) => a1.CompareTo(a2).asCmpRes();
    }

    class Float : Numeric<float> {
      public float add(float a1, float a2) => a1 + a2;
      public float subtract(float a1, float a2) => a1 - a2;
      public float fromInt(int i) => i;
      public bool eql(float a1, float a2) => a1 == a2;
      public CompareResult compare(float a1, float a2) => a1.CompareTo(a2).asCmpRes();
    }

    class Double : Numeric<double> {
      public double add(double a1, double a2) => a1 + a2;
      public double subtract(double a1, double a2) => a1 - a2;
      public double fromInt(int i) => i;
      public bool eql(double a1, double a2) => a1 == a2;
      public CompareResult compare(double a1, double a2) => a1.CompareTo(a2).asCmpRes();
    }
  }
}
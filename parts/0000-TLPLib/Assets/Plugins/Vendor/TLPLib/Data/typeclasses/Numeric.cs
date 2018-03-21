using com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.fns;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public interface Numeric<A> : Comparable<A> {
    A add(A a1, A a2);
    A subtract(A a1, A a2);
    A mult(A a1, A a2);
    A div(A a1, A a2);
    A fromInt(int i);
  }

  public static class NumericOps {
    public static A add<A>(this Numeric<A> n, A a1, int a2) => n.add(a1, n.fromInt(a2));
    public static A subtract<A>(this Numeric<A> n, A a1, int a2) => n.subtract(a1, n.fromInt(a2));
  }

  public static class Numeric {
    public static readonly Numeric<int> integer = new Int();
    public static readonly Numeric<uint> uInteger = new UInt();
    public static readonly Numeric<ushort> ushort_ = new UShort();
    public static readonly Numeric<float> flt = new Float();
    public static readonly Numeric<double> dbl = new Double();

    class Int : Numeric<int> {
      public int add(int a1, int a2) => a1 + a2;
      public int subtract(int a1, int a2) => a1 - a2;
      public int mult(int a1, int a2) => a1 * a2;
      public int div(int a1, int a2) => a1 / a2;
      public int fromInt(int i) => i;
      public bool eql(int a1, int a2) => a1 == a2;
      public CompareResult compare(int a1, int a2) => Compare(a1, a2).asCmpRes();
      public int Compare(int a1, int a2) => a1.CompareTo(a2);
    }

    class UInt : Numeric<uint> {
      public uint add(uint a1, uint a2) => a1 + a2;
      public uint subtract(uint a1, uint a2) => a1 - a2;
      public uint mult(uint a1, uint a2) => a1 * a2;
      public uint div(uint a1, uint a2) => a1 / a2;
      public uint fromInt(int i) => (uint) i;
      public bool eql(uint a1, uint a2) => a1 == a2;
      public CompareResult compare(uint a1, uint a2) => Compare(a1, a2).asCmpRes();
      public int Compare(uint a1, uint a2) => a1.CompareTo(a2);
    }

    class UShort : Numeric<ushort> {
      public int Compare(ushort x, ushort y) => x.CompareTo(y);
      public bool eql(ushort a1, ushort a2) => a1 == a2;
      public CompareResult compare(ushort a1, ushort a2) => Compare(a1, a2).asCmpRes();
      public ushort add(ushort a1, ushort a2) => (ushort) (a1 + a2);
      public ushort subtract(ushort a1, ushort a2) => (ushort) (a1 - a2);
      public ushort mult(ushort a1, ushort a2) => (ushort) (a1 * a2);
      public ushort div(ushort a1, ushort a2) => (ushort) (a1 / a2);
      public ushort fromInt(int i) => (ushort) i;
    }

    class Float : Numeric<float> {
      public float add(float a1, float a2) => a1 + a2;
      public float subtract(float a1, float a2) => a1 - a2;
      public float mult(float a1, float a2) => a1 * a2;
      public float div(float a1, float a2) => a1 / a2;
      public float fromInt(int i) => i;
      public bool eql(float a1, float a2) => a1 == a2;
      public CompareResult compare(float a1, float a2) => Compare(a1, a2).asCmpRes();
      public int Compare(float a1, float a2) => a1.CompareTo(a2);
    }

    class Double : Numeric<double> {
      public double add(double a1, double a2) => a1 + a2;
      public double subtract(double a1, double a2) => a1 - a2;
      public double mult(double a1, double a2) => a1 * a2;
      public double div(double a1, double a2) => a1 / a2;
      public double fromInt(int i) => i;
      public bool eql(double a1, double a2) => a1 == a2;
      public CompareResult compare(double a1, double a2) => Compare(a1, a2).asCmpRes();
      public int Compare(double a1, double a2) => a1.CompareTo(a2);
    }
  }
}
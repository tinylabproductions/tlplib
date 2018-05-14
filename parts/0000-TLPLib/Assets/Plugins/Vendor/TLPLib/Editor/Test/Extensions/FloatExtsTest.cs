using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class FloatExtsTest : ImplicitSpecification {
    [Test]
    public void toIntClamped() => describe(() => {
      it["should return number itself when it is between min and max values and has no fraction"] = () => {
        foreach (var f in new[] {int.MinValue, 1f, 2353f, 3423f, -234234f, 864623f, int.MaxValue})
          f.toIntClamped().shouldEqual((int) f);
      };

      it["should cast number to int when it is between min and max values and has a fraction"] = () => {
        foreach (var (actual, expected) in new[] {
          F.t(1.34f, 1),
          F.t(2353.76f, 2353),
          F.t(3423.111123f, 3423),
          F.t(-234234.846f, -234234),
          F.t(-12.0123331f, -12)
        }) actual.toIntClamped().shouldEqual(expected);
      };

      it["should clamp to int.MaxValue"] =
        () => float.MaxValue.toIntClamped().shouldEqual(int.MaxValue);

      it["should clamp to int.MinValue"] =
        () => float.MinValue.toIntClamped().shouldEqual(int.MinValue);
    });
  }
}
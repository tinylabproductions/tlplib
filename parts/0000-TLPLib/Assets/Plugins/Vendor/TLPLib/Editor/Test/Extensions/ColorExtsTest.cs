using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class ColorExtsTest : ImplicitSpecification {
    [Test]
    public void with32() => describe(() => {
      var color = new Color32(1, 2, 3, 4);
      const byte value = 100;

      void notChange(
        Color32 newColor,
        bool r = true, bool g = true, bool b = true, bool a = true
      ) {
        if (r) it["should not change r"] = () => newColor.r.shouldEqual(color.r);
        if (g) it["should not change g"] = () => newColor.g.shouldEqual(color.g);
        if (b) it["should not change b"] = () => newColor.b.shouldEqual(color.b);
        if (a) it["should not change a"] = () => newColor.a.shouldEqual(color.a);
      }
      
      when["changing r"] = () => {
        var newColor = color.with32(r: F.some(value));
        it["should change r"] = () => newColor.r.shouldEqual(value);
        notChange(newColor, r: false);
      };
      
      when["changing g"] = () => {
        var newColor = color.with32(g: F.some(value));
        it["should change g"] = () => newColor.g.shouldEqual(value);
        notChange(newColor, g: false);
      };
      
      when["changing b"] = () => {
        var newColor = color.with32(b: F.some(value));
        it["should change b"] = () => newColor.b.shouldEqual(value);
        notChange(newColor, b: false);
      };
      
      when["changing a"] = () => {
        var newColor = color.with32(a: F.some(value));
        it["should change a"] = () => newColor.a.shouldEqual(value);
        notChange(newColor, a: false);
      };
    });
  }
}
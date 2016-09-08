using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class ImmutableHashSetExtsTestToggle {
    [Test]
    public void WhenExists() =>
      ImmutableHashSet.Create(1, 2, 3).toggle(2).shouldEqual(ImmutableHashSet.Create(1, 3));

    [Test]
    public void WhenDoesNotExist() =>
      ImmutableHashSet.Create(1, 3).toggle(2).shouldEqual(ImmutableHashSet.Create(1, 2, 3));
  }
}
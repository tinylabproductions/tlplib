using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Data;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Test.Components.DebugConsole {
  public class DebugSequenceDirectionTestSequenceValidation {
    [Test]
    public void WhenGood() {
      var _ = new DConsole.DebugSequenceDirectionData(sequence: ImmutableList.Create(
        DConsole.Direction.Left, DConsole.Direction.Up,
        DConsole.Direction.Down, DConsole.Direction.Right
      ));
    }

    [Test]
    public void WhenBadInStart() {
      Assert.Throws<ArgumentException>(() => new DConsole.DebugSequenceDirectionData(sequence: ImmutableList.Create(
        DConsole.Direction.Left, DConsole.Direction.Left,
        DConsole.Direction.Up, DConsole.Direction.Down, DConsole.Direction.Right
      )));
    }

    [Test]
    public void WhenBadInEnd() {
      Assert.Throws<ArgumentException>(() => new DConsole.DebugSequenceDirectionData(sequence: ImmutableList.Create(
        DConsole.Direction.Left, DConsole.Direction.Up, DConsole.Direction.Down,
        DConsole.Direction.Right, DConsole.Direction.Right
      )));
    }

    [Test]
    public void WhenSingleElement() {
      new DConsole.DebugSequenceDirectionData(sequence: ImmutableList.Create(
        DConsole.Direction.Left
      ));
    }
  }
}
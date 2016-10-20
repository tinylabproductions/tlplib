using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Data {
  public static class Once {
    public struct Builder<In> {
      public Once<In, Out> to<Out>(Fn<In, Out> fn) { return a(fn); }
    }

    public static Builder<In> from<In>() { return new Builder<In>(); }

    public static Once<In, Out> a<In, Out>(Fn<In, Out> fn) {
      return new Once<In, Out>(fn);
    }
  }

  /* Something that needs to only happen once per unique argument. Pretty much a cache. */
  public class Once<In, Out> {
    readonly Dictionary<In, Out> cache = new Dictionary<In, Out>();
    readonly Fn<In, Out> fn;

    public Once(Fn<In, Out> fn) { this.fn = fn; }

    public Out run(In input) {
      return cache.get(input).fold(
        () => {
          var output = fn(input);
          cache[input] = output;
          return output;
        },
        _ => _
      );
    }
  }
}

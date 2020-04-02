using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class Scope {
    public static void create<A>(Action local) => local();
    public static void create<A, Data>(Data d, Action<Data> local) => local(d);
    public static T create<T>(Func<T> local) => local();
    public static T create<Data, T>(Data d, Func<Data, T> local) => local(d);
  }
}
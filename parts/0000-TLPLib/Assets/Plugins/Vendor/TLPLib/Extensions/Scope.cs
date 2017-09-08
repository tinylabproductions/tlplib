using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class Scope {
    public static void create(Action local) => local();
    public static T create<T>(Fn<T> local) => local();

    public static void locally(this object any, Action local) => local();
    public static T locally<T>(this object any, Fn<T> local) => local();
  }
}
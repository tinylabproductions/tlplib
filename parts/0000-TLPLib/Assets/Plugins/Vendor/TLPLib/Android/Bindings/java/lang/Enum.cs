#if UNITY_ANDROID
using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public abstract class Enum : Binding {
    protected Enum(AndroidJavaObject java) : base(java) {}
  }

// Example usage:
//
//  public sealed class ResponseStatus : Enum {
//    static readonly EnumBuilder<ResponseStatus> builder = 
//      EnumBuilder.a(
//        "com.ampiri.sdk.mediation.ResponseStatus",
//        jo => new ResponseStatus(jo)
//      );
//
//    ResponseStatus(AndroidJavaObject java) : base(java) { }
//
//    public static readonly ResponseStatus
//      OK = builder.a("OK"),
//      EMPTY = builder.a("EMPTY"),
//      ERROR = builder.a("ERROR");
//  }
  public static class EnumBuilder {
    public static EnumBuilder<A> a<A>(
      AndroidJavaClass klass,
      Fn<AndroidJavaObject, A> create
    ) where A : Enum => new EnumBuilder<A>(klass, create);

    public static EnumBuilder<A> a<A>(
      string klass,
      Fn<AndroidJavaObject, A> create
    ) where A : Enum => a(
      // Fake support when running in editor
      Application.platform == RuntimePlatform.Android 
        ? new AndroidJavaClass(klass)
        : null,
      create
    );
  }

  public class EnumBuilder<A> where A : Enum {
    readonly AndroidJavaClass klass;
    readonly Fn<AndroidJavaObject, A> create;

    public EnumBuilder(
      AndroidJavaClass klass,
      Fn<AndroidJavaObject, A> create
    ) {
      this.klass = klass;
      this.create = create;
    }

    public A a(string name) => create(
      Application.platform == RuntimePlatform.Android
      ? klass.GetStatic<AndroidJavaObject>(name)
      // Fake support when running in editor
      : null
    );
  }
}
#endif
#if UNITY_ANDROID
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using pzd.lib.exts;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.accounts {
  public class AccountManager : Binding {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.accounts.AccountManager");

    public AccountManager(AndroidJavaObject java) : base(java) {}

    public static AccountManager get(Context context) =>
      new AccountManager(klass.csjo("get", context.java));

    public ImmutableArray<Account> getAccounts() =>
      ImmutableArrayUnsafe.createByMove(
        java.Call<AndroidJavaObject[]>("getAccounts").map(ajo => new Account(ajo))
      );

    public ImmutableArray<Account> getAccountsByType(string type) =>
      ImmutableArrayUnsafe.createByMove(
        java.Call<AndroidJavaObject[]>("getAccountsByType", type).map(ajo => new Account(ajo))
      );

    public static ImmutableArray<Account> getGoogleAccounts() =>
      get(AndroidActivity.current).getAccountsByType("com.google");
  }
}
#endif
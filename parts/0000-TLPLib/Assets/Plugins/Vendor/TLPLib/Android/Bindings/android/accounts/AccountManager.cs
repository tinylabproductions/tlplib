using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.accounts {
  public class AccountManager : Binding {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.accounts.AccountManager");

    public AccountManager(AndroidJavaObject java) : base(java) {}

    public static AccountManager get(Context context) =>
      new AccountManager(klass.csjo("get", context.java));

    public Account[] getAccounts() =>
      java.Call<AndroidJavaObject[]>("getAccounts").map(ajo => new Account(ajo));

    public Account[] getAccountsByType(string type) =>
      java.Call<AndroidJavaObject[]>("getAccountsByType", type).map(ajo => new Account(ajo));

    public static Account[] getGoogleAccounts() =>
      get(AndroidActivity.current).getAccountsByType("com.google");
  }
}
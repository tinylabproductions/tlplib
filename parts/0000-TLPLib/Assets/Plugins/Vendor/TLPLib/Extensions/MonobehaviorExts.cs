using System.Collections.Generic;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Extensions {
  public static class MonobehaviorExts {
    public static void setActive<A>(this ICollection<A> list, bool isActive) where A : MonoBehaviour {
      foreach (var item in list) item.gameObject.SetActive(isActive);
    }
  }
}
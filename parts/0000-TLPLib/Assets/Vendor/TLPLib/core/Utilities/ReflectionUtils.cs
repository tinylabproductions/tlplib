﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ReflectionUtils {
    static readonly Dictionary<(MemberInfo, Type, bool), object[]> attributesCache =
      new Dictionary<(MemberInfo, Type, bool), object[]>();
    
    public static bool hasAttribute<T>(this MemberInfo mi) =>
      getAttributes<T>(mi).Any();

    public static IEnumerable<T> getAttributes<T>(this MemberInfo mi, bool inherit = false) {
      var key = (mi, typeof(T), inherit);
      return attributesCache.getOrUpdate(key, _key => {
        var (_mi, _type, _inherit) = _key;
        return _mi.GetCustomAttributes(_type, _inherit);
      }) as T[];
    }
  }
}

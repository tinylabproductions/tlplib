﻿using System;
 using System.Collections.Concurrent;
 using System.Collections.Generic;
using System.Linq;
using System.Reflection;

 namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ReflectionUtils {
    static readonly ConcurrentDictionary<(MemberInfo, Type, bool), object[]> attributesCache = new();
    
    public static bool hasAttribute<T>(this MemberInfo mi) =>
      getAttributes<T>(mi).Any();

    public static IEnumerable<T> getAttributes<T>(this MemberInfo mi, bool inherit = false) {
      var type = typeof(T);
      var key = (mi, type, inherit);
      if (!attributesCache.TryGetValue(key, out var value)) {
        value = mi.GetCustomAttributes(type, inherit);
        attributesCache.TryAdd(key, value);
      }
      return value as T[];
    }
  }
}

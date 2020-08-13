﻿using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
 using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;

 namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    static readonly Dictionary<Type, Type[]> requireComponentCache = new Dictionary<Type, Type[]>();

    public static void checkRequireComponents(
      CheckContext context, GameObject go, Type type, AddError addError
    ) {
      var requiredComponents = requireComponentCache.getOrUpdate(type, _type => {
        return _type
          .getAttributes<RequireComponent>(inherit: true)
          .SelectMany(rc => new[] {F.opt(rc.m_Type0), F.opt(rc.m_Type1), F.opt(rc.m_Type2)}.flatten(),
            (rc, requiredType) => requiredType)
          .ToArray();
      });
      foreach (var requiredType in requiredComponents) {
        if (!go.GetComponent(requiredType)) {
          addError(() => Error.requiredComponentMissing(go, requiredType, type, context));
        }
      }
    }
  }
}
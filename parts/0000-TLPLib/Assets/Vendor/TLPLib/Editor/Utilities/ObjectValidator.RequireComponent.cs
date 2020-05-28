﻿using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine.Events;
using JetBrains.Annotations;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.validations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    static readonly Dictionary<Type, Type[]> requireComponentCache = new Dictionary<Type, Type[]>();

    public static void checkRequireComponents(
      CheckContext context, GameObject go, Type type, List<Error> errors
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
          errors.Add(Error.requiredComponentMissing(go, requiredType, type, context));
        }
      }
    }
  }
}

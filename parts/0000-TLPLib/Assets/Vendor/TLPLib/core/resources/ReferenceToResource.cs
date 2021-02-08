using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.resources {
  /// <summary>
  /// Allows you to have a reference to Resources folder and have it validated by an object validator.
  ///
  /// Must be subclassed and made non-generic to use.
  /// </summary>
  [PublicAPI, Serializable] public partial class ReferenceToResource<A> : OnObjectValidate where A : Object {
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized
    [
      SerializeField, NotNull, PublicAccessor, 
      ValidateInput(nameof(validate), "Can't find the specified name in Resources!")
    ] string _name;
    // ReSharper restore NotNullMemberIsNotInitialized
#pragma warning restore 649
    
    public bool onObjectValidateIsThreadSafe => false;
    
    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (load().leftValueOut(out var error)) {
        yield return new ErrorMsg(error);
      }
    }

    static bool validate(string name) => load(name).isRight;

    public Either<string, A> load() => load(_name);
    
    public static Either<string, A> load(string name) {
      var maybeA = Resources.Load<A>(name);
      return maybeA 
        ? Either<string, A>.Right(maybeA) 
        : $"Can't load {typeof(A).FullName} from path '{name}' in Resources!";
    }
  }
  [Serializable] public sealed class ReferenceToResourceImage : ReferenceToResource<Image> {}
}
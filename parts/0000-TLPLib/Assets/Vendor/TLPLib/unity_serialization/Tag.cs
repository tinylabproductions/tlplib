using System;

using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  [Serializable] public struct Tag {
    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField/*, UnityTag*/] public string value;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649
    #endregion
  }
  [Serializable, PublicAPI] public class UnityOptionTag : UnityOption<Tag> { }
}
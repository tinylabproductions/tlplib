using System;
using com.tinylabproductions.TLPLib.Components.gradient;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.BlockBar {
  public partial class GradientSimpleBlockBar : BlockBar<GradientSimple> {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] Colors active, inactive;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public new class Init : BlockBar<GradientSimple>.Init {
      public Init(GradientSimpleBlockBar backing) : base(
        backing,
        (el, active) => {
          el.topColor = active ? backing.active.top : backing.inactive.top;
          el.bottomColor = active ? backing.active.bottom : backing.inactive.bottom;
          el.setAllDirty();
        }) { }
    }

    [Serializable]
    partial struct Colors {
      #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
      [SerializeField, PublicAccessor] Color _top, _bottom;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

      #endregion
    }
  }
}
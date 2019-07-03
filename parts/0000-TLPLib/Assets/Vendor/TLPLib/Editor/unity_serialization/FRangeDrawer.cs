using System.Reflection;
using com.tinylabproductions.TLPLib.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

//Copied from MinMaxSliderAttributeDrawer
namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  /// <summary>
  /// Draws FRange properties marked with <see cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />.
  /// </summary>
  /// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
  /// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
  /// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
  /// <seealso cref="T:UnityEngine.RangeAttribute" />
  /// <seealso cref="T:UnityEngine.DelayedAttribute" />
  /// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
  public sealed class FRangeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, FRange> {
    private string errorMessage;
    private InspectorPropertyValueGetter<int> intMinGetter;
    private InspectorPropertyValueGetter<float> floatMinGetter;
    private InspectorPropertyValueGetter<int> intMaxGetter;
    private InspectorPropertyValueGetter<float> floatMaxGetter;
    private InspectorPropertyValueGetter<Vector2> vector2MinMaxGetter;

    /// <summary>Initializes the drawer.</summary>
    protected override void Initialize() {
      MemberInfo memberInfo;
      if (this.Attribute.MinMember != null && MemberFinder.Start(this.Property.ParentType).IsNamed(this.Attribute.MinMember).HasNoParameters().TryGetMember(out memberInfo, out this.errorMessage)) {
        System.Type returnType = memberInfo.GetReturnType();
        if (returnType == typeof (int))
          this.intMinGetter = new InspectorPropertyValueGetter<int>(this.Property, this.Attribute.MinMember, true, true);
        else if (returnType == typeof (float))
          this.floatMinGetter = new InspectorPropertyValueGetter<float>(this.Property, this.Attribute.MinMember, true, true);
      }
      if (this.Attribute.MaxMember != null && MemberFinder.Start(this.Property.ParentType).IsNamed(this.Attribute.MaxMember).HasNoParameters().TryGetMember(out memberInfo, out this.errorMessage)) {
        System.Type returnType = memberInfo.GetReturnType();
        if (returnType == typeof (int))
          this.intMaxGetter = new InspectorPropertyValueGetter<int>(this.Property, this.Attribute.MaxMember, true, true);
        else if (returnType == typeof (float))
          this.floatMaxGetter = new InspectorPropertyValueGetter<float>(this.Property, this.Attribute.MaxMember, true, true);
      }
      if (this.Attribute.MinMaxMember == null)
        return;
      this.vector2MinMaxGetter = new InspectorPropertyValueGetter<Vector2>(this.Property, this.Attribute.MinMaxMember, true, true);
      if (this.errorMessage == null)
        return;
      this.errorMessage = this.vector2MinMaxGetter.ErrorMessage;
    }

    /// <summary>Draws the property.</summary>
    protected override void DrawPropertyLayout(GUIContent label) {
      Vector2 limits;
      if (this.vector2MinMaxGetter != null && this.errorMessage == null) {
        limits = this.vector2MinMaxGetter.GetValue();
      }
      else {
        limits.x = this.intMinGetter == null ? (this.floatMinGetter == null ? this.Attribute.MinValue : this.floatMinGetter.GetValue()) : (float) this.intMinGetter.GetValue();
        limits.y = this.intMaxGetter == null ? (this.floatMaxGetter == null ? this.Attribute.MaxValue : this.floatMaxGetter.GetValue()) : (float) this.intMaxGetter.GetValue();
      }
      if (this.errorMessage != null)
        SirenixEditorGUI.ErrorMessageBox(this.errorMessage, true);

      var sv = this.ValueEntry.SmartValue;
      var v2 = SirenixEditorFields.MinMaxSlider(label, new Vector2(sv.from, sv.to), limits, this.Attribute.ShowFields, new GUILayoutOption[0]);
      this.ValueEntry.SmartValue = new FRange(v2.x, v2.y);
    }
  }
}
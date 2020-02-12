#if ODIN_INSPECTOR
using com.tinylabproductions.TLPLib.unity_serialization;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  public static class EitherDrawer {
    public static void drawPropertyLayout<A, B>(
      string isLeftName, string leftName, string rightName, 
      InspectorProperty property, GUIContent label
    ) where A : new() where B : new() {
      var isLeftProperty = property.Children[isLeftName];
      var isLeft = (bool) isLeftProperty.ValueEntry.WeakSmartValue;
      var valueName = isLeft ? leftName : rightName;
      var otherName = isLeft ? rightName : leftName;
      var valueProperty = property.getChildSmart(valueName);
      var otherProperty = property.getChildSmart(otherName);

      var oneLine = valueProperty.Children.Count == 1 && valueProperty.Children[0].Children.Count == 0;

      SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
      EditorGUI.BeginChangeCheck();
      isLeftProperty.Draw(null);
      var isLeftChanged = EditorGUI.EndChangeCheck();
      if (!oneLine) SirenixEditorGUI.EndHorizontalPropertyLayout();
      
      if (oneLine) {
        valueProperty.Draw(GUIContent.none);
      }
      else {
        GUIHelper.PushIndentLevel(EditorGUI.indentLevel + 1);
        valueProperty.Draw(valueProperty.Label);
        GUIHelper.PopIndentLevel();
      }

      if (isLeftChanged) {
        otherProperty.ValueEntry.WeakSmartValue = isLeft ? (object) new B() : new A();
      }

      if (oneLine) SirenixEditorGUI.EndHorizontalPropertyLayout();
    }
  }

  [UsedImplicitly]
  public class EitherDrawer<TEither, A, B> : OdinValueDrawer<TEither> 
    where TEither : UnityEither<A, B> 
    where A : new()
    where B : new()
  {
    protected override void DrawPropertyLayout(GUIContent label) => 
      EitherDrawer.drawPropertyLayout<A, B>("_isA", "a", "b", Property, label);
  }

  // [UsedImplicitly]
  // public class UnityEitherAttributeProcessor<TOpt, A> : OdinAttributeProcessor<TOpt> where TOpt : UnityEither<A> {
  //   // move attributes from whole Either field to Either value
  //   public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
  //     attributes.RemoveAll(a => !a.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true));
  //   }
  //
  //   public override void ProcessChildMemberAttributes(
  //     InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes
  //   ) {
  //     if (member.Name == "_value") {
  //       attributes.AddRange(
  //         parentProperty.Info.GetMemberInfo().GetAttributes().Where(a => !parentProperty.Attributes.Contains(a))
  //       );
  //     }
  //   }
  // }
}
#endif
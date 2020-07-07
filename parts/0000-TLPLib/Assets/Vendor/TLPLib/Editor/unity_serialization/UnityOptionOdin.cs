#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.unity_serialization;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  public static class OptionDrawer {
    public static void drawPropertyLayout<TOpt>(
      string isSomeName, string valueName,
      InspectorProperty property, IPropertyValueEntry<TOpt> valueEntry, GUIContent label
    ) where TOpt : new() {
      var isSet = property.Children[isSomeName];
      var value = property.getChildSmart(valueName);

      var oneLine = value.Children.Count == 0 ||
                    value.Children.Count == 1 && value.Children[0].Children.Count == 0;

      SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
      EditorGUI.BeginChangeCheck();
      isSet.Draw(null);
      var changed = EditorGUI.EndChangeCheck();
      if (!oneLine) SirenixEditorGUI.EndHorizontalPropertyLayout();
      if ((bool) isSet.ValueEntry.WeakSmartValue) {
        if (oneLine) {
          value.Draw(GUIContent.none);
        }
        else {
          GUIHelper.PushIndentLevel(EditorGUI.indentLevel + 1);
          value.Draw(value.Label);
          GUIHelper.PopIndentLevel();
        }
      }
      else {
        if (changed) {
          valueEntry.SmartValue = new TOpt();
        }
      }

      if (oneLine) SirenixEditorGUI.EndHorizontalPropertyLayout();
    }
  }

  [UsedImplicitly]
  public class OptionDrawer<TOpt, A> : OdinValueDrawer<TOpt> where TOpt : UnityOption<A>, new() {
    protected override void DrawPropertyLayout(GUIContent label) => 
      OptionDrawer.drawPropertyLayout("_isSome", "_value", Property, ValueEntry, label);
  }

  [UsedImplicitly]
  public class UnityOptionAttributeProcessor<TOpt, A> : OdinAttributeProcessor<TOpt> where TOpt : UnityOption<A> {
    // move attributes from whole option field to option value
    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
      attributes.RemoveAll(a => !a.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true));
    }

    public override void ProcessChildMemberAttributes(
      InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes
    ) {
      if (member.Name == "_value") {
        attributes.AddRange(
          parentProperty.Info.GetMemberInfo().GetAttributes().Where(a => !parentProperty.Attributes.Contains(a))
        );
      }
    }
  }
}
#endif
#if ODIN_INSPECTOR
using com.tinylabproductions.TLPLib.unity_serialization;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[UsedImplicitly]
public class OptionDrawer<TOpt, A> : OdinValueDrawer<TOpt> where TOpt : UnityOption<A>, new() {
    protected override void DrawPropertyLayout(GUIContent label) {
        var isSet = Property.Children["_isSome"];
        var value = Property.Children["_value"];

        var oneLine = value.Children.Count <= 1;

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
                ValueEntry.SmartValue = new TOpt();
            }
        }
        if (oneLine) SirenixEditorGUI.EndHorizontalPropertyLayout();
    }
}
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers {
  [DrawerPriority(0, 0, 2002), UsedImplicitly]
  public sealed class ChildDropdownAttributeDrawer : OdinAttributeDrawer<ChildDropdownAttribute> {
    ValueDropdownHelper helper;

    protected override void Initialize() {
      var type = Property.ValueEntry.TypeOfValue;
      if (Property.ChildResolver is IOrderedCollectionResolver col) {
        type = col.ElementType;
      }

      helper = new ValueDropdownHelper(
        this,
        _ => CallNextDrawer(_),
        new DropdownSettings(IsUniqueList: Attribute.IsUniqueList, AppendNextDrawer: true),
        getItems: () => {
          var go = ((MonoBehaviour) Property.Tree.UnitySerializedObject.targetObject).gameObject;
          var result = new List<ValueDropdownItem>();

          void enterChild(string path, Transform t) {
            path += $"/{t.gameObject.name}";
            if (type == typeof(GameObject)) {
              result.Add(new ValueDropdownItem(path, t.gameObject));
            }
            else {
              var components = t.gameObject.GetComponents(type);
              if (components.Length == 1) {
                result.Add(new ValueDropdownItem(path, components[0]));
              }
              else {
                for (var i = 0; i < components.Length; i++) {
                  result.Add(new ValueDropdownItem($"{path} ({i} - {components[i].GetType().Name})", components[i]));
                }
              }
            }

            for (var i = 0; i < t.childCount; i++) {
              enterChild(path, t.GetChild(i));
            }
          }

          enterChild("", go.transform);
          return result;
        }
      );
    }

    protected override void DrawPropertyLayout(GUIContent label) {
      helper.DrawPropertyLayout(label);
    }
  }
}
#endif
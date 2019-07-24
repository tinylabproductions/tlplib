using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.attributes;
using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers {
  // InlineEditorDrawer is 3000
  [DrawerPriority(0, 0, 3000 + 10), UsedImplicitly]
  public sealed class TLPCreateDerivedAttributeDrawer : OdinAttributeDrawer<TLPCreateDerivedAttribute> {
    ValueDropdownHelper helper;

    static readonly Dictionary<Type, Type[]> cache = new Dictionary<Type, Type[]>();

    protected override void Initialize() {
      var type = Property.ValueEntry.TypeOfValue;
      if (Property.ChildResolver is IOrderedCollectionResolver col) {
        type = col.ElementType;
      }

      helper = new ValueDropdownHelper(
        this,
        _ => CallNextDrawer(_),
        new DropdownSettings(IsUniqueList: false, AppendNextDrawer: true),
        getItems: () => {
          var items = cache.getOrUpdate(
            type,
            newType => AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(_ => _.GetTypes())
              .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(newType)).ToArray()
          );

          var result = new List<ValueDropdownItem>();

          var go = ((MonoBehaviour) Property.Tree.UnitySerializedObject.targetObject).gameObject;

          foreach (var item in items) {
            Component component = null;
            result.Add(new ValueDropdownItem(item.Name, new DropdownValueFunc(() => {
              // odin calls this 2 times for unknown reasons
              if (!component) component = Undo.AddComponent(go, item);
              return component;
            })));
          }

          return result;
        }
      );
    }

    protected override void DrawPropertyLayout(GUIContent label) {
      helper.DrawPropertyLayout(label);
    }
  }

  [UsedImplicitly]
  public class CreateDerivedAttributeProcessor<A> : OdinAttributeProcessor<A> where A : class {
    static HashSet<Type> cacheDerived;

    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
      if (cacheDerived == null) {
        cacheDerived = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes())
          .Where(myType => myType.IsSubclassOf(typeof(TLPComponentMonoBehaviour))).toHashSet();
      }

      if (cacheDerived.Contains(property.Info.TypeOfValue)
          || cacheDerived.Contains(property.Info.TypeOfValue.GetElementType())
      ) {
        attributes.Add(new TLPCreateDerivedAttribute());
      }

      if (attributes.Exists(a => a is TLPCreateDerivedAttribute)) {
        attributes.Add(new InlineEditorAttribute());
      }
    }
  }
}
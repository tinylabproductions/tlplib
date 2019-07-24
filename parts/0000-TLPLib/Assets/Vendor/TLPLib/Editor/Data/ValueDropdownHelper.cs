using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.reflection;
using GenerationAttributes;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers {
  [Record]
  public partial class DropdownSettings {
    public readonly int NumberOfItemsBeforeEnablingSearch = 10;
    public readonly bool IsUniqueList;
    public readonly bool DrawDropdownForListElements = true;
    public readonly bool DisableListAddButtonBehaviour;
    public readonly bool ExcludeExistingValuesInList;
    public readonly bool ExpandAllMenuItems;
    public readonly bool AppendNextDrawer;
    public readonly bool DisableGUIInAppendedDrawer;
    public readonly bool DoubleClickToConfirm;
    public readonly bool FlattenTreeView;
    public readonly int DropdownWidth;
    public readonly int DropdownHeight;
    public readonly string DropdownTitle;
    public readonly bool SortDropdownItems;
    public readonly bool HideChildProperties = false;

    public DropdownSettings(bool IsUniqueList, bool AppendNextDrawer) {
      this.IsUniqueList = IsUniqueList;
      this.AppendNextDrawer = AppendNextDrawer;
    }
  }

  // we need this class because if we use Func<object>, odin tries to call it every frame
  public class DropdownValueFunc {
    public readonly Func<object> create;

    public DropdownValueFunc(Func<object> create) {
      this.create = create;
    }
  }

  internal class TLPIValueDropdownEqualityComparer : IEqualityComparer<object> {
    bool isTypeLookup;

    public TLPIValueDropdownEqualityComparer(bool isTypeLookup) { this.isTypeLookup = isTypeLookup; }

    public new bool Equals(object x, object y) {
      if (x is ValueDropdownItem xItem) x = xItem.Value;
      if (y is ValueDropdownItem)
        y = ((ValueDropdownItem) y).Value;
      return EqualityComparer<object>.Default.Equals(x, y) || x == null == (y == null) && this.isTypeLookup &&
        (x as Type ?? x.GetType()) == (y as Type ?? y.GetType());
    }

    public int GetHashCode(object obj) {
      if (obj == null)
        return -1;
      if (obj is ValueDropdownItem)
        obj = ((ValueDropdownItem) obj).Value;
      if (obj == null)
        return -1;
      if (this.isTypeLookup)
        return (obj as Type ?? obj.GetType()).GetHashCode();
      return obj.GetHashCode();
    }
  }

  /// <summary>
  /// Slightly modified code from ValueDropdownAttributeDrawer. Probably can't put this on TLPLib
  /// </summary>
  public class ValueDropdownHelper
  {
    readonly InspectorProperty Property;
    readonly Action<GUIContent> CallNextDrawer;
    readonly DropdownSettings Attribute;
    readonly Func<IEnumerable<ValueDropdownItem>> rawGetter;

    readonly string error;
    GUIContent label;
    readonly bool isList;
    readonly bool isListElement;
    readonly Func<IEnumerable<ValueDropdownItem>> getValues;
    readonly Func<IEnumerable<object>> getSelection;
    IEnumerable<object> result;
    bool enableMultiSelect;
    Dictionary<object, string> nameLookup;
    readonly LocalPersistentContext<bool> isToggled;
    GenericSelector<object> inlineSelector;
    IEnumerable<object> nextResult;

    public ValueDropdownHelper(OdinDrawer drawer, Action<GUIContent> callNextDrawer, DropdownSettings settings, Func<IEnumerable<ValueDropdownItem>> getItems, string errorMessage = null)
    {
      CallNextDrawer = callNextDrawer;
      rawGetter = getItems;
      Property = drawer.Property;
      Attribute = settings;

      isToggled = drawer.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);

      error = errorMessage;
      isList = Property.ChildResolver is IOrderedCollectionResolver;
      isListElement = Property.Info.GetMemberInfo() == null;
      getSelection = () => Property.ValueEntry.WeakValues.Cast<object>();
      getValues = rawGetter;

      ReloadDropdownCollections();
    }

    void ReloadDropdownCollections()
    {
      var first = rawGetter().FirstOrDefault();
      var isNamedValueDropdownItems = first.Text != null;

      if (isNamedValueDropdownItems)
      {
        var vals = getValues();
        nameLookup = new Dictionary<object, string>(new TLPIValueDropdownEqualityComparer(false));
        foreach (var item in vals)
        {
          nameLookup[item] = item.Text;
          nameLookup[item] = item.Text;
        }
      }
      else
      {
        nameLookup = null;
      }
    }

    static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
    {
      return query.Select(x =>
      {
        if (x is ValueDropdownItem)
        {
          return (ValueDropdownItem)x;
        }

        if (x is IValueDropdownItem)
        {
          var ix = x as IValueDropdownItem;
          return new ValueDropdownItem(ix.GetText(), ix.GetValue());
        }

        return new ValueDropdownItem(null, x);
      });
    }

    public void DrawPropertyLayout(GUIContent label)
    {
      this.label = label;

      if (Property.ValueEntry == null)
      {
        CallNextDrawer(label);
        return;
      }

      if (error != null)
      {
        SirenixEditorGUI.ErrorMessageBox(error);
        CallNextDrawer(label);
      }
      else if (isList)
      {
        if (Attribute.DisableListAddButtonBehaviour)
        {
          CallNextDrawer(label);
        }
        else {
          
          var field = typeof(CollectionDrawerStaticInfo)
            .GetField("NextCustomAddFunction", BindingFlags.Static | BindingFlags.NonPublic);
          field.SetValue(null, new Action(OpenSelector));
          CallNextDrawer(label);
          if (result != null)
          {
            AddResult(result);
            result = null;
          }
        }
      }
      else
      {
        if (Attribute.DrawDropdownForListElements || !isListElement)
        {
          DrawDropdown();
        }
        else
        {
          CallNextDrawer(label);
        }
      }
    }

    void AddResult(IEnumerable<object> query)
    {
      if (isList)
      {
        var changer = Property.ChildResolver as IOrderedCollectionResolver;

        if (enableMultiSelect)
        {
          changer.QueueClear();
        }

        foreach (var item in query) {
          var realItem = item;
          { if (realItem is DropdownValueFunc f) realItem = f.create(); }

          object[] arr = new object[Property.ParentValues.Count];

          for (int i = 0; i < arr.Length; i++)
          {
            arr[i] = SerializationUtility.CreateCopy(realItem);
          }

          changer.QueueAdd(arr);
        }
      }
      else
      {
        var first = query.FirstOrDefault();
        { if (first is DropdownValueFunc f)  first = f.create(); }
        for (int i = 0; i < Property.ValueEntry.WeakValues.Count; i++)
        {
          Property.ValueEntry.WeakValues[i] = SerializationUtility.CreateCopy(first);
        }
      }
    }

    void DrawDropdown()
    {
      IEnumerable<object> newResult = null;

      //if (this.Attribute.InlineSelector)
      //{
      //  bool recreateBecauseOfListChange = false;

      //  if (Event.current.type == EventType.Layout)
      //  {
      //    var _newCol = this.rawGetter.GetValue();
      //    if (_newCol != this.rawPrevGettedValue)
      //    {
      //      this.ReloadDropdownCollections();
      //      recreateBecauseOfListChange = true;
      //    }

      //    var iList = _newCol as IList;
      //    if (iList != null)
      //    {
      //      if (iList.Count != this.rawPrevGettedValueCount)
      //      {
      //        this.ReloadDropdownCollections();
      //        recreateBecauseOfListChange = true;
      //      }

      //      this.rawPrevGettedValueCount = iList.Count;
      //    }

      //    this.rawPrevGettedValue = _newCol;
      //  }

      //  if (this.inlineSelector == null || recreateBecauseOfListChange)
      //  {
      //    this.inlineSelector = this.CreateSelector();
      //    this.inlineSelector.SelectionChanged += (x) =>
      //    {
      //      this.nextResult = x;
      //    };
      //  }

      //  this.inlineSelector.OnInspectorGUI();

      //  if (this.nextResult != null)
      //  {
      //    newResult = this.nextResult;
      //    this.nextResult = null;
      //  }
      //}
      //else if (this.Attribute.AppendNextDrawer && !this.isList)
      if (Attribute.AppendNextDrawer && !isList)
      {
        GUILayout.BeginHorizontal();
        {
          var width = 15f;
          if (label != null)
          {
            width += GUIHelper.BetterLabelWidth;
          }

          newResult = GenericSelector<object>.DrawSelectorDropdown(label, GUIContent.none, ShowSelector, GUIStyle.none, GUILayoutOptions.Width(width));
          if (Event.current.type == EventType.Repaint)
          {
            var btnRect = GUILayoutUtility.GetLastRect().AlignRight(15);
            btnRect.y += 4;
            SirenixGUIStyles.PaneOptions.Draw(btnRect, GUIContent.none, 0);
          }

          GUILayout.BeginVertical();
          bool disable = Attribute.DisableGUIInAppendedDrawer;
          if (disable) GUIHelper.PushGUIEnabled(false);
          CallNextDrawer(null);
          if (disable) GUIHelper.PopGUIEnabled();
          GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
      }
      else
      {
        string valueName = GetCurrentValueName();

        if (Attribute.HideChildProperties == false && Property.Children.Count > 0)
        {
          Rect valRect;
          isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out valRect);
          newResult = GenericSelector<object>.DrawSelectorDropdown(valRect, valueName, ShowSelector);

          if (SirenixEditorGUI.BeginFadeGroup(this, isToggled.Value))
          {
            EditorGUI.indentLevel++;
            for (int i = 0; i < Property.Children.Count; i++)
            {
              var child = Property.Children[i];
              child.Draw(child.Label);
            }
            EditorGUI.indentLevel--;
          }
          SirenixEditorGUI.EndFadeGroup();
        }
        else
        {
          newResult = GenericSelector<object>.DrawSelectorDropdown(label, valueName, ShowSelector);
        }
      }

      if (newResult != null)
      {
        AddResult(newResult);
      }
    }

    void OpenSelector()
    {
      ReloadDropdownCollections();
      var rect = new Rect(Event.current.mousePosition, Vector2.zero);
      var selector = ShowSelector(rect);
      selector.SelectionConfirmed += x => result = x;
    }

    OdinSelector<object> ShowSelector(Rect rect)
    {
      var selector = CreateSelector();

      rect.x = (int)rect.x;
      rect.y = (int)rect.y;
      rect.width = (int)rect.width;
      rect.height = (int)rect.height;

      if (Attribute.AppendNextDrawer && !isList)
      {
        rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
      }

      selector.ShowInPopup(rect, new Vector2(Attribute.DropdownWidth, Attribute.DropdownHeight));
      return selector;
    }

    GenericSelector<object> CreateSelector()
    {
      var IsUniqueList = Attribute.IsUniqueList || Attribute.ExcludeExistingValuesInList;
      IEnumerable<ValueDropdownItem> query = getValues();
      var isEmpty = query.Any() == false;

      if (!isEmpty)
      {
        if (isList && Attribute.ExcludeExistingValuesInList || (isListElement && IsUniqueList))
        {
          var list = query.ToList();
          var listProperty = Property.FindParent(x => (x.ChildResolver as IOrderedCollectionResolver) != null, true);
          var comparer = new TLPIValueDropdownEqualityComparer(false);

          listProperty.ValueEntry.WeakValues.Cast<IEnumerable>()
            .SelectMany(x => x.Cast<object>())
            .ForEach(x =>
            {
              list.RemoveAll(c => comparer.Equals(c, x));
            });

          query = list;
        }
      }

      var enableSearch = query.Take(Attribute.NumberOfItemsBeforeEnablingSearch).Count() == Attribute.NumberOfItemsBeforeEnablingSearch;

      GenericSelector<object> selector = new GenericSelector<object>(Attribute.DropdownTitle, false, query.Select(x => new GenericSelectorItem<object>(x.Text, x.Value)));

      enableMultiSelect = isList && IsUniqueList && !Attribute.ExcludeExistingValuesInList;

      if (Attribute.FlattenTreeView)
      {
        selector.FlattenedTree = true;
      }

      if (isList && !Attribute.ExcludeExistingValuesInList && IsUniqueList)
      {
        selector.CheckboxToggle = true;
      }
      else if (Attribute.DoubleClickToConfirm == false && !enableMultiSelect)
      {
        selector.EnableSingleClickToSelect();
      }

      if (isList && enableMultiSelect)
      {
        selector.SelectionTree.Selection.SupportsMultiSelect = true;
        selector.DrawConfirmSelectionButton = true;
      }

      selector.SelectionTree.Config.DrawSearchToolbar = enableSearch;

      IEnumerable<object> selection = Enumerable.Empty<object>();

      if (!isList)
      {
        selection = getSelection();
      }
      else if (enableMultiSelect)
      {
        selection = getSelection().SelectMany(x => (x as IEnumerable).Cast<object>());
      }

      selector.SetSelection(selection);
      selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);

      if (Attribute.ExpandAllMenuItems)
      {
        selector.SelectionTree.EnumerateTree(x => x.Toggled = true);
      }

      if (Attribute.SortDropdownItems)
      {
        selector.SelectionTree.SortMenuItemsByName();
      }

      return selector;
    }

    string GetCurrentValueName() {
      if (!EditorGUI.showMixedValue)
      {
        var weakValue = Property.ValueEntry.WeakSmartValue;

        string name = null;
        if (nameLookup != null && weakValue != null)
        {
          nameLookup.TryGetValue(weakValue, out name);
        }

        return new GenericSelectorItem<object>(name, weakValue).GetNiceName();
      }

      return SirenixEditorGUI.MixedValueDashChar;
    }
  }
}
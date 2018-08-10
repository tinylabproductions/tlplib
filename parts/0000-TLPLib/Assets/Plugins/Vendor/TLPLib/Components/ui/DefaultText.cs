using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui.DefaultValuesText {
  /// <summary>
  /// When added, helper component adds a Text component,
  /// sets it's default values, then deletes itself in editor.
  /// If added and text component already exists, it shows a popup
  /// with values that you would override asking, if you really want to override.
  /// </summary>
  [ExecuteInEditMode, AddComponentMenu("Default-Text")]
  public class DefaultText : MonoBehaviour, IMB_Awake {
    const bool 
      defaultSupportRichText = false,
      defaultResizeTextForBestFit = false,
      defaultAlignByGeometry = true,
      defaultRaycastTarget = false;
    const TextAnchor defaultAlignment = TextAnchor.MiddleCenter;
    const HorizontalWrapMode defaultHorizontalOverflow = HorizontalWrapMode.Overflow;
    const VerticalWrapMode defaultVerticalOverflow = VerticalWrapMode.Overflow;

    public void Awake() {
      var existingText = gameObject.GetComponent<Text>();

      handleSetValues(
        existingText ? existingText : gameObject.AddComponent<Text>(),
        !existingText
      );
      
      DestroyImmediate(this);
    }
    
    void setText(Text text, bool overwrite) => 
      text.text = overwrite ? "Placeholder" : text.text;

    void handleSetValues(Text text, bool newlyCreated) {
      void set() {
        setText(text, newlyCreated);
        setDefaultValues(text);
      }

      if (newlyCreated) set();
      else {
        var differencesOpt =
          whatDiffersfromDefault(text)
          .mapVal(_ => _.Any().opt(_));
        
        var bodyMsg = 
          differencesOpt  
          .fold(
            "Nothing will be changed as everything is already at default values.",
            _ => "These settings will be overwritten:\n" + _.mkStringEnum("", "", "")
          );
        
        var response = EditorUtility.DisplayDialog(
          "Text component already exists.",
          bodyMsg,
          differencesOpt.isSome ? "Set" : "Ok"
        );

        if (differencesOpt.isSome && response) set();
      }
    }

    IEnumerable<string> whatDiffersfromDefault(Text text) {
      string s<A>(A previous, A default_, string previousName) => 
        $"{previousName}: {previous} => {default_}\n";

      if (text.supportRichText != defaultSupportRichText)
        yield return s(text.supportRichText, defaultSupportRichText, nameof(text.supportRichText));
      if (text.alignment != defaultAlignment)
        yield return s(text.alignment, defaultAlignment, nameof(text.alignment));
      if (text.alignByGeometry != defaultAlignByGeometry)
        yield return s(text.alignByGeometry, defaultAlignByGeometry, nameof(text.alignByGeometry));
      if (text.horizontalOverflow != defaultHorizontalOverflow)
        yield return s(text.horizontalOverflow, defaultHorizontalOverflow, nameof(text.horizontalOverflow));
      if (text.verticalOverflow != defaultVerticalOverflow)
        yield return s(text.verticalOverflow, defaultVerticalOverflow, nameof(text.verticalOverflow));
      if (text.resizeTextForBestFit != defaultResizeTextForBestFit)
        yield return s(text.resizeTextForBestFit, defaultResizeTextForBestFit, nameof(text.resizeTextForBestFit));
      if (text.raycastTarget != defaultRaycastTarget)
        yield return s(text.raycastTarget, defaultRaycastTarget, nameof(text.raycastTarget));
    }

    static void setDefaultValues(Text text) {
      text.supportRichText = defaultSupportRichText;
      text.alignment = defaultAlignment;
      text.alignByGeometry = defaultAlignByGeometry;
      text.horizontalOverflow = defaultHorizontalOverflow;
      text.verticalOverflow = defaultVerticalOverflow;
      text.resizeTextForBestFit = defaultResizeTextForBestFit;
      text.raycastTarget = defaultRaycastTarget;
    }
  }
}
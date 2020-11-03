using com.tinylabproductions.TLPLib.Components.Interfaces;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  /// <summary>
  /// Shows a modal editor window where a user can enter some text.
  /// </summary>
  public class EditorInputDialog : EditorWindow, IMB_OnGUI {
    string dialogTitle, submitText, cancelText, currentValue = "";
    Promise<Option<string>> promise;

    /// <summary>
    /// Show the dialog and return the future with the value.
    /// </summary>
    /// <param name="title">Title text</param>
    /// <param name="submitText">Text for button that returns Some</param>
    /// <param name="cancelText">Text for button that returns None</param>
    /// <param name="windowCenterXPosition">Position in screen percentage (0-1) of the window center along X axis.</param>
    /// <param name="windowCenterYPosition">Position in screen percentage (0-1) of the window center along Y axis.</param>
    /// <param name="width">Width of the window in screen percentage (0-1).</param>
    /// <param name="height">Height of the window in screen percentage (0-1).</param>
    /// <returns>
    /// None if user pressed <see cref="cancelText"/> and Some if user pressed <see cref="submitText"/>.
    /// </returns>
    [PublicAPI] public static Future<Option<string>> show(
      string title, string submitText = "Submit", string cancelText = "Cancel",
      float windowCenterXPosition = 0.5f, float windowCenterYPosition = 0.5f, float width = 0.8f, float height = 0.8f
    ) {
      var window = CreateInstance<EditorInputDialog>();
      var future = Future.async(out window.promise);
      window.dialogTitle = title;
      window.submitText = submitText;
      window.cancelText = cancelText;
      var resolution = Screen.currentResolution;
      var displayWidth = resolution.width;
      var displayHeight = resolution.height;
      var windowWidth = displayWidth * width;
      var windowHeight = displayHeight * height;
      var windowX = displayWidth * windowCenterXPosition - windowWidth / 2;
      var windowY = displayHeight * windowCenterYPosition - windowHeight / 2;
      window.position = new Rect(windowX, windowY, windowWidth, windowHeight);
      window.ShowPopup();
      return future;
    }

    public void OnGUI() {
      EditorGUILayout.LabelField(dialogTitle, EditorStyles.boldLabel);
      GUILayout.Space(10);
      currentValue = EditorGUILayout.TextArea(currentValue, GUILayout.ExpandHeight(true));
      GUILayout.Space(10);
      GUILayout.BeginHorizontal();
      // Promise will become null after project recompilation and then clicking that button would be no good
      // so just close the window.
      if (promise == null) Close();
      
      if (GUILayout.Button(submitText)) {
        promise.tryComplete(Some.a(currentValue));
        Close();
      }
      else if (GUILayout.Button(cancelText)) {
        promise.tryComplete(None._);
        Close();
      }
      GUILayout.EndHorizontal();
    }
  }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class TimelineEditor : EditorWindow {
    
    Timeline timeline;
    GameObject selectedGameObject;
    //private VisualTweener tweener;
    
    [MenuItem("TLP/Window/FunTween timeline")]
    public static void ShowWindow() {
      TimelineEditor window = GetWindow<TimelineEditor>(false, "VisualTween");
      window.wantsMouseMove = true;
      UnityEngine.Object.DontDestroyOnLoad(window);
    }
    void OnEventGUI(Rect rect) {

        for (int i = 0; i < 10; i++) {
          Rect rect1 = new Rect(timeline.SecondsToGUI(10) - timeline.scroll.x + rect.x - 5f, rect.y, 17,
            20);
          if (rect1.x + 6f > rect.x) {
            Color color = GUI.color;
            if (i == 5) {
              Color mColor = Color.blue;
              GUI.color = mColor;
            }

            GUI.Label(rect1, "", (GUIStyle) "Grad Up Swatch");
            GUI.color = color;
          }
        }
    }

    void OnEnable() {
      if (timeline == null) {
        timeline = new Timeline();
      }
    }


  }
}
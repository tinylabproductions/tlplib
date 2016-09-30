using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof(ObjectClone) )]
public class ObjectCloneEditor : Editor {
  GameObject objectToMoveAround;

  void OnSceneGUI() {
    var mono = target as ObjectClone;
    var currentEvent = Event.current;

    if (currentEvent.type == EventType.Layout) { HandleUtility.AddDefaultControl(0); }

    if (currentEvent.control && mono?.prefab) {
      if (objectToMoveAround == null) {
        objectToMoveAround = (GameObject)Instantiate(mono.prefab, posInWorld(currentEvent.mousePosition), Quaternion.identity);
        objectToMoveAround.transform.parent = mono.parent;
      }
    }
    else {
      if (objectToMoveAround != null) DestroyImmediate(objectToMoveAround);
    }

    if (currentEvent.type == EventType.MouseUp) {
      objectToMoveAround = null;
    }

    if (currentEvent.type == EventType.MouseMove) {
      var cursorPos = currentEvent.mousePosition;
      var worldPos = posInWorld(cursorPos);
      if (objectToMoveAround != null) {
        var tempPos = worldPos;
        tempPos.z = 0;
        objectToMoveAround.transform.position = tempPos;
      }
    }
  }

  Vector3 posInWorld(Vector3 mousePosition) { return HandleUtility.GUIPointToWorldRay(mousePosition).origin; }
}

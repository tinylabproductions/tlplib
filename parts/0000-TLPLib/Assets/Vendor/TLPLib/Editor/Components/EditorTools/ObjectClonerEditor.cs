using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  [CustomEditor(typeof(ObjectCloner))]
  public class ObjectClonerEditor : UnityEditor.Editor {
    Option<GameObject> objectToMoveAround;

    [UsedImplicitly]
    void OnSceneGUI() {
      var _target = (ObjectCloner) target;
      var currentEvent = Event.current;

      if (currentEvent.type == EventType.Layout) { HandleUtility.AddDefaultControl(0); }

      if (currentEvent.control) {
        if (objectToMoveAround.isEmpty && _target.prefab) {
          foreach (var position in visiblePosition(currentEvent, _target)) {
            var prefabType = PrefabUtility.GetPrefabType(_target.prefab);
            var isPrefab =
              prefabType == PrefabType.Prefab ||
              prefabType == PrefabType.ModelPrefab;
            var obj = (GameObject)(
              isPrefab
              ? PrefabUtility.InstantiatePrefab(_target.prefab)
              : Instantiate(_target.prefab)
            );
            obj.transform.position = position;
            obj.transform.parent = _target.gameObject.transform;
            objectToMoveAround = F.some(obj);
          }    
        }
      }
      else {
        foreach (var obj in objectToMoveAround) DestroyImmediate(obj);
        objectToMoveAround = Option<GameObject>.None;
      }

      if (currentEvent.type == EventType.MouseUp) {
        foreach (var obj in objectToMoveAround) {
          Undo.RegisterCreatedObjectUndo(obj, $"Object ({obj.name}) created");
          obj.transform.parent = _target.parent;
        }
        objectToMoveAround = Option<GameObject>.None;
      }

      if (currentEvent.type == EventType.MouseMove) {
        foreach (var obj in objectToMoveAround) {
          foreach (var position in visiblePosition(currentEvent, _target)) {
            obj.transform.position = position;
          }
        }
      } 
    }

    static Option<Vector3> visiblePosition(Event currentEvent, ObjectCloner _target) {
      var ray = posInWorld(currentEvent.mousePosition);
    
      var origin = _target.prefab.transform.position;
      var x = _target.lockedAxis == ObjectCloner.LockedAxis.X ? origin.x : origin.x - 1;
      var y = _target.lockedAxis == ObjectCloner.LockedAxis.Y ? origin.y : origin.y - 1;
      var z = _target.lockedAxis == ObjectCloner.LockedAxis.Z ? origin.z : origin.z - 1;

      var point1 = new Vector3(x, y, origin.z);
      var point2 = new Vector3(x, origin.y, z);
      var point3 = new Vector3(origin.x, y, z);

      var plane = new Plane(point1, point2, point3);

      float distance;
      plane.Raycast(ray, out distance);

      return distance > 0 
        ? F.some(ray.GetPoint(distance))
        : Option<Vector3>.None;
    }

    static Ray posInWorld(Vector3 mousePosition) { return HandleUtility.GUIPointToWorldRay(mousePosition); }
  }
}
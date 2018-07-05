using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.path {
  [CustomEditor(typeof(Vector3PathBehaviour))]
  public class Vector3PathEditor : UnityEditor.Editor {
    public const KeyCode
      xAxisLockKey = KeyCode.G,
      yAxisLockKey = KeyCode.H,
      zAxisLockKey = KeyCode.J;
      
    Vector3PathBehaviour behaviour;
    List<Vector3> points = new List<Vector3>();

    bool
      isRecalculatedToLocal,
      isPathClosed,
      lockXAxisPressed,
      lockYAxisPressed,
      lockZAxisPressed,
      pathChanged = true;

    void OnSceneGUI() {
      updateLockAxisPressedStates();
      input();
      recalculate();
      draw();

      if (GUI.changed) {
        behaviour.invalidate();
      }
    }

    void updateLockAxisPressedStates() {
      var guiEvent = Event.current;

      void update(ref bool keyIsDown, KeyCode key) {
        if (guiEvent.isKey && guiEvent.keyCode == key) {
          switch (guiEvent.type) {
            case EventType.KeyDown:
              keyIsDown = true;
              break;
            case EventType.KeyUp:
              keyIsDown = false;
              break;
          }
        }
      }

      update(ref lockXAxisPressed, xAxisLockKey);
      update(ref lockYAxisPressed, yAxisLockKey);
      update(ref lockZAxisPressed, zAxisLockKey);
    }
    
    bool xLocked => lockXAxisPressed || behaviour.lockXAxis;
    bool yLocked => lockYAxisPressed || behaviour.lockYAxis;
    bool zLocked => lockZAxisPressed || behaviour.lockZAxis;
    
    void OnEnable() {
      behaviour = (Vector3PathBehaviour) target;
      isRecalculatedToLocal = behaviour.relative;
    }
 
    void input() {
      var guiEvent = Event.current;
      var transform = behaviour.transform;
      var mousePos = getMousePos(Event.current.mousePosition, transform);
      if (behaviour.relative) mousePos = behaviour.transform.InverseTransformPoint(mousePos);
      
      //Removing nodes
      if (guiEvent.type == EventType.MouseDown && ( guiEvent.button == 1 || guiEvent.button == 0 && guiEvent.alt) ) {
        Option<int> maybeNode = nodeAtPos(mousePos);
        if (maybeNode.isSome) {
          Undo.RecordObject(behaviour, "Delete point");
          deleteNode(maybeNode.get);
        }
      }
      
      // Prepearing to draw white lines
      var secondIsLast = true;
      var closestIsFirst = false;

      var closestNodeID = getClosestNodeID(mousePos);
      var secondNodeID = Option<int>.None;

      if (guiEvent.shift && behaviour.nodes.Count != 0) {
        Handles.color = Color.white;

        if (closestNodeID.isSome) {
          var closestID = closestNodeID.get;
          secondNodeID = closestID + 1 >= behaviour.nodes.Count ? F.none_ : (closestID + 1).some();
          
          if (0 == closestID) closestIsFirst = true;

          var firstDist = Vector2.Distance(mousePos, behaviour.nodes[closestID]);
          var secondDist = secondNodeID.isSome ? F.some(Vector2.Distance(mousePos, behaviour.nodes[secondNodeID.get])) : F.none_;
          var pt = secondNodeID.isSome
            ? GetClosetPointOnLine(closestID, secondNodeID.get, mousePos, true, behaviour.nodes)
            : (Vector2) behaviour.nodes[closestID];

          // Checks if distance to nodes are the closest distance to whole path
          if (firstDist > Vector2.Distance(mousePos, pt))
            closestIsFirst = false;
          if (secondDist.isSome) {
            if (behaviour.nodes.Count - 1 != secondNodeID.get || secondDist.get > Vector2.Distance(mousePos, pt))
              secondIsLast = false; 
          }
        }
               
        //Draws line between closest node and mouse position
        if (closestNodeID.isSome && !secondIsLast || behaviour.nodes.Count == 1 ) {
          drawLine(closestNodeID.get, mousePos);
        }
        //Draws line between next to closest node and mouse position
        if (secondNodeID.isSome && !closestIsFirst) {
          drawLine(secondNodeID.get, mousePos);
        }

        SceneView.RepaintAll();
      }

      //Adding new node
      if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
        //If starting new path, and its closed - open it
        if (behaviour.nodes.Count == 0 && behaviour.closed) {
          isPathClosed = false;
          behaviour.closed = false;
        }
        
        Undo.RecordObject(behaviour, "Add node");
        if (!closestIsFirst && !secondIsLast && secondNodeID.isSome)
          behaviour.nodes.Insert(secondNodeID.get, mousePos);
        else if (closestIsFirst && secondNodeID.isSome && !secondIsLast)
          behaviour.nodes.Insert(closestNodeID.get, mousePos);
        else {
          behaviour.nodes.Add(mousePos);
        }
      }
    }
    
    void recalculate() {
      //Recalculating to world or local space
      recalculateCoordinates(behaviour.relative);

      //Closing path
      if (behaviour.nodes.Count > 2) {
        if (behaviour.closed && !isPathClosed) {
          if (behaviour.nodes[behaviour.nodes.Count - 1] != behaviour.nodes[0])
            behaviour.nodes.Add(behaviour.nodes[0]);
            isPathClosed = true;
            pathChanged = true;
        }
      }
      //Opening path
      if (!behaviour.closed && isPathClosed) {
        behaviour.nodes.RemoveAt(behaviour.nodes.Count - 1);
        isPathClosed = false;
        pathChanged = true;
      }
    }
    
    void draw() {
      if (behaviour.nodes.Count > 1) {
        Handles.color = Color.red;
        drawCurve(subdividePath());
      }

      Handles.color = Color.yellow;
      var length = behaviour.nodes.Count;
      for (var i = 0; i < length; i++)
        moveAndDrawHandles(i, length);

      SceneView.RepaintAll();
    }

    Option<int> nodeAtPos(Vector3 pos) {
      if (behaviour.nodes.isEmpty()) return F.none_;
      
      var minDist = HandleUtility.GetHandleSize(behaviour.nodes[0]) / (1 / behaviour.nodeHandleSize) / 2;
      Option<int> closestNodeIDX = F.none_; 
      for (var i = 0; i < behaviour.nodes.Count; i++) {
        var dist = Vector2.Distance(pos, behaviour.nodes[i]);
        var radius = HandleUtility.GetHandleSize(behaviour.nodes[i]);
        if (dist < minDist && dist < radius) {
          minDist = dist;
          closestNodeIDX = F.some(i);
        }
      }

      return closestNodeIDX;
    }

    void drawLine(int nodeIDX, Vector3 mousePos) {
      if (behaviour.relative) mousePos = behaviour.transform.TransformPoint(mousePos);
      Handles.DrawLine(behaviour.relative
          ? behaviour.transform.TransformPoint(behaviour.nodes[nodeIDX])
          : behaviour.nodes[nodeIDX],
        mousePos);
    }

    public Vector3 getMousePos(Vector2 aMousePos, Transform aTransform) {
      var ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(aMousePos.x, aMousePos.y, 0));
      var plane = new Plane(aTransform.TransformDirection(new Vector3(0, 0, -1)), aTransform.position);
      float dist = 0;
      var result = new Vector3(0, 0, 0);

      ray = HandleUtility.GUIPointToWorldRay(aMousePos);
      if (plane.Raycast(ray, out dist)) {
        result = ray.GetPoint(dist);
      }

      return result;
    }
    
    List<Vector3>  recalculateRelativePosition(
      List<Vector3> points, bool toLocal
    ) {
      for (var idx = 0; idx < points.Count; idx++) {
        var point = points[idx];
        points[idx] =
          toLocal
            ? behaviour.transform.InverseTransformPoint(point)
            : behaviour.transform.TransformPoint(point);
      }

      return points;
    }

    void recalculateCoordinates(bool isRelative) {
      if (!isRecalculatedToLocal == isRelative) {
        behaviour.nodes = recalculateRelativePosition(behaviour.nodes, isRelative);
        behaviour.relative = isRelative;
        isRecalculatedToLocal = isRelative;
      }

      pathChanged = true;
    }
    
    List<Vector3> transformList(IEnumerable<Vector3> nodes, bool toLocal) => 
      nodes.Select(x => toLocal
        ? behaviour.transform.InverseTransformPoint(x)
        : behaviour.transform.TransformPoint(x))
        .ToList();

    List<Vector3> subdividePath() {
      //If path is linear we don't need to subdivide it, returning the nodes
      if (behaviour.method == Vector3Path.InterpolationMethod.Linear) {
        return behaviour.relative ? transformList(behaviour.nodes, false) : behaviour.nodes;
      }

      if (pathChanged) {
        behaviour.invalidate();
        points = new List<Vector3>();
        for (float i = 0; i < behaviour.curveSubdivisions; i++) {
          points.Add(behaviour.path.evaluate(i / behaviour.curveSubdivisions, false));
        }

        points.Add(behaviour.path.evaluate(1, false)); //Adding last point
        pathChanged = false;
      }
        

      return points;
    }

    static void drawCurve(IList<Vector3> subdividedPath) {
      for (var idx = 1; idx < subdividedPath.Count; idx++) {
        Handles.DrawLine(subdividedPath[idx - 1], subdividedPath[idx]);
      }
    }

    void moveAndDrawHandles(int idx, int length) {
      
      if (idx > 0) Handles.color = Color.magenta;
      if (idx == 0) Handles.color = Color.green;
      if (idx == length - 1) Handles.color = Color.red;
      if (idx == length - 1 && behaviour.closed) Handles.color = Color.green;
      
      var currentNode = behaviour.relative
        ? behaviour.transform.TransformPoint(behaviour.nodes[idx])
        : behaviour.nodes[idx];
      //Setting handlesize
      var handleSize = HandleUtility.GetHandleSize(currentNode) / (1 / behaviour.nodeHandleSize) / 2;

      var newPos = Handles.FreeMoveHandle(currentNode, Quaternion.identity, handleSize, Vector3.zero,
        Handles.SphereHandleCap);
      if (behaviour.showDirection)
        drawDirectionHandles(currentNode, idx, handleSize);

      if (currentNode != newPos) {
        if (behaviour.relative) {
          newPos = behaviour.transform.InverseTransformPoint(newPos);
          currentNode = behaviour.transform.InverseTransformPoint(currentNode);
          
        }
        pathChanged = true;
        Undo.RecordObject(behaviour, "Move point");
        behaviour.nodes[idx] = calculateNewNodePosition(newPos, currentNode);
        //If path closed check if we are moving last node, if true move first node identicaly
        if (behaviour.closed && idx == behaviour.nodes.Count - 1) {
          behaviour.nodes[0] = calculateNewNodePosition(newPos, currentNode);
        }
        
      } 
    }

    void drawDirectionHandles(Vector3 currentNode, int idx, float size) {
      if (idx != behaviour.nodes.Count - 1) {
        Vector3 nextNode = behaviour.relative ? behaviour.transform.TransformPoint(behaviour.nodes[idx + 1]) : behaviour.nodes[idx + 1];
        Handles.ArrowHandleCap(0, currentNode, Quaternion.LookRotation(nextNode - currentNode), size * 1.5f,
          EventType.Repaint);
      }
    }

    public void deleteNode(int idx) {
      //If we remove starting point - open path
      if (behaviour.closed && (behaviour.nodes.Count - 1 == idx || idx == 0)) {
        behaviour.nodes.RemoveAt(0);
        behaviour.closed = false;
      }
      //If theres two nodes left - open path
      else if (behaviour.closed && behaviour.nodes.Count - 1 == 3) {
        behaviour.nodes.RemoveAt(idx);
        behaviour.closed = false;
      }
      else 
        behaviour.nodes.RemoveAt(idx);
    }

    Vector3 calculateNewNodePosition(Vector3 newPos, Vector3 currPos) {
      var xPos = xLocked ? currPos.x : newPos.x;
      var yPos = yLocked ? currPos.y : newPos.y;
      var zPos = zLocked ? currPos.z : newPos.z;

      return new Vector3(xPos, yPos, zPos);
    }
    
    public Option<int> getClosestNodeID(Vector2 aPoint) {
      var pathVerts = behaviour.nodes;
      if (pathVerts.Count <= 0) return Option<int>.None;
  
      var dist = float.MaxValue;
      var seg = 0;
      var count = pathVerts.Count - 1 ;
      for (var i = 0; i < count; i++) {
        var next = i == pathVerts.Count - 1 ? 0 : i + 1;
        var pt    = GetClosetPointOnLine(i, next, aPoint, true, pathVerts);
        var tDist = (aPoint - pt).SqrMagnitude();
        if (tDist < dist) {
          dist = tDist;
          seg = i;
        }
      }
      return seg.some();
    }
    
    public Vector2 GetClosetPointOnLine(int aStart, int aEnd, Vector3 aPoint, bool aClamp, List<Vector3> points) {
      var AP = aPoint - points[aStart];
      var AB = points[aEnd] - points[aStart];
      var ab2 = AB.x * AB.x + AB.y * AB.y;
      var ap_ab = AP.x * AB.x + AP.y * AB.y;
      var t = ap_ab / ab2;
      if (aClamp) {
        if (t < 0.0f) t = 0.0f;
        else if (t > 1.0f) t = 1.0f;
      }
      Vector2 Closest = points[aStart] + AB * t;
      return Closest;
    }
    
  }
}
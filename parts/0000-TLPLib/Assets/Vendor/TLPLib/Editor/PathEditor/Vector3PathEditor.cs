using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Editor.Utils;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.path {
  [CustomEditor(typeof(Vector3PathBehaviour))]
  public class Vector3PathEditor : OdinEditor, IMB_OnDisable, IMB_OnEnable {
    
    DisposableTracker dt = new DisposableTracker();

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
      lockZAxisPressed;

    void OnSceneGUI() {
      updateLockAxisPressedStates();
      input();
      recalculate();
      draw();

      if (GUI.changed) {
        behaviour.invalidate();
        refreshPath();
      }
      
      if (behaviour.transform.hasChanged) refreshPath();
    }

    void updateLockAxisPressedStates() {
      var guiEvent = Event.current;
      var isKey = guiEvent.isKey;
      var keyCode = guiEvent.keyCode;

      void update(ref bool keyIsDown, KeyCode key) {
        if (isKey && keyCode == key) {
          #pragma warning disable SwitchEnumAnalyzer
          switch (guiEvent.type) {
            case EventType.KeyDown:
              keyIsDown = true;
              break;
            case EventType.KeyUp:
              keyIsDown = false;
              break;
          }
          #pragma warning restore SwitchEnumAnalyzer
        }
      }

      update(ref lockXAxisPressed, xAxisLockKey);
      update(ref lockYAxisPressed, yAxisLockKey);
      update(ref lockZAxisPressed, zAxisLockKey);
    }
    
    bool xLocked => lockXAxisPressed || behaviour.lockXAxis;
    bool yLocked => lockYAxisPressed || behaviour.lockYAxis;
    bool zLocked => lockZAxisPressed || behaviour.lockZAxis;
    
    public void OnEnable() {
      behaviour = (Vector3PathBehaviour) target;
      behaviour.onValidate.subscribe(dt, _ => refreshPath());
      isRecalculatedToLocal = behaviour.relative;
      refreshPath();
    }

    public void OnDisable() {
      dt.Dispose();
    }
    
    Vector3 getWorldPos(Vector3 position) => 
      behaviour.relative ? behaviour.transform.TransformPoint(position) : position;
    
    Vector3 getLocalPos(Vector3 position) => 
      behaviour.relative ? behaviour.transform.InverseTransformPoint(position) : position;

    void input() {
      var guiEvent = Event.current;
      var transform = behaviour.transform;
      var mousePos = getLocalPos(EditorUtils.getMousePos(Event.current.mousePosition, transform));
      
      //Removing nodes
      if (guiEvent.type == EventType.MouseDown && (guiEvent.button == 1 || guiEvent.button == 0 && guiEvent.alt) ) {
        foreach (var node in nodeAtPos(mousePos)) {
          Undo.RecordObject(behaviour, "Delete point");
          deleteNode(node);
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
          var closestID = closestNodeID.__unsafeGetValue;
          secondNodeID = (closestID + 1 < behaviour.nodes.Count).opt(closestID + 1);
          
          if (0 == closestID) closestIsFirst = true;

          var firstDist = Vector2.Distance(mousePos, behaviour.nodes[closestID]);
          
          var pt = secondNodeID.isSome
            ? GetClosetPointOnLine(closestID, secondNodeID.__unsafeGetValue, mousePos, true, behaviour.nodes)
            : (Vector2) behaviour.nodes[closestID];

          // Checks if distance to nodes are the closest distance to whole path
          if (firstDist > Vector2.Distance(mousePos, pt))
            closestIsFirst = false;
          
          foreach (var nodeID in secondNodeID) {
            var secondDist = Vector2.Distance(mousePos, behaviour.nodes[nodeID]);
            if (behaviour.nodes.Count - 1 != nodeID || secondDist > Vector2.Distance(mousePos, pt))
              secondIsLast = false;
            
            if (!closestIsFirst) drawLine(nodeID, mousePos);
          }
          
          //Draws line between closest node and mouse position
          if (!secondIsLast || behaviour.nodes.Count == 1) {
            drawLine(closestID, mousePos);
          }
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
        if (!secondIsLast && secondNodeID.isSome) 
          behaviour.nodes.Insert(closestIsFirst ? closestNodeID.get : secondNodeID.get, mousePos);
        else 
          behaviour.nodes.Add(mousePos);
        
        refreshPath();
      }
    }
    
    void recalculate() {
      //Recalculating to world or local space
      recalculateCoordinates();

      //Closing path
      if (behaviour.nodes.Count > 2) {
        if (behaviour.closed && !isPathClosed) {
          var firstNode = behaviour.nodes[0];
          if (behaviour.nodes[behaviour.nodes.Count - 1] != firstNode) 
            behaviour.nodes.Add(firstNode);
          
          isPathClosed = true;
          refreshPath();
        }
      }
      //Opening path
      if (!behaviour.closed && isPathClosed) {
        behaviour.nodes.RemoveAt(behaviour.nodes.Count - 1);
        isPathClosed = false;
        refreshPath();
      }
    }
    
    void draw() {
      if (behaviour.nodes.Count > 1) {
        Handles.color = Color.red;
        drawCurve();
      }

      Handles.color = Color.yellow;
      var length = behaviour.nodes.Count;
      for (var i = 0; i < length; i++)
        moveAndDrawHandles(i, length);

      SceneView.RepaintAll();
    }

    Option<int> nodeAtPos(Vector3 pos) {
      if (behaviour.nodes.isEmpty()) return F.none_;
      
      var minDist = HandleUtility.GetHandleSize(behaviour.nodes[0]) * behaviour.nodeHandleSize;
      var closestNodeIDX = Option<int>.None; 
      
      for (var i = 0; i < behaviour.nodes.Count; i++) {
        var node = behaviour.nodes[i];
        var dist = Vector2.Distance(pos, node);
        var radius = HandleUtility.GetHandleSize(node);
        if (dist < minDist && dist < radius) {
          minDist = dist;
          closestNodeIDX = F.some(i);
        }
      }

      return closestNodeIDX;
    }

    void drawLine(int nodeIDX, Vector3 mousePos) => 
      Handles.DrawLine(getWorldPos(behaviour.nodes[nodeIDX]), getWorldPos(mousePos));
    
    List<Vector3> recalculateRelativePosition(List<Vector3> nodes, bool toLocal) {
      for (var idx = 0; idx < nodes.Count; idx++) {
        var point = nodes[idx];
        nodes[idx] =
          toLocal
            ? behaviour.transform.InverseTransformPoint(point)
            : behaviour.transform.TransformPoint(point);
      }

      return nodes;
    }

    void recalculateCoordinates() {
      var isRelative = behaviour.relative;
      if (!isRecalculatedToLocal == isRelative) {
        behaviour.nodes = recalculateRelativePosition(behaviour.nodes, isRelative);
        behaviour.relative = isRelative;
        isRecalculatedToLocal = isRelative;
        refreshPath();
      }
    }
    
    List<Vector3> transformList(IEnumerable<Vector3> nodes, bool toLocal) => 
      nodes.Select(x => toLocal
        ? behaviour.transform.InverseTransformPoint(x)
        : behaviour.transform.TransformPoint(x)
      ).ToList();
   
    void refreshPath() {
      if (behaviour.method == Vector3Path.InterpolationMethod.Linear) {
        points = behaviour.relative ? transformList(behaviour.nodes, false) : behaviour.nodes;
      }
      else {
        behaviour.invalidate();
        points = new List<Vector3>();
        for (float i = 0; i < behaviour.curveSubdivisions; i++) {
          points.Add(behaviour.path.evaluate(i / behaviour.curveSubdivisions, false));
        }

        points.Add(behaviour.path.evaluate(1, false)); //Adding last point
      }
    }

    void drawCurve() {
      for (var idx = 1; idx < points.Count; idx++) {
        Handles.DrawLine(points[idx - 1], points[idx]);
      }
    }
    
    void moveAndDrawHandles(int idx, int length) {
      Color getHandleColor() {
        if (idx == 0 || idx == length - 1 && behaviour.closed) return Color.green;
        if (idx == length - 1) return Color.red;
        return Color.magenta;
      }

      Handles.color = getHandleColor();

      var currentNode = getWorldPos(behaviour.nodes[idx]);
      //Setting handlesize
      var handleSize = HandleUtility.GetHandleSize(currentNode) * behaviour.nodeHandleSize;

      var newPos = Handles.FreeMoveHandle(
        currentNode, Quaternion.identity, handleSize, Vector3.zero, Handles.SphereHandleCap
      );
      if (behaviour.showDirection)
        drawArrows(currentNode, idx, handleSize * 1.5f);

      if (currentNode != newPos) {
        refreshPath();
        Undo.RecordObject(behaviour, "Move point");
        var calculatedNode = calculateNewNodePosition(getLocalPos(newPos), behaviour.nodes[idx]);
        behaviour.nodes[idx] = calculatedNode;
        //If path closed check if we are moving last node, if true move first node identicaly
        if (behaviour.closed && idx == behaviour.nodes.Count - 1) {
          behaviour.nodes[0] = calculatedNode;
        }
      } 
    }

    void drawArrows(Vector3 currentNode, int idx, float size) {
      if (idx != behaviour.nodes.Count - 1) {
        var nextNode = getWorldPos(behaviour.nodes[idx + 1]);
        Handles.ArrowHandleCap(
          0, currentNode, Quaternion.LookRotation(nextNode - currentNode), size, EventType.Repaint
        );
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
      var Closest = points[aStart] + AB * t;
      return Closest;
    }
    
  }
}
#if ADV_INS_CHANGES
using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using UnityEditor;
using UnityEngine;
using SnapType = com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline.TimelineEditor.SnapType;
using AnimationPlaybackEvent = com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline.TweenPlaybackController.AnimationPlaybackEvent;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineVisuals {
    
    public TimelineVisuals(
      SettingsGUICallback settingsGuiCallback, PlaybackControllerCallback playbackControllerCallback,
      TimelineCursorLineCallback cursorLineCallback, NodeEventsCallback nodeEventsCallback,
      Val<bool> visualizaitonMode
    ) {
      visualizationMode = visualizaitonMode;
      onSettingsGUI = settingsGuiCallback;
      onPlaybackControllerButton = playbackControllerCallback;
      onTimelineCursorLine = cursorLineCallback;
      onNodeEvent = nodeEventsCallback;
      
      toStartButtonTexture = EditorGUIUtility.FindTexture("d_beginButton");
      startButtonTexture = EditorGUIUtility.FindTexture("d_StepButton");
      playButtonTexture = EditorGUIUtility.FindTexture("d_PlayButton");
      pauseButtonTexture = EditorGUIUtility.FindTexture("d_PauseButton");
      playFromEndButtonTexture = EditorGUIUtility.FindTexture("d_StepLeftButton");
      toEndButtonTexture = EditorGUIUtility.FindTexture("d_endButton");
      reverseButtonTexture = EditorGUIUtility.FindTexture("d_playLoopOff");
      exitButtonTexture = EditorGUIUtility.FindTexture("P4_DeletedLocal");
    }
    
    public delegate void SettingsGUICallback(float width, bool isVisualiisation);
    public delegate void TimelineCursorLineCallback(bool isStart, float time = 0f);
    public delegate void PlaybackControllerCallback(AnimationPlaybackEvent animationEvent);

    public delegate void NodeEventsCallback(TimelineEditor.NodeEvents nodeEvent, TimelineNode node);

    readonly SettingsGUICallback onSettingsGUI;
    readonly PlaybackControllerCallback onPlaybackControllerButton;
    readonly TimelineCursorLineCallback onTimelineCursorLine;
    readonly NodeEventsCallback onNodeEvent;

    readonly Texture toStartButtonTexture,
      startButtonTexture,
      playButtonTexture,
      pauseButtonTexture,
      playFromEndButtonTexture,
      toEndButtonTexture,
      reverseButtonTexture,
      exitButtonTexture;

    readonly GUIStyle toolbarStyle;
    
    [Record]
    partial struct CallbackVisuals {
      public readonly Rect iconRect;
      public readonly Rect labelRect;
      public readonly GUIContent labelContent;
  
      public CallbackVisuals (Rect boxRect, GUIContent labelContent) {
        iconRect = new Rect(boxRect.x - 10, boxRect.y, 20, boxRect.height);
        labelRect = new Rect(boxRect.x - 30, boxRect.y, 60, boxRect.height);
        this.labelContent = labelContent;
      }
    }
    
    readonly List<CallbackVisuals> callbackVisualsList = new List<CallbackVisuals>();
  
    Rect blackBarRect;
    const float ZOOM = 20;
    const int OUTLINE_WIDTH = 3;
    static readonly int[] timeFactor = {1,5,10,30,60,300,600,1800,3600,7200};
    
    int timeIndexFactor = 1;
    Rect timeRect;
    readonly Val<bool> visualizationMode;
    Vector2 _scroll;
    
    [PublicAccessor] Rect _timelineRect;

    float currentTime {
      get => GUIToSeconds(timePosition - timelineOffset);
      set => timePosition = secondsToGUI (value) + timelineOffset;
    }

    Vector2 expandView;
    bool changeTime, changeOffset, applicationPlaying, playingBackwards;
    float timePosition, clickOffset, timeZoomFactor = 1, lastNodeTime, timelineOffset = 450;
  
    Option<EditorApplication.CallbackFunction> updateDelegateOpt;
  
    public void doTimeline(Rect position, Option<FunTweenManager> funTweenManager, List<TimelineNode> funNodes) {
      applicationPlaying = Application.isPlaying;
      _timelineRect = position;
      timeRect = new Rect (position.x + timelineOffset, position.y, position.width - 15, 20);
      blackBarRect = new Rect (position.x + timelineOffset - 1, position.y + 19, position.width, 16);
  
      if (funTweenManager.valueOut(out var ftm) && (visualizationMode.value || applicationPlaying)) {
        currentTime = ftm.timeline.timeline.timePassed;
      }
      
      doCursor(funNodes);
      doToolbarGUI(position, funTweenManager);
      drawTicksGUI();
      //doTimelineEvents(funNodes);
    }

    public void endScrollView() => GUI.EndScrollView();

    public Vector2 startScrollView() =>
      _scroll = GUI.BeginScrollView(
        new Rect(_timelineRect.x + timelineOffset,
          timeRect.height + blackBarRect.height,
          _timelineRect.width - timelineOffset,
          _timelineRect.height - timeRect.height - blackBarRect.height),
        _scroll,
        new Rect(0, 0,
          _timelineRect.width + lastNodeTime + expandView.x - timelineOffset,
          _timelineRect.height + 400 + expandView.y),
        true, true
      );
    
    
    public void drawNodes(
      List<TimelineNode> funNodes, List<TimelineNode> selectedNodes, Option<TimelineNode> root,
      Option<TimelineEditor.NodeSnappedTo> nodeSnappedToOpt
      ) {
      GUI.enabled = !visualizationMode.value;
  
      Option<Rect> snapIndicatorOpt = F.none_;
      callbackVisualsList.Clear();
      var indicatorColor = Color.green;
  
      foreach (var currNode in funNodes) {
        if (currNode.element.element != null) {
          if (!currNode.isCallback) {
            EditorGUIUtility.AddCursorRect(
              new Rect(secondsToGUI(currNode.startTime) - 5, currNode.channel * 20, 10, 20),
              MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(
              new Rect(secondsToGUI(currNode.startTime + currNode.duration) - 5, currNode.channel * 20, 10, 20),
              MouseCursor.ResizeHorizontal);
          }
          EditorGUIUtility.AddCursorRect(
            new Rect(
              secondsToGUI(currNode.startTime - (currNode.isCallback ? 0.5f : 0)),
              currNode.channel * 20,
              secondsToGUI(currNode.isCallback ? 1 : currNode.duration), 20
            ),
            MouseCursor.Pan);
  
          var boxRect = new Rect(secondsToGUI(currNode.startTime), currNode.channel * 20,
            secondsToGUI(currNode.duration), 20);
  
          var selectedCurrentNode = selectedNodes.find(selected => selected == currNode);
  
          var iconRect = new Rect(boxRect.x - 10, boxRect.y, 20, boxRect.height);
          var tooltip = new GUIContent(EditorGUIUtility.FindTexture("tranp"), currNode.name);
  
          void drawOutline(Rect aroundRect, Color outlineColor) {
              
            if (!currNode.isCallback) {
              EditorGUI.DrawRect(aroundRect, outlineColor);
              GUI.Box(new Rect(
                aroundRect.x + OUTLINE_WIDTH,
                aroundRect.y + OUTLINE_WIDTH,
                aroundRect.width - OUTLINE_WIDTH * 2,
                aroundRect.height - OUTLINE_WIDTH * 2
              ), "", "TL LogicBar 0");
            }
            else {
              EditorGUI.DrawRect(iconRect, outlineColor);
              drawCallbackIcon(new CallbackVisuals(boxRect, tooltip));
            }
          }
  
          if (selectedCurrentNode.valueOut(out var selectedNode)) {
            drawOutline(boxRect, Color.magenta);

            bool isSnappedToRootStart(TimelineEditor.NodeSnappedTo nodeSnappedTo) =>
              nodeSnappedTo.snapType == SnapType.StartWithEnd
              || nodeSnappedTo.snapType == SnapType.StartWithStart;
            
            if ( root.valueOut(out var rootNode) && rootNode == currNode ) {
              if (nodeSnappedToOpt.valueOut(out var nodeSnappedTo)) {
                var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
                var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * 20;
                snapIndicatorOpt =
                  selectedIsHigher
                    ? getIndicatorRect(selectedNode, isSnappedToRootStart(nodeSnappedTo), distance).some()
                    : getIndicatorRect(nodeSnappedTo.node,
                      nodeSnappedTo.snapType == SnapType.EndWithStart
                      || nodeSnappedTo.snapType == SnapType.StartWithStart,
                      distance
                    ).some();
  
                Rect getIndicatorRect(TimelineNode nawd, bool isSnappedToStart, float dist) =>
                  new Rect(secondsToGUI(
                      isSnappedToStart
                        ? nawd.startTime
                        : nawd.getEnd()),
                    nawd.channel * 20 - 10, 2, dist
                  );
              }
  
              indicatorColor = isSnappedToRootStart(nodeSnappedTo) ? Color.yellow : Color.cyan;
            }
          }
          else if (!currNode.isCallback) {
            GUI.Box(boxRect, "", "TL LogicBar 0");
          }
          
          if (currNode.isCallback) {
            callbackVisualsList.Add(new CallbackVisuals(boxRect, tooltip));
          }
  
          if (currNode.linkedNode.valueOut(out var linkedNode)) {
            drawOutline(boxRect, Color.green);
            EditorGUI.DrawRect(
              new Rect(secondsToGUI(linkedNode.getEnd()),
                currNode.channel * 20 + 10, secondsToGUI(currNode.startTime - linkedNode.getEnd()), 2),
              Color.green
            );
            EditorGUI.DrawRect(
              new Rect(secondsToGUI(linkedNode.getEnd()),
                currNode.channel * 20, 3, 20),
              Color.green
            );
          }
          
          if ( (currNode.element.element.getTargets().Length == 0
              || currNode.element.element.getTargets().Any(target => target == null)) && !currNode.isCallback
          ) {
            drawOutline(boxRect, Color.red);
          }
  
          var style = new GUIStyle("Label");
          style.fontSize = selectedCurrentNode.isSome ? 12 : style.fontSize;
          style.fontStyle = FontStyle.Bold;
          var color = selectedCurrentNode.isSome ? Color.magenta : currNode.nodeTextColor;
          color.a = selectedCurrentNode.isSome ? 1.0f : 0.7f;
          style.normal.textColor = color;
          Vector3 size = style.CalcSize(new GUIContent($"content: {currNode.name}"));
          var labelPosX = Mathf.Clamp(boxRect.x + boxRect.width / 2 - size.x / 3 , boxRect.x, boxRect.x + boxRect.width);
          var rect1 = new Rect(labelPosX,
            boxRect.y + boxRect.height * 0.5f - size.y * 0.5f, Mathf.Clamp(size.x, 0, boxRect.width + (boxRect.x - labelPosX)), size.y);
  
          GUI.Label(rect1, $"{currNode.name}", style);
        }
  
        if (snapIndicatorOpt.valueOut(out var snapIndicator)) {
          EditorGUI.DrawRect(snapIndicator, indicatorColor);
        }
  
        foreach (var callbackVisual in callbackVisualsList) {
          drawCallbackIcon(callbackVisual);
        }
        
        void drawCallbackIcon(CallbackVisuals visuals) {
          GUI.color = Color.yellow;
          GUI.DrawTexture(visuals.iconRect,
            EditorGUIUtility.FindTexture("d_animationkeyframe"));
          GUI.Label(visuals.labelRect, visuals.labelContent);
          GUI.color = Color.white;
        }
      }
    }
  
    public void doLines(){
      Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
      for (var y = 0; y < (int)_timelineRect.height + _scroll.y; y += 20) {
        Handles.DrawLine(new Vector3(0, y, 0), new Vector3(_timelineRect.width+_scroll.x, y, 0));	
      }
      Handles.color = Color.white;
    }
    
    public void doTimelineGUI(){
      if ((changeTime || Application.isPlaying || visualizationMode.value)
        && timePosition - _scroll.x >= timelineOffset && timePosition - _scroll.x < _timelineRect.width - 15) {
        var color = Color.red;
        color.a = Application.isPlaying ? 0.6f : 1.0f;
        Handles.color = color;
        
        var style = new GUIStyle("Label") {
          fontSize = 12,
          fontStyle = FontStyle.Bold,
          normal = {textColor = color}
        };
        Vector3 size = style.CalcSize(new GUIContent($"content: {currentTime:F2}s"));
        var rect1 = new Rect(timePosition - _scroll.x, 19, size.x, size.y);
        GUI.Label(rect1, $"{currentTime:F2}s", style);
        Handles.DrawLine(new Vector3(timePosition - _scroll.x, 0, 0),
          new Vector3(timePosition - _scroll.x, _timelineRect.height - 15, 0)
        );
        Handles.color = Color.white;
      }
    }
    
    
    void doCursor(List<TimelineNode> funNodes){
      if (funNodes.find(node =>
        new Rect(timelineOffset - 5, (node.channel + 2) * 20, 20, 20).Contains(Event.current.mousePosition)
        && node.startTime < 1f).isNone
      ) {
        EditorGUIUtility.AddCursorRect(new Rect(timelineOffset - 5, 37, 10, _timelineRect.height),
          MouseCursor.ResizeHorizontal);
      }
    }
    
    public void doBlackBar(){
      if (Event.current.type == EventType.Repaint) {
        ((GUIStyle)"AnimationEventBackground").Draw(blackBarRect, GUIContent.none, 0);
      }
    }
    
    void doEvents(List<TimelineNode> funNodes) {
        var currEvent = Event.current;
/*        var snappingEnabled = !Event.current.shift && snapping;
        if (currEvent.control && currEvent.keyCode == KeyCode.A && currEvent.type == EventType.keyUp) {
          selectedNodesList.Clear();
          foreach (var node in funNodes) {
            selectedNodesList.Add(node);
          }
        }

        if (currEvent.keyCode == KeyCode.Delete && currEvent.type == EventType.keyDown) {
          removeoAllSelectedNodes();
          selectedNodesList.Clear();
          importTimeline();
        }*/

        switch (currEvent.rawType) {
          case EventType.MouseDown:

            foreach (var node in funNodes) {
              if (new Rect(secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20)
                  .Contains(Event.current.mousePosition) && !node.isCallback
              ) {
                onNodeEvent(TimelineEditor.NodeEvents.ResizeStart, node);
                
                EditorGUI.FocusTextInControl("");
                currEvent.Use();
              }

              if (new Rect(secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20)
                .Contains(Event.current.mousePosition) && !node.isCallback
              ) {
                onNodeEvent(TimelineEditor.NodeEvents.ResizeEnd, node);
                
                EditorGUI.FocusTextInControl("");
                currEvent.Use();
              }

              if (new Rect(
                secondsToGUI(node.startTime - (node.isCallback ? 0.5f : 0)),
                node.channel * 20,
                secondsToGUI(node.isCallback ? 1 : node.duration), 20
              ).Contains(Event.current.mousePosition)) {

                switch (currEvent.button) {
                  case 0:
                    
                    onNodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB1, node);

                    EditorGUI.FocusTextInControl("");
                    break;
                  case 1:
                    onNodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB2, node);
                    break;
                }

                currEvent.Use();
              }
            }
            break;
          
          case EventType.MouseDrag:
            break;

          case EventType.MouseUp:
            break;

          default: break;
        }
      }
    
    public void doToolbarGUI(Rect position, Option<FunTweenManager> funTweenManager){
      
      GUILayout.BeginArea (new Rect (position.x, position.y, timelineOffset, position.height), GUIContent.none);
      
      GUILayout.BeginHorizontal (EditorStyles.toolbar);

      GUI.enabled = !EditorApplication.isCompiling;
        
      if (GUILayout.Button(toStartButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.GoToStart);
      }

      GUI.backgroundColor = new Color(0, 0.8f, 1, 0.5f);
      if (GUILayout.Button(startButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromStart);
      }
      GUI.backgroundColor = Color.white;

      if (GUILayout.Button(playButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromCurrentTime);
      }

      if (GUILayout.Button(pauseButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.Pause);
      }

      GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
      if (GUILayout.Button(playFromEndButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromEnd);
      }

      GUI.backgroundColor = Color.white;

      if (GUILayout.Button(toEndButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.GoToEnd);
      }

      GUILayout.Space(10f);

      if (GUILayout.Button(reverseButtonTexture, EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.Reverse);
      }

      GUILayout.FlexibleSpace();

      if (!Application.isPlaying) {
        GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
        if (GUILayout.Button(exitButtonTexture, EditorStyles.toolbarButton)) {
          onPlaybackControllerButton(AnimationPlaybackEvent.Exit);
          return;
        }
      }
  
      GUI.backgroundColor = Color.white;
      GUILayout.EndHorizontal ();
      
      GUILayout.BeginHorizontal ();
      GUILayout.BeginVertical ();
      onSettingsGUI(timelineOffset - 1.5f, visualizationMode.value);			
      GUILayout.EndVertical ();
      GUILayout.Space (1.5f);
      GUILayout.EndHorizontal ();
      GUILayout.EndArea ();
    }
    
    void drawTicksGUI(){
      if (Event.current.type == EventType.Repaint) {
        EditorStyles.toolbar.Draw (timeRect, GUIContent.none, 0);
      }
      Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
      var count = 0;
      for (var x = timeRect.x - _scroll.x; x < timeRect.width; x += ZOOM * timeZoomFactor) {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        if(x >= timelineOffset){
          if(count % 5 == 0){ 
            Handles.DrawLine(new Vector3(x, 7, 0), new Vector3(x, 17, 0));
            var displayMinutes = Mathf.FloorToInt(count / 5.0f * timeFactor[timeIndexFactor] / 60.0f);
            var	displaySeconds = Mathf.FloorToInt(count / 5.0f * timeFactor[timeIndexFactor] % 60.0f);
            var content = new GUIContent($"{displayMinutes:0}:{displaySeconds:00}");
            var size = ((GUIStyle)"Label").CalcSize(content);
            size.x = Mathf.Clamp(size.x, 0.0f, timeRect.width - x);
            GUI.Label(new Rect(x, -2, size.x, size.y), content);
          } else {
            Handles.DrawLine(new Vector3(x, 13, 0), new Vector3(x, 17, 0));
          }
        }
        count++;
      }
      Handles.color = Color.white;
    }
  
    public void doTimelineEvents(List<TimelineNode> funNodes){
      
      if (!GUI.enabled && !visualizationMode.value) {
        return;
      }
      var ev = Event.current;

      switch (ev.rawType) {
      case EventType.MouseDown:
        foreach (var node in funNodes) {

          if (new Rect(secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20)
              .Contains(Event.current.mousePosition) && !node.isCallback
          ) {
            onNodeEvent(TimelineEditor.NodeEvents.ResizeStart, node);
                
            EditorGUI.FocusTextInControl("");
            ev.Use();
          }

          if (new Rect(secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20)
              .Contains(Event.current.mousePosition) && !node.isCallback
          ) {
            onNodeEvent(TimelineEditor.NodeEvents.ResizeEnd, node);
                
            EditorGUI.FocusTextInControl("");
            ev.Use();
          }

          if (new Rect(
            secondsToGUI(node.startTime - (node.isCallback ? 0.5f : 0)),
            node.channel * 20,
            secondsToGUI(node.isCallback ? 1 : node.duration), 20
          ).Contains(Event.current.mousePosition)) {

            switch (ev.button) {
              case 0:
                    
                onNodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB1, node);

                EditorGUI.FocusTextInControl("");
                break;
              case 1:
                onNodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB2, node);
                break;
            }

            ev.Use();
          }
        }
        
        if (new Rect(timelineOffset - 5, 37, 10, _timelineRect.height).Contains(ev.mousePosition)
          && funNodes.find(node => 
            new Rect(timelineOffset - 5, (node.channel + 2) * 20, 20, 20).Contains(ev.mousePosition)
            && node.startTime < 1f ).isNone
        ) {
          changeOffset = true;
          clickOffset = timePosition - timelineOffset;
        }
        
        if (new Rect (timelineOffset, 0, _timelineRect.width, 37).Contains(Event.current.mousePosition)
          && Event.current.button == 0
        ) {
          timePosition = Event.current.mousePosition.x + _scroll.x;
          changeTime = true;

          onTimelineCursorLine(true, currentTime);
  
          ev.Use();
        }
        
        break;
      
      case EventType.MouseUp:
        if (changeTime) {
          onTimelineCursorLine(false);
        }
        changeTime = false;
        changeOffset = false;
  
        break;
      
      case EventType.MouseDrag:
        if (changeTime && Event.current.button == 0) {
          timePosition = Event.current.mousePosition.x + _scroll.x;
  
          onTimelineCursorLine(true, currentTime);
        }
        switch (ev.button) {
        case 0:
          if (changeOffset) {
            timelineOffset = ev.mousePosition.x;
            timelineOffset = Mathf.Clamp(timelineOffset, 170, _timelineRect.width - 170);
            timePosition = timelineOffset + clickOffset;
            timePosition = Mathf.Clamp(timePosition, timelineOffset, Mathf.Infinity);
            ev.Use();
          }
          break;
        case 1:
          break;
        case 2:
          _scroll -= ev.delta;
          _scroll.x = Mathf.Clamp(_scroll.x, 0f, Mathf.Infinity);
          _scroll.y = Mathf.Clamp(_scroll.y, 0f, Mathf.Infinity);
          
          expandView -= ev.delta;
          expandView.x = Mathf.Clamp(expandView.x, 20f, Mathf.Infinity);
          expandView.y = Mathf.Clamp(expandView.y, 20f, Mathf.Infinity);
          ev.Use();
          break;
        }
        break;
      case EventType.ScrollWheel:
        var f = timeZoomFactor;
        if (timeIndexFactor == timeFactor.Length - 1 && f < 0.5f) {
          f = 0.5f;
        } else {
          f += ev.delta.y / 100;
        }
        if (f < 0.5f && timeIndexFactor < timeFactor.Length - 1) {
          timeIndexFactor++;
          f = 1;
        }
        if (f > 1.5f && timeIndexFactor > 0) {
          timeIndexFactor--;
          f = 1;
        }

        recalculateTimelineWidth(funNodes.some());
        timeZoomFactor = f;
        
        ev.Use ();
        break;
      default: break;
      }
    }
  
    public float secondsToGUI(float seconds) => 
      seconds / timeFactor[timeIndexFactor] * ZOOM * timeZoomFactor * 5.0f;

    public float GUIToSeconds(float xCoord){
      var guiSecond = ZOOM * timeZoomFactor * 5.0f / timeFactor[timeIndexFactor];
      return xCoord / guiSecond;
    }
    
    public void recalculateTimelineWidth(Option<List<TimelineNode>> funNodes) => lastNodeTime = secondsToGUI(
      funNodes.fold(
        () => 0,
        nodes => nodes.Max(node => node.getEnd())
      )
    );
  }
}
#endif

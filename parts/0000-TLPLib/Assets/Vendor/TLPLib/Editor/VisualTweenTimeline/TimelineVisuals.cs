#if UNITY_EDITOR
#pragma warning disable SwitchEnumAnalyzer
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pzd.lib.typeclasses;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using pzd.lib.data;
using pzd.lib.functional;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using SnapType = com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline.TimelineEditor.SnapType;
using AnimationPlaybackEvent = com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline.TweenPlaybackController.AnimationPlaybackEvent;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineVisuals {
    
    [Record]
    partial struct CallbackVisuals {
      public readonly Rect iconRect, labelRect;
      public readonly GUIContent labelContent;
  
      public CallbackVisuals (Rect boxRect, GUIContent labelContent) {
        iconRect = new Rect(boxRect.x - CHANNEL_HEIGHT * .5f, boxRect.y, CHANNEL_HEIGHT, boxRect.height);
        labelRect = new Rect(boxRect.x - 30, boxRect.y, 60, boxRect.height);
        this.labelContent = labelContent;
      }
    }
    
    [Record]
    public partial struct TimelineVisualsSettings {
      public int timeIndexFactor, selectedFTMindex;
      public Vector2 scroll;
      public float timeZoomFactor, timelineOffset, lastNodeTime;

      public TimelineVisualsSettings(int selectedFTMindex) {
        timeIndexFactor = 0;
        timeZoomFactor = 0; // temporary value that will be replaced on first update
        timelineOffset = 450;
        lastNodeTime = 0;
        this.selectedFTMindex = selectedFTMindex;
        scroll = Vector2.zero;
      }
    }

    const float ZOOM = 100;
    public const float CHANNEL_HEIGHT = 20;
    
    const int OUTLINE_WIDTH = 3;
    static readonly int[] timeFactor = {1,2,3,4,5,10,30,60,300,600,1800,3600,7200};
    public delegate void TimelineCursorLineCallback(bool isStart, float time = 0f);
    public delegate void PlaybackControllerCallback(AnimationPlaybackEvent animationEvent);
    public delegate void SettingsEventsCallback(TimelineEditor.SettingsEvents settingsEvent);
    public delegate void FTMSelectedCallback(int index);
    public delegate void LockButtonCallback();
    public delegate void NodeEventsCallback(
      TimelineEditor.NodeEvents nodeEvent, Option<TimelineNode> node, float guiToSeconds,
      int mousePositionChannel
    );

    readonly PlaybackControllerCallback onPlaybackControllerButton;
    readonly TimelineCursorLineCallback onTimelineCursorLine;
    readonly NodeEventsCallback onNodeEvent;
    readonly SettingsEventsCallback onNewSettings;
    readonly LockButtonCallback onLockButton;
    readonly FTMSelectedCallback onFTMselectionChange;
    readonly Texture toStartButtonTexture,
      startButtonTexture,
      playButtonTexture,
      pauseButtonTexture,
      playFromEndButtonTexture,
      toEndButtonTexture,
      reverseButtonTexture,
      exitButtonTexture,
      lockTexture,
      lockOnTexture;

    readonly GUIStyle toolbarStyle;
    readonly string[] ftmsLabels;
    readonly List<CallbackVisuals> callbackVisualsList = new();
    readonly Val<bool> visualizationMode, isLocked;
    
    [PublicAccessor] TimelineVisualsSettings _visualsSettings;

    Rect timeRect, timelineRect, blackBarRect;
    Vector2 expandView, settingsScroll;
    
    /// <summary>
    /// Currently time bar has active position. User sees current time.
    /// </summary>
    bool changeTime;
    
    bool changeOffset;
    bool applicationPlaying;
    bool playingBackwards;
    bool isDifferentFTMselected;
    float timePosition, clickOffset;
    
    PropertyTree maybeTree;
    InspectorProperty maybeProperty;
    Vector2 elementPreviewScrollPosition;
    
    public TimelineVisuals(
      PlaybackControllerCallback playbackControllerCallback, LockButtonCallback onLockButton,
      TimelineCursorLineCallback cursorLineCallback, NodeEventsCallback nodeEventsCallback,
      SettingsEventsCallback settingsEventsCallback, FTMSelectedCallback FTMSelectedCallback,
      Val<bool> visualizationMode, Val<bool> isLocked, ImmutableArray<FunTweenManagerV2> ftms,
      TimelineVisualsSettings visualsSettings
    ) {
      this.visualizationMode = visualizationMode;
      onPlaybackControllerButton = playbackControllerCallback;
      onTimelineCursorLine = cursorLineCallback;
      onNodeEvent = nodeEventsCallback;
      onNewSettings = settingsEventsCallback;
      onFTMselectionChange = FTMSelectedCallback;
      this.isLocked = isLocked;
      this.onLockButton = onLockButton;
      _visualsSettings = visualsSettings;
      ftmsLabels = ftms.Select((ftm, idx) => $"{idx}: {ftm.title}").ToArray();

      toStartButtonTexture = EditorGUIUtility.FindTexture("d_beginButton");
      startButtonTexture = EditorGUIUtility.FindTexture("d_StepButton");
      playButtonTexture = EditorGUIUtility.FindTexture("d_PlayButton");
      pauseButtonTexture = EditorGUIUtility.FindTexture("d_PauseButton");
      playFromEndButtonTexture = EditorGUIUtility.FindTexture("d_StepLeftButton");
      toEndButtonTexture = EditorGUIUtility.FindTexture("d_endButton");
      reverseButtonTexture = EditorGUIUtility.FindTexture("d_playLoopOff");
      exitButtonTexture = EditorGUIUtility.FindTexture("P4_DeletedLocal");
      lockTexture = EditorGUIUtility.FindTexture("LockIcon");
      lockOnTexture = EditorGUIUtility.FindTexture("LockIcon-On");
    }

    void initializeZoomFactor(float initialDuration) {
      var timelineEditorWidth = timelineZone.width;

      var contentDuration =
        Math.Max(0, initialDuration)
        + 1f;        // Keep a little space at the end of the timeline.
      
      var calculatedZoomFactor = timelineEditorWidth / ZOOM / contentDuration;
      _visualsSettings.timeZoomFactor = Math.Min(4f, calculatedZoomFactor);
    }

    float currentTime {
      get => GUIToSeconds(timePosition - _visualsSettings.timelineOffset);
      set => timePosition = secondsToGUI(value) + _visualsSettings.timelineOffset;
    }

    public void doTimeline(Rect position, Option<FunTweenManagerV2> funTweenManager, List<TimelineNode> funNodes,
      List<TimelineNode> selectedNodesList, bool snapping, Option<TimelineNode> rootNode,
      Option<TimelineEditor.NodeSnappedTo> nodeSnappedToOpt
    ) {
      var duration = funTweenManager.map(_ => _.timeline.duration).nullable();
      applicationPlaying = Application.isPlaying;
      timelineRect = position;
      if (_visualsSettings.timeZoomFactor == 0) {
        // Initialize only if it was not initialized before.
        // We must set timelineRect before calling this.
        initializeZoomFactor(initialDuration: funTweenManager.map(_ => _.timeline.duration).getOrElse(1f));
      }
      timeRect = new Rect (position.x + _visualsSettings.timelineOffset, position.y, position.width - 15, 20);
      blackBarRect = new Rect (position.x + _visualsSettings.timelineOffset - 1, position.y + 19, position.width, 16);
  
      if (funTweenManager.valueOut(out var ftm) && (visualizationMode.value || applicationPlaying)) {
        currentTime = ftm.timeline.timePassed;
      }
      doCursor(funNodes);
      doToolbarGUI(position, funTweenManager, funNodes, selectedNodesList, snapping, rootNode);
      drawTicksGUI();
      doTimelineEvents(funNodes);

      startScrollView();
      {
        doLines(duration);
        drawNodes(funNodes, selectedNodesList, rootNode, nodeSnappedToOpt);
        doNodeEvents(funNodes, hasManager: funTweenManager.isSome);
      }
      endScrollView();
      doBlackBar();
      doTimelineGUI();
    }

    /// <summary>
    /// Timeline zone. It displays a time bar and rows of tweens.
    /// </summary>
    public Rect timelineZone => new Rect(
      timelineRect.x + _visualsSettings.timelineOffset,
      timeRect.height + blackBarRect.height,
      timelineRect.width - _visualsSettings.timelineOffset,
      timelineRect.height - timeRect.height - blackBarRect.height
    );

    public void endScrollView() => GUI.EndScrollView();

    public Vector2 startScrollView() =>
      _visualsSettings.scroll = GUI.BeginScrollView(
        timelineZone,
        _visualsSettings.scroll,
        new Rect(0, 0,
          timelineRect.width + _visualsSettings.lastNodeTime + expandView.x - _visualsSettings.timelineOffset,
          timelineRect.height + 400 + expandView.y),
        true, true
      );

    static GUIStyle barStyle;
    
    public void drawNodes(
      List<TimelineNode> funNodes, List<TimelineNode> selectedNodes, Option<TimelineNode> root,
      Option<TimelineEditor.NodeSnappedTo> nodeSnappedToOpt
    ) {
      GUI.enabled = GUI.enabled && !EditorApplication.isCompiling;
      
      if (barStyle == null) barStyle = "flow node 0"; // "TL LogicBar 0"
  
      Option<Rect> snapIndicatorOpt = None._;
      callbackVisualsList.Clear();
      var indicatorColor = Color.green;
  
      foreach (var currNode in funNodes) {
        {
          if (!currNode.isCallback) {
            var mouseCursor = Event.current.alt
              ? MouseCursor.Pan 
              : MouseCursor.ResizeHorizontal;
            EditorGUIUtility.AddCursorRect(nodeStartRect(currNode), mouseCursor);
            EditorGUIUtility.AddCursorRect(nodeEndRect(currNode), mouseCursor);
          }
          
          EditorGUIUtility.AddCursorRect(nodeBodyRect(currNode), MouseCursor.Pan);
  
          var boxRect = new Rect(
            secondsToGUI(currNode.startTime), 
            currNode.channel * CHANNEL_HEIGHT,
            Math.Max(secondsToGUI(currNode.duration), 6f), 
            CHANNEL_HEIGHT
          );
          
          var selectedCurrentNode = selectedNodes.find(selected => selected == currNode);
  
          // Transparent texture with a tooltip text.
          var tooltip = new GUIContent(EditorGUIUtility.FindTexture("tranp"), currNode.name);

          if (selectedCurrentNode.valueOut(out var selectedNode)) {
            drawOutline(boxRect, Color.magenta);

            bool isSnappedToRootStart(TimelineEditor.NodeSnappedTo nodeSnappedTo) =>
              nodeSnappedTo.snapType == SnapType.StartWithEnd
              || nodeSnappedTo.snapType == SnapType.StartWithStart;
            
            if (root.valueOut(out var rootNode) && rootNode == currNode) {
              if (nodeSnappedToOpt.valueOut(out var nodeSnappedTo)) {
                var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
                var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * CHANNEL_HEIGHT;
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
                    nawd.channel * CHANNEL_HEIGHT - 10, 2, dist
                  );
              }
  
              indicatorColor = isSnappedToRootStart(nodeSnappedTo) ? Color.yellow : Color.cyan;
            }
          }
          else if (!currNode.isCallback) {
            GUI.Box(boxRect, "", barStyle);
          }
          
          if (currNode.isCallback) {
            callbackVisualsList.Add(new CallbackVisuals(boxRect, tooltip));
          }
  
          if (currNode.linkedNode.valueOut(out var linkedNode)) {
            drawOutline(boxRect, Color.green);
            EditorGUI.DrawRect(
              new Rect(secondsToGUI(linkedNode.getEnd()),
                currNode.channel * CHANNEL_HEIGHT + 10, secondsToGUI(currNode.startTime - linkedNode.getEnd()), 2),
              Color.green
            );
            EditorGUI.DrawRect(
              new Rect(secondsToGUI(linkedNode.getEnd()),
                currNode.channel * CHANNEL_HEIGHT, 3, CHANNEL_HEIGHT),
              Color.green
            );
          }

          if (!currNode.element.isValid) {
            drawOutline(boxRect, Color.red);
          }

          var style = new GUIStyle("Label");
          style.fontSize = selectedCurrentNode.isSome ? 12 : style.fontSize;
          if (!currNode.isCallback) {
            style.fontStyle = FontStyle.Bold;
          }
          var color = currNode.nodeTextColor;
          // color.a = selectedCurrentNode.isSome ? 1.0f : 0.7f;
          style.normal.textColor = color;
          var content = currNode.name;
          if (changeTime) {
            content += $" {(int) (currentTime.remapClamped(currNode.startTime, currNode.getEnd(), 0, 100))}%";
          } 
          var labelRect = boxRect;
          if (currNode.isCallback) {
            // Put label to the right of the callback
            labelRect.xMin = labelRect.xMax + 5;
            // TODO: limit width until the next element starts on the same line
            labelRect.width = 400;
          }
          using (new GUILayout.AreaScope(labelRect)) {
            // FlexibleSpace centers vertically
            GUILayout.FlexibleSpace();
            using (new GUILayout.HorizontalScope()) {
              {
                var target = currNode.element?.element?.getTarget();
                if (target) {
                  var icon = ElementSelector.componentIcon(target);
                  GUILayout.Label(
                    icon, 
                    GUILayout.MaxHeight(CHANNEL_HEIGHT - 4),
                    GUILayout.MaxWidth(CHANNEL_HEIGHT)
                  );
                }
              }
              // FlexibleSpace centers horizontally
              // if (!currNode.isCallback) GUILayout.FlexibleSpace();
              // Draw label in a middle of the cell
              GUILayout.Label(content, style);
              // if (!currNode.isCallback) GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
          }
          
          void drawOutline(Rect aroundRect, Color outlineColor) {
            if (!currNode.isCallback) {
              EditorGUI.DrawRect(aroundRect, outlineColor);
              GUI.Box(new Rect(
                aroundRect.x + OUTLINE_WIDTH,
                aroundRect.y + OUTLINE_WIDTH,
                aroundRect.width - OUTLINE_WIDTH * 2,
                aroundRect.height - OUTLINE_WIDTH * 2
              ), "", barStyle);
            }
            else {
              var visuals = new CallbackVisuals(boxRect, tooltip);
              EditorGUI.DrawRect(visuals.iconRect, outlineColor);
              drawCallbackIcon(new CallbackVisuals(boxRect, tooltip));
            }
          }
        }
  
        if (snapIndicatorOpt.valueOut(out var snapIndicator)) {
          EditorGUI.DrawRect(snapIndicator, indicatorColor);
        }
  
        foreach (var callbackVisual in callbackVisualsList) {
          drawCallbackIcon(callbackVisual);
        }
        
        void drawCallbackIcon(CallbackVisuals visuals) {
          GUI.color = Color.yellow;
          GUI.DrawTexture(visuals.iconRect, EditorGUIUtility.FindTexture("d_animationkeyframe"));
          GUI.Label(visuals.labelRect, visuals.labelContent);
          GUI.color = Color.white;
        }
      }
    }
  
    public void doLines(float? duration) {
      Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
      // Horizontal lines
      for (var y = 0f; y < (int) timelineRect.height + _visualsSettings.scroll.y; y += CHANNEL_HEIGHT) {
        Handles.DrawLine(new Vector3(0, y, 0), new Vector3(timelineRect.width + _visualsSettings.scroll.x, y, 0));	
      }
      // Vertical line - end of timeline
      if (duration.HasValue) {
        Handles.DrawLine(
          new Vector3(secondsToGUI(duration.Value), 0),
          new Vector3(secondsToGUI(duration.Value), timelineRect.height)
        );
      }

      Handles.color = Color.white;
    }
    
    public void doTimelineGUI() {
      if ((changeTime || Application.isPlaying || visualizationMode.value)
        && timePosition - _visualsSettings.scroll.x >= _visualsSettings.timelineOffset 
        && timePosition - _visualsSettings.scroll.x < timelineRect.width - 15
      ) {
        var color = Color.red;
        color.a = Application.isPlaying ? 0.6f : 1.0f;
        Handles.color = color;
        
        var style = new GUIStyle("Label") {
          fontSize = 12,
          fontStyle = FontStyle.Bold,
          normal = {textColor = color}
        };
        Vector3 size = style.CalcSize(new GUIContent($"content: {currentTime:F2}s"));
        var rect1 = new Rect(timePosition - _visualsSettings.scroll.x, 19, size.x, size.y);
        GUI.Label(rect1, $"{currentTime:F2}s", style);
        Handles.DrawLine(
          new Vector3(timePosition - _visualsSettings.scroll.x, 0),
          new Vector3(timePosition - _visualsSettings.scroll.x, timelineRect.height - 15)
        );
        Handles.color = Color.white;
      }
    }
    
    void doCursor(List<TimelineNode> funNodes) {
      if (!funNodes.Exists(node =>
        new Rect(_visualsSettings.timelineOffset - 5, (node.channel + 2) * CHANNEL_HEIGHT, 20, CHANNEL_HEIGHT).Contains(Event.current.mousePosition)
        && node.startTime < 1f
      )) {
        EditorGUIUtility.AddCursorRect(
          new Rect(_visualsSettings.timelineOffset - 5, 37, 10, timelineRect.height),
          MouseCursor.ResizeHorizontal
        );
      }
    }
    
    public void doBlackBar() {
      if (Event.current.type == EventType.Repaint) {
        ((GUIStyle)"AnimationEventBackground").Draw(blackBarRect, GUIContent.none, 0);
      }
    }
    
    public void doToolbarGUI(Rect position, Option<FunTweenManagerV2> funTweenManager,
      List<TimelineNode> funNodes, List<TimelineNode> selectedNodes, bool snapping, Option<TimelineNode> rootNode
    ){
      
      GUILayout.BeginArea(
        new Rect(position.x, position.y, _visualsSettings.timelineOffset, position.height), 
        GUIContent.none
      );

      GUILayout.BeginHorizontal (EditorStyles.toolbar);
      var guiEnabled = GUI.enabled;

      GUI.enabled = !ftmsLabels.isEmpty() && !visualizationMode.value
        && !isLocked.value && !EditorApplication.isCompiling;
      
      EditorGUI.BeginChangeCheck();
      _visualsSettings.selectedFTMindex = EditorGUILayout.Popup(_visualsSettings.selectedFTMindex, ftmsLabels);
      isDifferentFTMselected = EditorGUI.EndChangeCheck();

      if (isDifferentFTMselected && !visualizationMode.value) {
        onFTMselectionChange(_visualsSettings.selectedFTMindex);
      }

      GUI.enabled = !EditorApplication.isCompiling && guiEnabled;
      GUI.backgroundColor = isLocked.value ? Color.gray : Color.white;
      
      if (GUILayout.Button(isLocked.value ? new GUIContent(lockOnTexture, "Lock selected timeline") : new GUIContent(lockTexture, "Unlock selected timeline"), EditorStyles.toolbarButton)) {
        onLockButton();
      }
      
      GUI.backgroundColor = Color.white;
      GUILayout.Space(10f);

      if (GUILayout.Button(new GUIContent(toStartButtonTexture, "Go to start"), new GUIStyle(EditorStyles.toolbarButton))) {
        onPlaybackControllerButton(AnimationPlaybackEvent.GoToStart);
      }

      GUI.backgroundColor = new Color(0, 0.8f, 1, 0.5f);
      if (GUILayout.Button(new GUIContent(startButtonTexture, "Play from start"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromStart);
      }
      GUI.backgroundColor = Color.white;

      if (GUILayout.Button(new GUIContent(playButtonTexture, "Play from current time"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromCurrentTime);
      }

      if (GUILayout.Button(new GUIContent(pauseButtonTexture, "Pause"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.Pause);
      }

      GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
      if (GUILayout.Button(new GUIContent(playFromEndButtonTexture, "Play from end"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.PlayFromEnd);
      }

      GUI.backgroundColor = Color.white;

      if (GUILayout.Button(new GUIContent(toEndButtonTexture, "Go to end"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.GoToEnd);
      }

      if (GUILayout.Button(new GUIContent(reverseButtonTexture, "Reverse"), EditorStyles.toolbarButton)) {
        onPlaybackControllerButton(AnimationPlaybackEvent.Reverse);
      }

      GUILayout.FlexibleSpace();

      if (!Application.isPlaying) {
        GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
        if (GUILayout.Button(new GUIContent(exitButtonTexture, "Exit"), EditorStyles.toolbarButton)) {
          onPlaybackControllerButton(AnimationPlaybackEvent.Exit);
          return;
        }
      }
  
      GUI.backgroundColor = Color.white;
      GUILayout.EndHorizontal ();
      
      GUILayout.BeginHorizontal ();
      GUILayout.BeginVertical ();
      
      doSettingsGUI(funTweenManager, funNodes, selectedNodes, snapping, rootNode);
      
      GUILayout.EndVertical ();
      GUILayout.Space (1.5f);
      GUILayout.EndHorizontal ();
      GUILayout.EndArea ();
    }

    public void doSettingsGUI(Option<FunTweenManagerV2> funTweenManager, List<TimelineNode> funNodes,
      List<TimelineNode> selectedNodesList, bool snapping, Option<TimelineNode> rootSelectedNodeOpt
    ) {
      if (funTweenManager.isSome) {
        GUILayout.BeginVertical();
        GUI.enabled = !visualizationMode.value && GUI.enabled;

        funNodes.find(elem => elem.element.element == null).map(_ => GUI.enabled = false);

        if (GUILayout.Button("Add Tween (USE DRAG & DROP INSTEAD !!!)")) {
          onNewSettings(TimelineEditor.SettingsEvents.AddTween);
        }

        GUILayout.EndVertical();
      }

      var guiEnabled = GUI.enabled;
      var oneNodeSelected = selectedNodesList.Count == 1;

        if (funTweenManager.isSome) {
          if (!selectedNodesList.isEmpty()) {
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
            if (GUILayout.Button("Remove Selected")) {
              onNodeEvent(
                TimelineEditor.NodeEvents.RemoveSelected, None._, GUIToSeconds(Event.current.mousePosition.x),
                GUIToChannel(Event.current.mousePosition.y)
              );
            }

            GUI.backgroundColor = snapping ? new Color(0, 1, 0, 0.5f) : Color.white;
            if (GUILayout.Button(snapping ? "Snapping ON" : "Snapping OFF")) {
              onNewSettings(TimelineEditor.SettingsEvents.ToggleSnapping);
            }

            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            // if (oneNodeSelected && rootSelectedNodeOpt.valueOut(out var selectedNode)) {
            //   var linkButtonText = "LINK";
            //   var unlinkButtonText = "UNLINK";
            //   GUILayout.BeginHorizontal();
            //
            //   if (selectedNode.linkedNode.isNone) {
            //     GUI.enabled = false;
            //     unlinkButtonText = "UNLINKED";
            //   }
            //
            //   if (GUILayout.Button(unlinkButtonText)) {
            //     onNewSettings(TimelineEditor.SettingsEvents.Unlink);
            //   }
            //
            //   GUI.enabled = guiEnabled;
            //
            //   if (selectedNode.linkedNode.isSome) {
            //     GUI.enabled = false;
            //     linkButtonText = "LINKED";
            //   }
            //
            //   if (GUILayout.Button(linkButtonText)) {
            //     onNewSettings(TimelineEditor.SettingsEvents.Link);
            //   }
            //
            //   GUI.enabled = guiEnabled;
            //   GUILayout.EndHorizontal();
            // }

          }
        }

        if (funTweenManager.isNone) {
          if (GUILayout.Button("[Add manager]")) {
            onNewSettings(TimelineEditor.SettingsEvents.AddManager);
          }
        }

        GUILayout.Space(10);
        GUI.enabled = !visualizationMode.value;
        settingsScroll = GUILayout.BeginScrollView(settingsScroll);
        if (funTweenManager.valueOut(out var ftm) && oneNodeSelected && !visualizationMode.value) {
          drawElementSettings(ftm, _visualsSettings.timelineOffset - 1.5f, rootSelectedNodeOpt);
        }
        else {
          if (funTweenManager.isSome) {
            using (new GUILayout.HorizontalScope()) {
              GUILayout.FlexibleSpace();
              GUILayout.Label(
                "Drag and drop a game object or a component to this screen to open tween selection popup.",
                EditorStyles.wordWrappedLabel
              );
              GUILayout.FlexibleSpace();
            }
          }
        }

        GUILayout.EndScrollView();
      }

    void drawElementSettings(FunTweenManagerV2 manager, float width, Option<TimelineNode> rootSelectedNodeOpt) {
      foreach (var rootSelectedObject in rootSelectedNodeOpt) {
        if (maybeTree == null || maybeTree.WeakTargets[0] is FunTweenManagerV2 treeManager && treeManager != manager) {
          maybeTree?.Dispose();
          maybeTree = new PropertyTree<FunTweenManagerV2>(new[] { manager }, new SerializedObject(manager));
        }
        if (maybeProperty == null || maybeProperty.ValueEntry.WeakSmartValue != rootSelectedObject.element) {
          maybeProperty?.Dispose();
          maybeProperty = null;
          var idx = Array.IndexOf(manager.serializedTimeline.elements, rootSelectedObject.element);
          if (idx >= 0) {
            // Need to update tree, otherwise we may not find new elements after duplicating them.
            maybeTree.UpdateTree();
            maybeProperty = maybeTree.GetPropertyAtPath($"_timeline._elements.${idx}");
          }
        }
      }
      
      const float OFFSET = 15;
      GUILayout.BeginArea(new Rect(0 + OFFSET, 0, width - OFFSET * 2, timelineRect.height - 100));
      elementPreviewScrollPosition = GUILayout.BeginScrollView(elementPreviewScrollPosition);
      
      {
        // use this to list all property paths
        // foreach (var inspectorProperty in tree.EnumerateTree(true)) {
        //   GUILayout.Label(inspectorProperty.Path);
        // }
      }
      
      if (maybeTree != null && maybeProperty != null) {
        InspectorUtilities.BeginDrawPropertyTree(maybeTree, true);
        foreach (var p in maybeProperty.Children) {
          try {
            p.Draw(p.Label);
          }
          catch (Exception ex) {
            // taken from InspectorUtilities.DrawPropertiesInTree
            // if (ex.IsExitGUIException()) throw ex.AsExitGUIException();
            Debug.LogException(new OdinPropertyException(
              "This error occurred while being drawn by Odin. \n" +
              "Odin Property Path: " + p.Path + "\n" +
              "Odin Drawer Chain: " + string.Join(
                ", ",
                p.GetActiveDrawerChain().BakedDrawerArray.Select(n => n.GetType().GetNiceName()).ToArray()
              ) + ".", ex));
          }
        }
        InspectorUtilities.EndDrawPropertyTree(maybeTree);
      }
      
      GUILayout.EndScrollView();
      GUILayout.EndArea();
    }
    
    void drawTicksGUI(){
      if (Event.current.type == EventType.Repaint) {
        EditorStyles.toolbar.Draw (timeRect, GUIContent.none, 0);
      }
      const float SCALE = 5f;
      Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
      var count = 0;
      for (var x = timeRect.x - _visualsSettings.scroll.x; x < timeRect.width; x += ZOOM / SCALE * _visualsSettings.timeZoomFactor) {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        if (x >= _visualsSettings.timelineOffset) {
          if (count % 5 == 0) {
            var first = _visualsSettings.timeIndexFactor == 0;
            Handles.DrawLine(new Vector3(x, 7, 0), new Vector3(x, 17, 0));
            var displayMinutes = Mathf.FloorToInt(count / SCALE * timeFactor[_visualsSettings.timeIndexFactor] / 60.0f);
            var displaySeconds = Mathf.FloorToInt(count / SCALE * timeFactor[_visualsSettings.timeIndexFactor] % 60.0f);
            var content = new GUIContent(
              first
                ? displaySeconds.ToString()
                : $"{displayMinutes:0}:{displaySeconds:00}"
            );
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
    
    Rect nodeStartRect(TimelineNode node) => 
      new Rect(secondsToGUI(node.startTime) - 5, node.channel * CHANNEL_HEIGHT, 10, CHANNEL_HEIGHT);

    Rect nodeEndRect(TimelineNode node) =>
      new Rect(secondsToGUI(node.startTime + node.duration) - 5, node.channel * CHANNEL_HEIGHT, 10, CHANNEL_HEIGHT);

    Rect nodeBodyRect(TimelineNode node) {
      var additionalWidth = node.isCallback 
        ? CHANNEL_HEIGHT 
        : (secondsToGUI(node.duration) < 7 ? 14 : 0);
      return new Rect(
        secondsToGUI(node.startTime) - additionalWidth / 2,
        node.channel * CHANNEL_HEIGHT,
        secondsToGUI(node.duration) + additionalWidth,
        CHANNEL_HEIGHT
      );
    }

    public void doNodeEvents(List<TimelineNode> funNodes, bool hasManager) {
      if (!GUI.enabled || visualizationMode.value || Application.isPlaying) {
        return;
      }

      var ev = Event.current;
      
      switch (ev.keyCode) {
        case KeyCode.A when ev.control && ev.type == EventType.KeyUp:
          nodeEvent(TimelineEditor.NodeEvents.SelectAll);
          break;
        case KeyCode.D when ev.control && ev.type == EventType.KeyUp:
          nodeEvent(TimelineEditor.NodeEvents.DuplicateSelected);
          break;
        case KeyCode.Delete when ev.type == EventType.KeyDown:
          nodeEvent(TimelineEditor.NodeEvents.RemoveSelected);
          break;
        case KeyCode.L when ev.type == EventType.KeyDown:
          onLockButton();
          break;
      }
      
      // rawType works better that type when we release mouse button outside window.
      switch (ev.rawType) {
        case EventType.MouseDown:
          foreach (var node in funNodes) {
            if (nodeStartRect(node).Contains(Event.current.mousePosition) && !node.isCallback && !ev.alt) {
              nodeEvent(
                ev.alt
                ? TimelineEditor.NodeEvents.NodeClicked_MB1
                : TimelineEditor.NodeEvents.ResizeStart,
                node
              );
                
              EditorGUI.FocusTextInControl("");
              return;
            }

            if (nodeEndRect(node).Contains(Event.current.mousePosition) && !node.isCallback && !ev.alt) {
              nodeEvent(
                ev.alt
                ? TimelineEditor.NodeEvents.NodeClicked_MB1
                : TimelineEditor.NodeEvents.ResizeEnd, 
                node
              );
                
              EditorGUI.FocusTextInControl("");
              return;
            }
            
            if (nodeBodyRect(node).Contains(Event.current.mousePosition)) {
              switch (ev.button) {
                case 0:
                  nodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB1, node);
                  EditorGUI.FocusTextInControl("");
                  break;
                case 1:
                  nodeEvent(TimelineEditor.NodeEvents.NodeClicked_MB2, node);
                  break;
              }
              return;
            }
          }
        
          // Deselect by clicking away
          if (!ev.control && timelineRect.Contains(Event.current.mousePosition)) {
            nodeEvent(TimelineEditor.NodeEvents.DeselectAll);
            return;
          }
          break;
        case EventType.MouseDrag:
          nodeEvent(TimelineEditor.NodeEvents.MouseDrag);
          return;
        case EventType.MouseUp:
          nodeEvent(TimelineEditor.NodeEvents.Refresh);
          return;
        case EventType.DragUpdated: 
          if (hasManager && canAcceptDrag()) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            ev.Use();
          }
          break;
        case EventType.DragPerform:
          if (hasManager && canAcceptDrag()) {
            nodeEvent(TimelineEditor.NodeEvents.AcceptDrag);
          }
          break;
          
        default: break;
      }

      void nodeEvent(TimelineEditor.NodeEvents nodeEvent, TimelineNode maybeNode = null) {
        ev.Use();
        onNodeEvent(
          nodeEvent, Option.a(maybeNode), 
          GUIToSeconds(Event.current.mousePosition.x), GUIToChannel(Event.current.mousePosition.y)
        );
      }
    }

    static bool canAcceptDrag() {
      var refs = DragAndDrop.objectReferences;
      return refs != null && refs.Length == 1;
    }

    public void doTimelineEvents(List<TimelineNode> funNodes){
      if (!GUI.enabled && !visualizationMode.value) {
        return;
      }
      
      var ev = Event.current;

      switch (ev.rawType) {
        
        case EventType.MouseDown:
          if (new Rect(_visualsSettings.timelineOffset - 5, 37, 10, timelineRect.height).Contains(ev.mousePosition)
            && funNodes.find(node => 
              new Rect(_visualsSettings.timelineOffset - 5, (node.channel + 2) * CHANNEL_HEIGHT, 20, CHANNEL_HEIGHT)
                .Contains(ev.mousePosition)
              && node.startTime < 1f).isNone
          ) {
            changeOffset = true;
            clickOffset = timePosition - _visualsSettings.timelineOffset;
            ev.Use();
          }
          
          if (new Rect (_visualsSettings.timelineOffset, 0, timelineRect.width, 37).Contains(Event.current.mousePosition)
            && Event.current.button == 0
          ) {
            timePosition = Event.current.mousePosition.x + _visualsSettings.scroll.x;
            changeTime = true;
  
            onTimelineCursorLine(true, currentTime);
    
            ev.Use();
          }
          
          break;
        
        case EventType.MouseUp:
          if (changeTime) {
            onTimelineCursorLine(false);
            
            // If we do not call use, we get this error after releasing mouse on time bar:
            //   ArgumentException: Getting control 0's position in a group with only 0 controls when doing mouseUp Aborting
            ev.Use();
          }
          changeTime = false;
          changeOffset = false;
          timePosition = 0;
          break;
        
        case EventType.MouseDrag:
          if (changeTime && Event.current.button == 0) {
            timePosition = Event.current.mousePosition.x + _visualsSettings.scroll.x;
            onTimelineCursorLine(true, currentTime);
          }
          
          switch (ev.button) {
            case 0:
              // Left button drag
              if (changeOffset) {
                _visualsSettings.timelineOffset = ev.mousePosition.x;
                _visualsSettings.timelineOffset = Mathf.Clamp(_visualsSettings.timelineOffset, 170, timelineRect.width - 170);
                timePosition = _visualsSettings.timelineOffset + clickOffset;
                timePosition = Mathf.Clamp(timePosition, _visualsSettings.timelineOffset, Mathf.Infinity);
                ev.Use();
              }
              break;
            case 1:
              break;
            case 2:
              // Middle button drag
              _visualsSettings.scroll -= ev.delta;
              _visualsSettings.scroll.x = Mathf.Clamp(_visualsSettings.scroll.x, 0f, Mathf.Infinity);
              _visualsSettings.scroll.y = Mathf.Clamp(_visualsSettings.scroll.y, 0f, Mathf.Infinity);
              
              expandView -= ev.delta;
              expandView.x = Mathf.Clamp(expandView.x, 20f, Mathf.Infinity);
              expandView.y = Mathf.Clamp(expandView.y, 20f, Mathf.Infinity);
              ev.Use();
              break;
            }
          break;
        case EventType.ScrollWheel:
          var f = _visualsSettings.timeZoomFactor;
          if (_visualsSettings.timeIndexFactor == timeFactor.Length - 1 && f < 0.5f) {
            f = 0.5f;
          } else {
            f += ev.delta.y / 100;
          }
          if (f < 0.5f && _visualsSettings.timeIndexFactor < timeFactor.Length - 1) {
            _visualsSettings.timeIndexFactor++;
            f = 1;
          }
          if (f > 1.5f && _visualsSettings.timeIndexFactor > 0) {
            _visualsSettings.timeIndexFactor--;
            f = 1;
          }
          
          recalculateTimelineWidth(funNodes);
          _visualsSettings.timeZoomFactor = f;

          ev.Use();
          break;
        default: break;
      }
    }
  
    public float secondsToGUI(float seconds) => 
      seconds / timeFactor[_visualsSettings.timeIndexFactor] * ZOOM * _visualsSettings.timeZoomFactor;

    public float GUIToSeconds(float xCoord) {
      var guiSecond = ZOOM * _visualsSettings.timeZoomFactor / timeFactor[_visualsSettings.timeIndexFactor];
      return xCoord / guiSecond;
    }
    
    public int GUIToChannel(float yCoord) => ((int) (yCoord / CHANNEL_HEIGHT)).atLeast(0);

    public void recalculateTimelineWidth(List<TimelineNode> funNodes) {
      foreach (var timelineNode in funNodes.maxBy(Comparable.float_, node => node.getEnd())) {
        _visualsSettings.lastNodeTime = secondsToGUI(timelineNode.getEnd());
      }
    }
  }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using pzd.lib.data;
using pzd.lib.functional;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager.SerializedTweenTimelineV2.Element;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineEditor : EditorWindow, IMB_OnGUI, IMB_OnEnable, IMB_OnDisable {
    
    public enum SettingsEvents : byte {
      AddTween,
      ToggleSnapping,
      Link,
      Unlink,
      AddManager,
      UpdateExternalWindow
    }
    
    public enum NodeEvents : byte {
      ResizeStart,
      ResizeEnd,
      NodeClicked_MB1,
      NodeClicked_MB2,
      MouseDrag,
      DeselectAll,
      RemoveSelected,
      SelectAll,
      Refresh,
      DuplicateSelected,
      AcceptDrag
    }
    
    public enum SnapType : byte {
      StartWithStart,
      StartWithEnd,
      EndWithEnd,
      EndWithStart
    }

    [Record]
    public readonly partial struct NodeSnappedTo {
      public readonly TimelineNode node;
      public readonly SnapType snapType;
    }

    Init init;

    readonly Dictionary<FunTweenManagerV2, TimelineVisuals.TimelineVisualsSettings> mappedSettings = new();

    public void OnGUI() => init?.onGUI(Event.current);

    public void OnEnable() {
      FunTweenManagerV2.timelineEditorIsOpen = true;
      refreshInit(None._, None._);
    }

    void refreshInit(Option<FunTweenManagerV2> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet) {
      if (init == null) init = new Init(this, ftmToSetOpt, rootSelectedNodeToSet);
      else if (!init.isLocked.value) {
        init.Dispose();
        init = new Init(this, ftmToSetOpt, rootSelectedNodeToSet);
      }
    }
   
    public void OnDisable() {
      FunTweenManagerV2.timelineEditorIsOpen = false;
      if (init != null) {
        init.Dispose();
        init = null;
      }
    }
    
    void OnSelectionChange() {
      init?.saveCurrentVisualSettings();
      refreshInit(None._, None._);
    } 
    
    void OnLostFocus() => init?.onLostFocus();

    // Removed, because it causes problems
    // void OnHierarchyChange() {
    //   var initOpt = F.opt(init);
    //   refreshInit(initOpt.flatMap(_ => _.selectedFunTweenManager), initOpt.flatMap(_ => _.rootSelectedNodeOpt));
    // }
    
    [MenuItem("TLP/TweenTimeline", false)]
    public static void showWindow() {
      var window = GetWindow<TimelineEditor>("Tween Timeline");
      window.wantsMouseMove = true;
      DontDestroyOnLoad(window);
    }

    partial class Init : IDisposable {
      const int SNAPPING_POWER = 10;
      public readonly Ref<bool> isLocked = new SimpleRef<bool>(false);
      readonly ImmutableArray<FunTweenManagerV2> ftms;
      readonly TimelineEditor backing;
      readonly TimelineVisuals timelineVisuals;
      readonly Option<GameObject> selectedGameObjectOpt;
      readonly List<float> diffList = new();
      readonly List<TimelineNode> selectedNodesList = new();
      readonly Ref<bool> visualizationMode = new SimpleRef<bool>(false);

      Option<FunTweenManagerV2> selectedFunTweenManager { get; set; }
      Option<TimelineNode> rootSelectedNodeOpt { get; set; }
      
      List<TimelineNode> funNodes = new();
      bool isStartSnapped, isEndSnapped, resizeNodeStart, resizeNodeEnd, dragNode, snapping = true;
      Option<NodeSnappedTo> nodeSnappedToOpt;
      Option<TweenPlaybackController> tweenPlaybackController;
      float timeClickOffset;

      void OnPlaymodeStateChanged(PlayModeStateChange change) {
        using var _ = new ProfiledScope(Macros.classAndMethodName);
        backing.OnEnable();
      }

      public void onLostFocus() {
        foreach (var controller in tweenPlaybackController) {
          controller.stopVisualization();
        }
      }

      public Init(
        TimelineEditor backing, Option<FunTweenManagerV2> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet
      ) {
        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        Undo.undoRedoPerformed += undoCallback;
        EditorSceneManager.sceneSaving += EditorSceneManagerOnSceneSaving;

        selectedGameObjectOpt = Selection.activeGameObject.opt();

        isStartSnapped = false;
        isEndSnapped = false;

        funNodes.Clear();
        selectedNodesList.Clear();

        this.backing = backing;

        ftms = getFunTweenManagers(selectedGameObjectOpt);

        foreach (var root in rootSelectedNodeToSet) selectedNodesList.Add(root);
        rootSelectedNodeOpt = rootSelectedNodeToSet;

        selectedFunTweenManager =
          ftmToSetOpt
            .flatMap(ftmToSet => ftms.find(ftm => ftm == ftmToSet))
            || ftms.headOption();
          
        timelineVisuals = new TimelineVisuals(
          manageAnimationPlayback, toggleLock, manageCursorLine, doNodeEvents, doNewSettings, selectNewFunTweenManager,
          visualizationMode, isLocked, ftms,
          getSettings()
        );

        TimelineVisuals.TimelineVisualsSettings getSettings() {
          var idx = selectedFunTweenManager.isSome
            ? ftms.IndexOf(selectedFunTweenManager.get)
            : 0;

          return selectedFunTweenManager.flatMap(
              ftm => backing.mappedSettings.get(ftm)
                .map(settings => {
                  settings.selectedFTMindex = idx;
                  return settings;
              })
          ).getOrElse(new TimelineVisuals.TimelineVisualsSettings(idx));
        } 
        
        selectedFunTweenManager.voidFold(
          () => funNodes.Clear(),
          manager => {
            tweenPlaybackController = new TweenPlaybackController(manager, visualizationMode).some();
            funNodes.Clear();
            importTimeline();
          }
        );

        backing.Repaint();
      }

      public void Dispose() { 
        EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        Undo.undoRedoPerformed -= undoCallback;
        EditorSceneManager.sceneSaving -= EditorSceneManagerOnSceneSaving;
      }

      void EditorSceneManagerOnSceneSaving(Scene scene, string path) {
        // Why was this even here? It breaks the UI on save.
        // funNodes.Clear();
        
        foreach (var controller in tweenPlaybackController) {
          controller.stopVisualization();
        }
      }

      public void onGUI(Event currentEvent) {
        {if (selectedFunTweenManager.valueOut(out var ftm)) {
          // ftm may become invalid if we locked it previously, but then switched the scene.
          if (!ftm) {
            selectedFunTweenManager = None._;
            rootSelectedNodeOpt = None._;
            isLocked.value = false;
            funNodes.Clear();
          }
        }}
        
        if (currentEvent.isKey && visualizationMode.value) {
          foreach (var controller in tweenPlaybackController) {
            controller.manageAnimation(TweenPlaybackController.AnimationPlaybackEvent.Exit);
          }
          return;
        }
        
        GUI.enabled = selectedGameObjectOpt.isSome && !EditorApplication.isCompiling;

        timelineVisuals.doTimeline(
          new Rect(0, 0, backing.position.width, backing.position.height),
          selectedFunTweenManager,
          funNodes,
          selectedNodesList,
          snapping,
          rootSelectedNodeOpt,
          nodeSnappedToOpt
        );

        if (GUI.changed) {
          importTimeline();
        }

        backing.Repaint();
      }

      void undoCallback() {
        importTimeline();
      }

      // Selects or deselects node
      void manageSelectedNode(TimelineNode nodeToAdd, Event currentEvent) {
        if (!selectedNodesList.isEmpty()) {
          selectedNodesList.find(selectedNode => selectedNode == nodeToAdd).voidFold(
            () => {
              if (currentEvent.control) {
                selectedNodesList.Add(nodeToAdd);
              }
              else if (selectedNodesList.Count >= 1) {
                selectedNodesList.Clear();
                selectedNodesList.Add(nodeToAdd);
              }
            },
            selectedNode => {
              if (currentEvent.control) {
                selectedNodesList.Remove(nodeToAdd);
              }
            }
          );
        }
        else if (selectedNodesList.Count <= 1) {
          selectedNodesList.Clear();
          selectedNodesList.Add(nodeToAdd);
        }
      }

      void doNodeEvents(
        NodeEvents nodeEvent, Option<TimelineNode> timelineNodeOpt, float mousePositionSeconds,
        int mousePositionChannel
      ) {
        var snappingEnabled = !Event.current.shift && snapping;
        
        switch (nodeEvent) {
          case NodeEvents.RemoveSelected:
            removeAllSelectedNodes();
            selectedNodesList.Clear();
            importTimeline();
            break;
          
          case NodeEvents.DuplicateSelected:
            duplicateAllSelectedNodes();
            break;
          
          case NodeEvents.AcceptDrag:
            var dragTarget = DragAndDrop.objectReferences[0];
            DragAndDrop.AcceptDrag();

            var selector = new ElementSelector(dragTarget);
            selector.SelectionConfirmed += selection => {
              {if (selection != null && selection.headOption().valueOut(out var selectedValue)) {
                var element = selectedValue.createElement();
                addElement(new Element(Math.Max(mousePositionSeconds, 0), mousePositionChannel, element));
              }}
            };
            selector.ShowInPopup();
            break;
          
          case NodeEvents.SelectAll:
            selectedNodesList.Clear();
            foreach (var node in funNodes) {
              selectedNodesList.Add(node);
            }
            break;
          
          case NodeEvents.ResizeStart:
            foreach (var timelineNode in timelineNodeOpt) {
              removeRootNodeIfHasNoElement();
              manageSelectedNode(timelineNode, Event.current);
              selectedNodesList.Clear();
              selectedNodesList.Add(timelineNode);
              rootSelectedNodeOpt = timelineNodeOpt;
              resizeNodeStart = true;
            }
            break;
          
          case NodeEvents.ResizeEnd:
            foreach (var timelineNode in timelineNodeOpt) {
              removeRootNodeIfHasNoElement();
              manageSelectedNode(timelineNode, Event.current);
              rootSelectedNodeOpt = timelineNodeOpt;
              resizeNodeEnd = true;
            }
            break;
          
          case NodeEvents.NodeClicked_MB1:
            foreach (var timelineNode in timelineNodeOpt) {
              removeRootNodeIfHasNoElement();
              timeClickOffset = timelineNode.startTime - mousePositionSeconds;
              dragNode = true;
              rootSelectedNodeOpt = timelineNodeOpt;
              manageSelectedNode(timelineNode, Event.current);
            }

            break;
          
          case NodeEvents.NodeClicked_MB2:
            foreach (var timelineNode in timelineNodeOpt) {
              removeRootNodeIfHasNoElement();
              rootSelectedNodeOpt = timelineNodeOpt;
              manageSelectedNode(timelineNode, Event.current);
              var genericMenu = new GenericMenu();
              addMenuItem("Unselect", () => deselect(timelineNode));
              addMenuItem("Duplicate This", () => duplicate(timelineNode));
              if (selectedNodesList.Count > 0) {
                addMenuItem("Duplicate Selected", duplicateAllSelectedNodes);
              }
              addMenuItem("Delete This", () => removeSelectedNode(timelineNode));
              if (selectedNodesList.Count > 0) {
                addMenuItem("Delete Selected", removeAllSelectedNodes);
              }
              genericMenu.ShowAsContext();

              void addMenuItem(string label, Action act) {
                genericMenu.AddItem(new GUIContent(label), false, _ => act(), null);
              }
            }
            break;
          
          case NodeEvents.MouseDrag:
            if (rootSelectedNodeOpt.valueOut(out var rootSelected)) {
              
              if (resizeNodeStart) {
                var selectedNodeEnd = rootSelected.getEnd();
                if (selectedNodeEnd < mousePositionSeconds) break;
  
                rootSelected.setStartTime(mousePositionSeconds);
  
                if (rootSelected.startTime > 0 && !isStartSnapped) {
                  rootSelected.setDuration(selectedNodeEnd - rootSelected.startTime); 
                }
  
                if (snappingEnabled) {
                  snapStart(rootSelected, selectedNodeEnd);
                }
  
                foreach (var selected in selectedNodesList) {
                  if (selected != rootSelected) {
                    var nodeEnd = selected.getEnd();
                    selected.setStartTime(rootSelected.startTime);
                    selected.setDuration(nodeEnd - selected.startTime);
                  }
                }
              }

              if (resizeNodeEnd) {
                if (rootSelected.startTime > mousePositionSeconds) break;
                
                rootSelected.setDuration(mousePositionSeconds - rootSelected.startTime);
  
                if (snappingEnabled) {
                  snapEnd(rootSelected);
                }
  
                foreach (var node in selectedNodesList) {
                  if (node != rootSelected) {
                    node.setDuration(rootSelected.duration - (node.startTime - rootSelected.startTime));
                  }
                  updateLinkedNodeStartTimes(node);
                }
                timelineVisuals.recalculateTimelineWidth(funNodes);
              }
    
              // Dragging the node
              if (dragNode && !resizeNodeStart && !resizeNodeEnd || resizeNodeEnd && resizeNodeStart) {
                foreach (var selected in selectedNodesList) {
                  diffList.Add(selected.startTime - rootSelected.startTime);
                }
  
                var clampLimit =
                  selectedNodesList.find(node => node.startTime <= 0).isSome
                    ? rootSelected.startTime
                    : 0;
  
                rootSelected.setStartTime(mousePositionSeconds + timeClickOffset, clampLimit);
  
                isEndSnapped = false;
                isStartSnapped = false;
  
                if (snappingEnabled) {
                  snapDrag(rootSelected, selectedNodesList);
                }
  
                //setting multiselected nodes starttimes
                for (var i = 0; i < selectedNodesList.Count; i++) {
  
                  var node = selectedNodesList[i];
                  node.setStartTime(rootSelected.startTime + diffList[i]);
  
                  updateLinkedNodeStartTimes(node);
                }
  
                diffList.Clear();
  
                while (
                  Event.current.mousePosition.y > (rootSelected.channel + 1) * TimelineVisuals.CHANNEL_HEIGHT + 5) {
                  foreach (var node in selectedNodesList) {
                    updateLinkedNodeChannels(node, _ => _.increaseChannel());
                    if (node == rootSelected) {
                      node.unlink();
                    }
                  }
                }
  
                while (Event.current.mousePosition.y < rootSelected.channel * TimelineVisuals.CHANNEL_HEIGHT - 5
                       && selectedNodesList.find(node => node.channel == 0).isNone) {
                  foreach (var node in selectedNodesList) {
                    updateLinkedNodeChannels(node, _ => _.decreaseChannel());
                    if (node == rootSelected) {
                      node.unlink();
                    }
                  }
                }
  
                void updateLinkedNodeChannels(TimelineNode node, Action<TimelineNode> changeChannel) {
                  getLinkedRightNode(node, node).voidFold(
                    () => { },
                    rightNode => { updateLinkedNodeChannels(rightNode, changeChannel); }
                  );
                  
                  changeChannel(node);
                }
              }
            }
            break;
          
          case NodeEvents.DeselectAll:
            if (!dragNode && !resizeNodeStart && !resizeNodeEnd) {
              selectedNodesList.Clear();
            }
            break;
          
          case NodeEvents.Refresh:
            if (dragNode || resizeNodeEnd || resizeNodeStart) {
              moveOtherNodesDownIfOverlapping(selectedNodesList);
              exportTimelineToTweenManager();
              importTimeline();
              timelineVisuals.recalculateTimelineWidth(funNodes);
            }
            
            unlinkNodesWithBrokenLink();            
            isEndSnapped = false;
            isStartSnapped = false;
            dragNode = false;
            resizeNodeStart = false;
            resizeNodeEnd = false;
            nodeSnappedToOpt = None._;
            break;
        }
      }
      
      bool moveCurrentNodeDownIfOverlapping(TimelineNode timelineNode) {
        var moved = false;
        while (getOverlappingNode(timelineNode).isSome) {
          timelineNode.increaseChannel();
          moved = true;
        }
        return moved;
      }
      
      void moveOtherNodesDownIfOverlapping(List<TimelineNode> timelineNodes) {
        foreach (var node in timelineNodes) {
          while (getOverlappingNode(node).valueOut(out var overlappingNode)) {
            moveCurrentNodeDownIfOverlapping(overlappingNode);
          }
        }
      }

      void unlinkNodesWithBrokenLink() {
        foreach (var node in funNodes) {
          if (node.linkedNode.valueOut(out var linkedNode)
            && getLeftNode(node).valueOut(out var leftNode)
            && linkedNode != leftNode
          ) {
            node.unlink();
          }
        }
      }

      void updateLinkedNodeStartTimes(TimelineNode node) =>
        getLinkedRightNode(node, node).voidFold(
          () => { },
          rightNode => {
            if (rightNode.linkedNode.valueOut(out var nodeLinkedTo) && nodeLinkedTo == node) {
              rightNode.setStartTime(node.getEnd() + rightNode.element.startsAt);
            }

            updateLinkedNodeStartTimes(rightNode);
          }
        );

      Option<TimelineNode> getLinkedRightNode(TimelineNode initialNode, TimelineNode node) =>
        getRightNode(node).flatMap(rightNode => 
          rightNode.linkedNode.fold(
            () => getLinkedRightNode(initialNode, rightNode),
            linkedNode => 
              linkedNode == initialNode
              && selectedNodesList.find(x => x == rightNode).isNone
                ? rightNode.some()
                : None._
            )
        );

      void snapDrag(TimelineNode rootNode, List<TimelineNode> selectedNodes) {
        var nonSelectedNodes = funNodes.Except(selectedNodes).ToList();

        nonSelectedNodes.ForEach(earlierNode => nodeSnappedToOpt.voidFold(
          () => snap(earlierNode),
          nodeSnapped => snap(nodeSnapped.node)
        ));

        void snap(TimelineNode nodeToSnapTo) {
          nodeSnappedToOpt = getSnapType(nodeToSnapTo).map(
            snapType => {
              switch (snapType) {
                case SnapType.StartWithStart:
                  return setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime, ref isStartSnapped);
                case SnapType.StartWithEnd:
                  return setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd(), ref isStartSnapped);
                case SnapType.EndWithStart:
                  return setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime - rootNode.duration, ref isEndSnapped);
                case SnapType.EndWithEnd:
                  return setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd() - rootNode.duration, ref isEndSnapped);
                default:
                  throw new ArgumentOutOfRangeException(nameof(snapType), snapType, null);
              }
            }
          );
        }

        NodeSnappedTo setSnapping(
          TimelineNode nodeToSnapTo, bool snappedToNodeStart, float timeToSet, ref bool sideToSnap
        ) {
          rootNode.setStartTime(timeToSet);
          sideToSnap = true;

          SnapType getsnapType(bool isRootStart, bool isNodeStart) {
            if (isRootStart) {
              return isNodeStart ? SnapType.StartWithStart : SnapType.StartWithEnd;
            }
            else {
              return isNodeStart ? SnapType.EndWithStart : SnapType.EndWithEnd;
            }
          }

          return new NodeSnappedTo(nodeToSnapTo, getsnapType(isStartSnapped, snappedToNodeStart));
        }

        Option<SnapType> getSnapType(TimelineNode nodeSnapTo) {
          var nodeStart = timelineVisuals.secondsToGUI(nodeSnapTo.startTime);
          var nodeEnd = timelineVisuals.secondsToGUI(nodeSnapTo.getEnd());
          var dragNodeStart = timelineVisuals.secondsToGUI(rootNode.startTime);
          var dragNodeEnd = timelineVisuals.secondsToGUI(rootNode.getEnd());

          if (isInRangeOfSnap(nodeStart, dragNodeStart)) return SnapType.StartWithStart.some();
          if (isInRangeOfSnap(nodeEnd, dragNodeStart)) return SnapType.StartWithEnd.some();
          if (isInRangeOfSnap(nodeStart, dragNodeEnd)) return SnapType.EndWithStart.some();
          if (isInRangeOfSnap(nodeEnd, dragNodeEnd)) return SnapType.EndWithEnd.some();
          return None._;
        }
      }

      delegate float GetNodePoint(TimelineNode node);

      delegate bool CompareFloats(float a, float b);

      void manageSnap(float selectedNodePoint, GetNodePoint nodePoint, CompareFloats cmpr, Action<TimelineNode> snap) =>
        funNodes
          .Except(selectedNodesList)
          .Where(node => cmpr(selectedNodePoint, nodePoint(node)))
          .ToList()
          .ForEach(earlierNode => nodeSnappedToOpt.voidFold(
            () => snap(earlierNode),
            nodeSnapped => snap(nodeSnapped.node)
          ));

      static bool isInRangeOfSnap(float snapPoint, float positionToCheck) =>
        positionToCheck < snapPoint + SNAPPING_POWER && positionToCheck > snapPoint - SNAPPING_POWER;

      void snapEnd(TimelineNode selectedNode) =>
        manageSnap(selectedNode.startTime, node => node.getEnd(), (a, b) => a <= b,
          nodeToSnapTo => {
            var snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.startTime);
            var nodeEndPos = timelineVisuals.secondsToGUI(selectedNode.getEnd());
            if (isInRangeOfSnap(snapPoint, nodeEndPos)) {
              selectedNode.setDuration(nodeToSnapTo.startTime - selectedNode.startTime);
              isEndSnapped = true;
              nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.EndWithStart).some();
            }
            else {
              snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
              if (isInRangeOfSnap(snapPoint, nodeEndPos)) {
                selectedNode.setDuration(nodeToSnapTo.getEnd() - selectedNode.startTime);
                isEndSnapped = true;
                nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.EndWithEnd).some();
              }
              else {
                isEndSnapped = false;
                nodeSnappedToOpt = None._;
              }
            }
          }
        );

      void snapStart(TimelineNode selectedNode, float initialEnd) =>
        manageSnap(selectedNode.getEnd(), node => node.startTime, (a, b) => a >= b,
          nodeToSnapTo => {
            var end = selectedNode.getEnd();
            var snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.startTime);
            var nodeStartPos = timelineVisuals.secondsToGUI(selectedNode.startTime);

            if (isInRangeOfSnap(snapPoint, nodeStartPos)) {
              selectedNode.setStartTime(nodeToSnapTo.startTime); 
              if (!isStartSnapped) {
                selectedNode.setDuration(end - selectedNode.startTime);
                nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.StartWithStart).some();
                isStartSnapped = true;
              }
            }
            else {
              snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
              if (isInRangeOfSnap(snapPoint, nodeStartPos)) {
                selectedNode.setStartTime(nodeToSnapTo.getEnd());
                if (!isStartSnapped) {
                  selectedNode.setDuration(end - selectedNode.startTime);
                  nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.StartWithEnd).some();
                  isStartSnapped = true;
                }
              }
              else if (isStartSnapped) {
                selectedNode.setDuration(initialEnd - selectedNode.startTime);
                nodeSnappedToOpt = None._;
                isStartSnapped = false;
              }
            }
          }
        );
      
      // creates nodeList from elements info
      void importTimeline() {
        if (selectedFunTweenManager.valueOut(out var manager) && manager.timeline != null) {
          var elements = manager.serializedTimeline.elements;

          if (elements != null) {
            funNodes = elements.Select(element => new TimelineNode(element)).ToList();
            
            var newSelectedNodes = selectedNodesList.collect(oldNode => funNodes.find(
              mapper: newNode => newNode.element,
              toFind: oldNode.element
            )).ToArray();
            selectedNodesList.Clear();
            selectedNodesList.AddRange(newSelectedNodes);

            {
              var movedAnyNode = false;
              // Iterate in reverse order to move down newer elements
              for (var idx = funNodes.Count - 1; idx >= 0; idx--) {
                movedAnyNode |= moveCurrentNodeDownIfOverlapping(funNodes[idx]);
              }
              if (movedAnyNode) exportTimelineToTweenManager();
            }
          }
          else {
            funNodes.Clear();
          }

          //Relinking linked nodes, since we dont serialize nodeLinkedTo
          foreach (var node in funNodes) {
            // if (getLeftNode(node).valueOut(out var leftNode)) {
            //   if (node.element.startAt == Element.At.AfterLastElement) {
            //     node.linkTo(leftNode);
            //   }
            // }

            node.refreshColor();
          }
        }
      }

      Option<TimelineNode> getLeftNode(TimelineNode selectedNode) =>
        funNodes.Where(node => node.channel == selectedNode.channel
          && node.startTime < selectedNode.startTime
        ).ToList().toNonEmpty().map(
          channelNodes => channelNodes.a.OrderBy(channelNode => channelNode.startTime).Last()
        );

      Option<TimelineNode> getRightNode(TimelineNode selectedNode) =>
        funNodes.Where(node => node.channel == selectedNode.channel
          && node.getEnd() > selectedNode.getEnd()
        ).ToList().toNonEmpty().map(
          channelNodes => channelNodes.a.OrderBy(channelNode => channelNode.startTime).First()
        );

      Option<TimelineNode> getOverlappingNode(TimelineNode node) {
        var channelNodes = funNodes.Where(funNode => funNode.channel == node.channel && funNode != node);

        const float EPS = 1e-6f;

        var nodeStart = node.startTime;
        var nodeEnd = node.getEnd();
        var nodeCanTouch = !node.isCallback;

        return channelNodes.find(channelNode => {
          var channelNodeStart = channelNode.startTime;
          var channelNodeEnd = channelNode.getEnd();
          var channelNodeCanTouch = !channelNode.isCallback;

          var canTouch = nodeCanTouch && channelNodeCanTouch;
          
          var epsCanTouch = canTouch ? EPS : 0f;
          var epsStrict = canTouch ? 0f : EPS;

          var onLeft = channelNodeEnd + epsStrict < nodeStart + epsCanTouch;
          var onRight = channelNodeStart + epsCanTouch > nodeEnd + epsStrict;
          
          var overlapsRange = !onLeft && !onRight;
          var overlapsCallbacksVisually = node.isCallback && channelNode.isCallback
            && Math.Abs(node.startTime - channelNode.startTime) < timelineVisuals.GUIToSeconds(15f);
          
          return overlapsRange || overlapsCallbacksVisually;
        });
      }
      
      void exportTimelineToTweenManager() {
        if (selectedFunTweenManager.valueOut(out var manager) && !funNodes.isEmpty()) {
          EditorUtility.SetDirty(manager);
          Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "something changed");
          
          var arr = new List<TimelineNode>();
          arr.AddRange(funNodes);
          // Do not reorder elements. Odin inspector starts throwing exceptions if we do it.
          // If we reorder elements, we should at least dispose clear maybeProperty field.
          // for (var i = 0; i <= funNodes.Max(funNode => funNode.channel); i++) {
          //   arr.AddRange(
          //     funNodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
          //   );
          // }
          
          manager.serializedTimeline.elements = arr.Select(elem => {
            var resElement = elem.element;
            resElement.timelineChannelIdx = elem.channel;
            
            resElement.element?.trySetDuration(elem.duration);
            if (elem.linkedNode.valueOut(out _)) {
              throw new NotImplementedException("node linking is not implemented");
            }
            else {
              resElement.setStartsAt(elem.startTime);
            }
            
            return resElement;
          }).ToArray();
          
          EditorUtility.SetDirty(manager);
        }

        if (funNodes.isEmpty()) manager.serializedTimeline.elements = new Element[0];
      }

      void doNewSettings(SettingsEvents settingsEvent) {
        switch (settingsEvent) {
          case SettingsEvents.AddTween:
            var selector = new TypeSelector(ElementSelector.allElementTypes, false);
            selector.SelectionConfirmed += selection => {
              {if (selection != null && selection.headOption().valueOut(out var selectedValue)) {
                var element = (ISerializedTweenTimelineElementBase) Activator.CreateInstance(selectedValue);
                addElement(new Element(0, 0, element));
              }}
            };
            selector.ShowInPopup();
            break;
          case SettingsEvents.ToggleSnapping:
            snapping = !snapping;
            break;
          case SettingsEvents.Link:
            foreach (var selectedNode in rootSelectedNodeOpt)
              if (selectedFunTweenManager.valueOut(out var ftm)) {
                Undo.RegisterFullObjectHierarchyUndo(ftm, "Linked Nodes");
                if (getLeftNode(selectedNode).valueOut(out var leftNode)) {
                  selectedNode.linkTo(leftNode);
                }
              }
            break;
          case SettingsEvents.Unlink:
            foreach (var selectedNode in rootSelectedNodeOpt) {
              if (selectedFunTweenManager.valueOut(out var ftm)) {
                Undo.RegisterFullObjectHierarchyUndo(ftm, "Unlinked Nodes");
                selectedNode.unlink();
              }
            }
            break;
          case SettingsEvents.AddManager:
            addFunTweenManagerComponent(Selection.activeGameObject);
            EditorGUIUtility.ExitGUI();
            break;
          case SettingsEvents.UpdateExternalWindow:
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(settingsEvent), settingsEvent, null);
        }
      }

      void deselect(TimelineNode node) {
        selectedNodesList.Remove(node);
        backing.Repaint();
      }

      void removeSelectedNode(TimelineNode node) {
        funNodes.Remove(node);
        foreach (var linkedNode in getLinkedRightNode(node, node)) linkedNode.unlink();

        exportTimelineToTweenManager();
        importTimeline();
        rootSelectedNodeOpt = None._;
      }
      
      void duplicate(TimelineNode node) {
        addElement(node.element.deepClone());
      }

      void addElement(Element newElement) {
        {if (selectedFunTweenManager.valueOut(out var manager)) {
          manager.serializedTimeline.elements = 
            manager.serializedTimeline.elements.concat(new []{newElement});
          importTimeline();
        }}
      }
      
      void duplicateAllSelectedNodes() {
        {if (selectedFunTweenManager.valueOut(out var manager)) {
          manager.serializedTimeline.elements = 
            manager.serializedTimeline.elements.concat(selectedNodesList.Select(_ => _.element.deepClone()).ToArray());
          importTimeline();
        }}
      }

      void removeAllSelectedNodes() {
        funNodes = funNodes.Except(selectedNodesList).ToList();
        
        foreach (var selectedNode in selectedNodesList) {
          foreach (var linkedNode in getLinkedRightNode(selectedNode, selectedNode)) linkedNode.unlink();
        }

        exportTimelineToTweenManager();
        importTimeline();
        selectedNodesList.Clear();
        rootSelectedNodeOpt = None._;
      }

      void removeNodeIfHasNoElement(TimelineNode node) {
        if (node.element.element == null && !funNodes.isEmpty()) {
          funNodes.Remove(node);
          exportTimelineToTweenManager();
          importTimeline();
          rootSelectedNodeOpt = None._;
        }
      }

      void removeRootNodeIfHasNoElement() {
        // why did we need this code?
        // foreach (var rootNode in rootSelectedNodeOpt) {
        //   removeNodeIfHasNoElement(rootNode);
        // }
      }

      void addFunTweenManagerComponent(GameObject gameObject) {
        selectedFunTweenManager = Undo.AddComponent<FunTweenManagerV2>(gameObject).some();
        importTimeline();
        EditorUtility.SetDirty(gameObject);
        backing.OnEnable(); 
      }

      public void saveCurrentVisualSettings() {
        foreach (var ftm in selectedFunTweenManager) {
          backing.mappedSettings.Remove(ftm);
          backing.mappedSettings.Add(ftm, timelineVisuals.visualsSettings);
        }
      }

      void toggleLock() {
        isLocked.value = !isLocked.value;
        var maybeSelectedGameObject = Selection.activeGameObject.opt();
        
        if (maybeSelectedGameObject.isSome && maybeSelectedGameObject == selectedGameObjectOpt) {
          backing.refreshInit(selectedFunTweenManager, rootSelectedNodeOpt);
        }
        else {
          backing.refreshInit(None._, None._);
        }
      }

      void selectNewFunTweenManager(int index) {
        saveCurrentVisualSettings();
        backing.refreshInit(ftms.get(index), None._);
        backing.Repaint();
      }

      static ImmutableArray<FunTweenManagerV2> getFunTweenManagers(Option<GameObject> gameObjectOpt) =>
        gameObjectOpt.fold(
          () => ImmutableArray<FunTweenManagerV2>.Empty,
          gameObject => gameObject.GetComponents<FunTweenManagerV2>().ToImmutableArray()
        );

      void manageAnimationPlayback(TweenPlaybackController.AnimationPlaybackEvent playbackEvent) {
        foreach (var controller in tweenPlaybackController) {
          controller.manageAnimation(playbackEvent);
        }
      }

      void manageCursorLine(bool isStart, float cursorTime) {
        foreach (var controller in tweenPlaybackController) {
          if (isStart) {
            controller.evaluateCursor(cursorTime);
          }
          else {
            controller.stopCursorEvaluation();
          }
        }
      }
      
    }
  }
}
#endif
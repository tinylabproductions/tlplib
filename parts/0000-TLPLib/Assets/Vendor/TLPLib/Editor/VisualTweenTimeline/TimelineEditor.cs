#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using pzd.lib.data;
using pzd.lib.exts;
using pzd.lib.functional;
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
      Drag,
      DeselectAll,
      RemoveSelected,
      SelectAll,
      Refresh
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

    readonly Dictionary<FunTweenManagerV2, TimelineVisuals.TimelineVisualsSettings>
      mappedSettings = new Dictionary<FunTweenManagerV2, TimelineVisuals.TimelineVisualsSettings>();

    public void OnGUI() => init?.onGUI(Event.current);

    public void OnEnable() {
      refreshInit(F.none_, F.none_);
    }

    void refreshInit(Option<FunTweenManagerV2> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet) {
      if (init == null) init = new Init(this, ftmToSetOpt, rootSelectedNodeToSet);
      else if (!init.isLocked.value) init = new Init(this, ftmToSetOpt, rootSelectedNodeToSet);
    }
   
    public void OnDisable() {
      if (init != null) {
        init.Dispose();
        init = null;
      }
    }
    
    void OnSelectionChange() {
      init?.saveCurrentVisualSettings();
      refreshInit(F.none_, F.none_);
    } 
    
    void OnLostFocus() => init?.onLostFocus();

    // Removed on purpose
    // void OnHierarchyChange() {
    //   var initOpt = F.opt(init);
    //   refreshInit(initOpt.flatMap(_ => _.selectedFunTweenManager), initOpt.flatMap(_ => _.rootSelectedNodeOpt));
    // }

    partial class Init : IDisposable {
      const int SNAPPING_POWER = 10;
      public readonly Ref<bool> isLocked = new SimpleRef<bool>(false);
      readonly ImmutableArray<FunTweenManagerV2> ftms;
      readonly TimelineEditor backing;
      readonly TimelineVisuals timelineVisuals;
      // readonly ExternalEditor advancedEditor;
      readonly Option<GameObject> selectedGameObjectOpt;
      readonly List<float> diffList = new List<float>();
      readonly List<TimelineNode> selectedNodesList = new List<TimelineNode>();
      readonly Ref<bool> visualizationMode = new SimpleRef<bool>(false);

      public Option<FunTweenManagerV2> selectedFunTweenManager { get; private set; }
      public Option<TimelineNode> rootSelectedNodeOpt { get; private set; }
      
      List<TimelineNode> funNodes = new List<TimelineNode>();
      bool isStartSnapped, isEndSnapped, resizeNodeStart, resizeNodeEnd, dragNode, snapping = true;
      Option<NodeSnappedTo> nodeSnappedToOpt;
      Option<TweenPlaybackController> tweenPlaybackController;
      float timeClickOffset;

      [MenuItem("TLP/TweenTimeline", false)]
      public static void ShowWindow() {
        var window = GetWindow<TimelineEditor>(false, "TweenTimeline");
        window.wantsMouseMove = true;
        DontDestroyOnLoad(window);
      }

      void OnPlaymodeStateChanged() { backing.OnEnable(); }

      public void onLostFocus() {
        foreach (var controller in tweenPlaybackController) {
          controller.stopVisualization();
        }
      }

      public Init(
        TimelineEditor backing, Option<FunTweenManagerV2> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet
      ) {
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
        Undo.undoRedoPerformed += undoCallback;
        EditorSceneManager.sceneSaving += EditorSceneManagerOnSceneSaving;

        // advancedEditor = CreateInstance<ExternalEditor>();
        // advancedEditor.Instances = new object[] { };
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
            // TODO
            // advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
            funNodes.Clear();
            importTimeline();
          }
        );

        backing.Repaint();
      }

      public void Dispose() { 
        EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
        Undo.undoRedoPerformed -= undoCallback;
        EditorSceneManager.sceneSaving -= EditorSceneManagerOnSceneSaving;
      }

      void EditorSceneManagerOnSceneSaving(Scene scene, string path) {
        funNodes.Clear();
        foreach (var controller in tweenPlaybackController) {
          controller.stopVisualization();
        }
      }

      public void onGUI(Event currentEvent) {
        
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
        selectedNodesList.Clear();
        importTimeline();
      }

      //Selects or deselects node
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

      void doNodeEvents(NodeEvents nodeEvent, Option<TimelineNode> timelineNodeOpt, float mousePositionSeconds) {
        var snappingEnabled = !Event.current.shift && snapping;
        //Cleaning up nodes list from nodes without element.element
        funNodes = funNodes.Where(funNode => funNode.element.element != null).ToList();
        
        switch (nodeEvent) {
          case NodeEvents.RemoveSelected:
            removeoAllSelectedNodes();
            selectedNodesList.Clear();
            importTimeline();
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
              genericMenu.AddItem(new GUIContent("Remove"), false, removeSelectedNode, timelineNode);
              genericMenu.AddItem(new GUIContent("Unselect"), false, deselect, timelineNode);
              genericMenu.ShowAsContext();
            }
            break;
          
          case NodeEvents.Drag:
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
    
              //Draging the node
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
  
                if (Event.current.mousePosition.y > rootSelected.channel * 20 + 25) {
                  foreach (var node in selectedNodesList) {
                    updateLinkedNodeChannels(node, _ => _.increaseChannel());
                    if (node == rootSelected) {
                      node.unlink();
                    }
                  }
                }
  
                if (Event.current.mousePosition.y < rootSelected.channel * 20 - 5
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
            nodeSnappedToOpt = F.none_;
            break;
        }
      }
      
      void moveDownIfOverlaping(TimelineNode timelineNode) {
        foreach (var overlapingNode in getOverlapingNodes(timelineNode)) {
          selectedNodesList.find(foundNode => foundNode == overlapingNode).voidFold(
            () => moveAndRecurse(timelineNode),
            moveAndRecurse
          );
        }
        void moveAndRecurse(TimelineNode node) {
          node.increaseChannel();
          moveDownIfOverlaping(node);
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
              rightNode.setStartTime(node.getEnd() + rightNode.element.timeOffset);
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
                : F.none_
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
          return F.none_;
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
                nodeSnappedToOpt = F.none_;
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
                nodeSnappedToOpt = F.none_;
                isStartSnapped = false;
              }
            }
          }
        );
      
      // creates nodeList from elements info
      void importTimeline() {
        // TODO:
        // advancedEditor.Instances = new object[] { };
        if (selectedFunTweenManager.valueOut(out var manager) && manager.timeline != null) {
          var elements = manager.serializedTimeline.elements;

          if (elements != null) {
            funNodes = manager.serializedTimeline.elements.Select(
              (element, idx) => {
                if (idx != 0 && element.startAt == Element.At.AfterLastElement
                ) {
                  element.timelineChannelIdx = elements[idx - 1].timelineChannelIdx;
                }

                var newNode = new TimelineNode(element, whereToStart(idx));

                return selectedNodesList
                  .find(selectedNode => selectedNode.element == element)
                  .fold(
                    () => newNode,
                    foundNode => {
                      selectedNodesList.Remove(foundNode);
                      selectedNodesList.Add(newNode);
                      return newNode;
                    }
                  );
              }
            ).ToList();
            foreach (var n in funNodes) moveDownIfOverlaping(n);
          }
          else {
            funNodes.Clear();
          }

          //Relinking linked nodes, since we dont serialize nodeLinkedTo
          foreach (var node in funNodes) {
            if (getLeftNode(node).valueOut(out var leftNode)) {
              if (node.element.startAt == Element.At.AfterLastElement) {
                node.linkTo(leftNode);
              }
            }

            node.refreshColor();
          }

          float whereToStart(int idx) {
            return idx == 0
              ? elements[idx].at(0, 0)
              : elements[idx].at(
                whereToStart(idx - 1),
                elements[idx - 1].element == null ? 0 : elements[idx - 1].element.duration
              );
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

      const float EPS = 15f;
      Option<TimelineNode> getOverlapingNodes(TimelineNode node) {
        var channelNodes = funNodes.Where(funNode => funNode.channel == node.channel && funNode != node);

        bool isOverlaping(TimelineNode channelNode, float nodePoint) =>
          channelNode.startTime < nodePoint && channelNode.getEnd() > nodePoint;

        return channelNodes.find(channelNode =>
          isOverlaping(channelNode, node.startTime)
          || isOverlaping(channelNode, node.getEnd())
          || isOverlaping(channelNode, (node.startTime + node.getEnd()) / 2)
          || node.isCallback && channelNode.isCallback
             && Math.Abs(node.startTime - channelNode.startTime) < timelineVisuals.GUIToSeconds(EPS)
        );

      }
      
      void exportTimelineToTweenManager() {
        if (selectedFunTweenManager.valueOut(out var manager) && !funNodes.isEmpty()) {
          EditorUtility.SetDirty(manager);
          Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "something changed");
          
          var arr = new List<TimelineNode>();
          for (var i = 0; i <= funNodes.Max(funNode => funNode.channel); i++) {
            arr.AddRange(
              funNodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
            );
          }
          
          manager.serializedTimeline.elements = arr.Select(elem => {
            elem.element.timelineChannelIdx = elem.channel;
            return elem.element;
          }).ToArray();

          foreach (var element in manager.serializedTimeline.elements) {
            foreach (var found in funNodes.find(funNode => funNode.element == element)) {
              if (element.element != null) {
                EditorUtility.SetDirty(manager);
                element.element.trySetDuration(found.duration);
                element.timelineChannelIdx = found.channel;
                
                if (found.linkedNode.valueOut(out var linked)) {
                  element.startAt = Element.At.AfterLastElement;
                  element.timeOffset = found.startTime - linked.getEnd();
                }
                else {
                  element.startAt = Element.At.SpecificTime;
                  element.timeOffset = found.startTime;
                }
              }
            }
          }
        }

        if (funNodes.isEmpty()) manager.serializedTimeline.elements = new Element[0];
      }

      void doNewSettings(SettingsEvents settingsEvent) {
          switch (settingsEvent) {
            case SettingsEvents.AddTween:
              var newElement = new Element {
                startAt = Element.At.SpecificTime
              };
              var newNode = new TimelineNode(newElement, 0);

              if (!funNodes.isEmpty()) {
                funNodes.Insert(0, newNode);
              }
              else {
                funNodes = new List<TimelineNode> {newNode};
              }

              selectedNodesList.Clear();
              selectedNodesList.Add(newNode);
              rootSelectedNodeOpt = newNode.some();

              exportTimelineToTweenManager();
              importTimeline();
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

      void deselect(object obj) {
        var node = obj as TimelineNode;
        selectedNodesList.Remove(node);
        backing.Repaint();
      }

      void removeSelectedNode(object obj) {
        var nawd = obj as TimelineNode;
        funNodes.Remove(nawd);
        foreach (var linkedNode in getLinkedRightNode(nawd, nawd)) linkedNode.unlink();

        exportTimelineToTweenManager();
        importTimeline();
        rootSelectedNodeOpt = F.none_;
      }

      void removeoAllSelectedNodes() {
        funNodes = funNodes.Where(node =>
          selectedNodesList.find(selectedNode => node == selectedNode).isNone).ToList();

        foreach (var selectedNode in selectedNodesList) {
          foreach (var linkedNode in getLinkedRightNode(selectedNode, selectedNode)) linkedNode.unlink();
        }

        exportTimelineToTweenManager();
        importTimeline();
        selectedNodesList.Clear();
        rootSelectedNodeOpt = F.none_;
      }

      void removeNodeIfHasNoElement(TimelineNode node) {
        if (node.element.element == null && !funNodes.isEmpty()) {
          funNodes.Remove(node);
          exportTimelineToTweenManager();
          importTimeline();
          rootSelectedNodeOpt = F.none_;
        }
      }

      void removeRootNodeIfHasNoElement() {
        foreach (var rootNode in rootSelectedNodeOpt) {
          removeNodeIfHasNoElement(rootNode);
        }
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
          backing.refreshInit(F.none_, F.none_);
        }
      }

      void selectNewFunTweenManager(int index) {
        saveCurrentVisualSettings();
        backing.refreshInit(ftms.get(index), F.none_);
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
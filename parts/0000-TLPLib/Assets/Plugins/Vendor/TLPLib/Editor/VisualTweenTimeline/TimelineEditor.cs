#if ADV_INS_CHANGES && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using Smooth.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences.SerializedTweenTimeline.Element;

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
    public partial struct NodeSnappedTo {
      public readonly TimelineNode node;
      public readonly SnapType snapType;
    }

    Init init;

    readonly Dictionary<FunTweenManager, TimelineVisuals.TimelineVisualsSettings>
      mappedSettings = new Dictionary<FunTweenManager, TimelineVisuals.TimelineVisualsSettings>();

    public void OnGUI() {
      if (init != null) init.onGUI(Event.current);
    }

    public void OnEnable() {
      refreshInit(F.none_, F.none_);
    }

    void refreshInit(Option<FunTweenManager> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet) {
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
      if (init != null) init.saveCurrentVisualSettings();
      refreshInit(F.none_, F.none_);
    } 
    
    void OnLostFocus() { if (init != null) init.onLostFocus(); }

    void OnHierarchyChange() {
      var initOpt = F.opt(init);
      refreshInit(initOpt.flatMap(_ => _.selectedFunTweenManager_), initOpt.flatMap(_ => _.rootSelectedNodeOpt_));
    }

    partial class Init : IDisposable {
      
      const int SNAPPING_POWER = 10;
      public readonly Ref<bool> isLocked = new SimpleRef<bool>(false);
      readonly ImmutableArray<FunTweenManager> ftms;
      readonly TimelineEditor backing;
      readonly TimelineVisuals timelineVisuals;
      readonly ExternalEditor advancedEditor;
      readonly Option<GameObject> selectedGameObjectOpt;
      readonly List<float> diffList = new List<float>();
      readonly List<TimelineNode> selectedNodesList = new List<TimelineNode>();
      readonly Ref<bool> visualizationMode = new SimpleRef<bool>(false);

      [PublicAccessor] Option<FunTweenManager> selectedFunTweenManager;
      [PublicAccessor] Option<TimelineNode> rootSelectedNodeOpt;
      
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
        TimelineEditor backing, Option<FunTweenManager> ftmToSetOpt, Option<TimelineNode> rootSelectedNodeToSet
      ) {
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
        Undo.undoRedoPerformed += undoCalback;
        EditorSceneManager.sceneSaving += EditorSceneManagerOnSceneSaving;

        advancedEditor = CreateInstance<ExternalEditor>();
        advancedEditor.Instances = new object[] { };
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
          visualizationMode, isLocked, advancedEditor, ftms,
          getSettings()
        );

        TimelineVisuals.TimelineVisualsSettings getSettings() {
          var idx = selectedFunTweenManager.isSome
            ? ftms.IndexOf(selectedFunTweenManager.get)
            : 0;

          return selectedFunTweenManager.flatMap(
              ftm => backing.mappedSettings.TryGet(ftm)
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
            advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
            funNodes.Clear();
            importTimeline();
          }
        );

     

        backing.Repaint();
      }

      public void Dispose() { 
        EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
        Undo.undoRedoPerformed -= undoCalback;
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

      void undoCalback() {
        selectedNodesList.Clear();
        importTimeline();
        refreshSelectedNodes();
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
  
                rootSelected.startTime = mousePositionSeconds;
                rootSelected.startTime = Mathf.Clamp(rootSelected.startTime, 0, float.MaxValue);
  
                if (rootSelected.startTime > 0 && !isStartSnapped) {
                  rootSelected.duration = selectedNodeEnd - rootSelected.startTime;
                  rootSelected.duration = Mathf.Clamp(rootSelected.duration, 0.01f, float.MaxValue);
                }
  
                if (snappingEnabled) {
                  snapStart(rootSelected, selectedNodeEnd);
                }
  
                foreach (var selected in selectedNodesList) {
                  if (selected != rootSelected) {
                    var nodeEnd = selected.getEnd();
                    selected.startTime = rootSelected.startTime;
                    selected.duration = nodeEnd - selected.startTime;
                    selected.startTime = Mathf.Clamp(selected.startTime, 0, float.MaxValue);
                    selected.duration = Mathf.Clamp(selected.duration, 0.01f, float.MaxValue);
                  }
                }
              }

              if (resizeNodeEnd) {
                if (rootSelected.startTime > mousePositionSeconds) break;
                
                rootSelected.duration = mousePositionSeconds - rootSelected.startTime;
                rootSelected.duration = Mathf.Clamp(rootSelected.duration, 0.01f, float.MaxValue);
  
                if (snappingEnabled) {
                  snapEnd(rootSelected);
                }
  
                foreach (var node in selectedNodesList) {
                  if (node != rootSelected) {
                    node.duration = rootSelected.duration - (node.startTime - rootSelected.startTime);
                    node.duration = Mathf.Clamp(node.duration, 0.01f, float.MaxValue);
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
  
                rootSelected.startTime = mousePositionSeconds + timeClickOffset;
                rootSelected.startTime = Mathf.Clamp(rootSelected.startTime, clampLimit, float.MaxValue);
  
                isEndSnapped = false;
                isStartSnapped = false;
  
                if (snappingEnabled) {
                  snapDrag(rootSelected, selectedNodesList);
                }
  
                //setting multiselected nodes starttimes
                for (var i = 0; i < selectedNodesList.Count; i++) {
  
                  var node = selectedNodesList[i];
                  node.startTime = rootSelected.startTime + diffList[i];
  
                  updateLinkedNodeStartTimes(node);
                  node.startTime = Mathf.Clamp(node.startTime, 0, float.MaxValue);
                }
  
                diffList.Clear();
  
                if (Event.current.mousePosition.y > rootSelected.channel * 20 + 25) {
                  foreach (var node in selectedNodesList) {
                    updateLinkedNodeChannels(node, _ => _.channel += 1);
                    if (node == rootSelected) {
                      node.unlink();
                    }
                  }
                }
  
                if (Event.current.mousePosition.y < rootSelected.channel * 20 - 5
                  && selectedNodesList.find(node => node.channel == 0).isNone) {
                  foreach (var node in selectedNodesList) {
                    updateLinkedNodeChannels(node, _ => _.channel -= 1);
                    if (node == rootSelected) {
                      node.unlink();
                    }
                  }
                }
  
                foreach (var node in selectedNodesList) {
                  node.channel = Mathf.Clamp(node.channel, 0, int.MaxValue);
                }
  
                void updateLinkedNodeChannels(TimelineNode node, Act<TimelineNode> changeChannel) {
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
            unlinkNodesWithBrokenLink();
            if (dragNode || resizeNodeEnd || resizeNodeStart) {
              foreach (var ftm in selectedFunTweenManager) {
                Undo.RegisterFullObjectHierarchyUndo(ftm, "Node changes");
                refreshSelectedNodes();
                exportTimelineToTweenManager();
                importTimeline();
                timelineVisuals.recalculateTimelineWidth(funNodes);
              }
            }
            
            isEndSnapped = false;
            isStartSnapped = false;
            dragNode = false;
            resizeNodeStart = false;
            resizeNodeEnd = false;
            nodeSnappedToOpt = F.none_;
            break;
        }
      }

      void moveDownIfOverlaping(TimelineNode node) {
        getOverlapingNodes(node).map(overlapingNode => {
          node.channel++;
          moveDownIfOverlaping(node);
          return node;
        });
      }

      void unlinkNodesWithBrokenLink() =>
        funNodes.ForEach(
          node => {
            if (
              node.linkedNode.valueOut(out var linkedNode)
              && getLeftNode(node).valueOut(out var leftNode)
              && linkedNode != leftNode
            ) {
              node.unlink();
            }
          }
        );

      void updateLinkedNodeStartTimes(TimelineNode node) =>
        getLinkedRightNode(node, node).voidFold(
          () => { },
          rightNode => {
            if (rightNode.linkedNode.valueOut(out var nodeLinkedTo) && nodeLinkedTo == node) {
              rightNode.startTime = node.getEnd() + rightNode.element.timeOffset;
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
          rootNode.startTime = timeToSet;
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

      void manageSnap(float selectedNodePoint, GetNodePoint nodePoint, CompareFloats cmpr, Act<TimelineNode> snap) =>
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
              selectedNode.duration = nodeToSnapTo.startTime - selectedNode.startTime;
              isEndSnapped = true;
              nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.EndWithStart).some();
            }
            else {
              snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
              if (isInRangeOfSnap(snapPoint, nodeEndPos)) {
                selectedNode.duration = nodeToSnapTo.getEnd() - selectedNode.startTime;
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
              selectedNode.startTime = nodeToSnapTo.startTime;
              if (!isStartSnapped) {
                selectedNode.duration = end - selectedNode.startTime;
                nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.StartWithStart).some();
                isStartSnapped = true;
              }
            }
            else {
              snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
              if (isInRangeOfSnap(snapPoint, nodeStartPos)) {
                selectedNode.startTime = nodeToSnapTo.getEnd();
                if (!isStartSnapped) {
                  selectedNode.duration = end - selectedNode.startTime;
                  nodeSnappedToOpt = new NodeSnappedTo(nodeToSnapTo, SnapType.StartWithEnd).some();
                  isStartSnapped = true;
                }
              }
              else if (isStartSnapped) {
                selectedNode.duration = initialEnd - selectedNode.startTime;
                nodeSnappedToOpt = F.none_;
                isStartSnapped = false;
              }
            }
          }
        );

      //creates nodeList from elements info
      void importTimeline() {
        advancedEditor.Instances = new object[] { };
        if (selectedFunTweenManager.valueOut(out var manager) && manager.timeline != null) {
          var elements = manager.timeline.elements;

          if (elements != null) {
            funNodes = manager.timeline.elements.Select(
              (element, idx) => {
                if (idx != 0 && element.startAt == Element.At.AfterLastElement
                ) {
                  element.timelineChannelIdx = elements[idx - 1].timelineChannelIdx;
                }

                if (element.title == "")
                  element.title =
                    element.element == null
                      ? ""
                      : element.element.GetType().Name;

                var newNode = new TimelineNode(element, whereToStart(idx), element.title);

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
            foreach (var n in selectedNodesList) moveDownIfOverlaping(n);
            foreach (var n in funNodes) moveDownIfOverlaping(n);
          }
          else {
            funNodes.Clear();
          }

          //Relinking linked nodes, since we dont serialize nodeLinkedTo
          foreach (var node in funNodes) {
            if (getLeftNode(node).valueOut(out var leftNode)) {
              node.reLink(leftNode);
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

      void refreshSelectedNodes() => selectedNodesList.ForEach(
        selected =>
          getLeftNode(selected).voidFold(
            () => selected.convert(Element.At.SpecificTime),
            leftNode => {
              if (selected.linkedNode.valueOut(out var linkedNode) && linkedNode == leftNode) {
                selected.convert(Element.At.AfterLastElement);
              }
              else {
                selected.convert(Element.At.SpecificTime);
              }
            }
          )
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
          var arr = new List<TimelineNode>();
          for (var i = 0; i <= funNodes.Max(x => x.channel); i++) {
            arr.AddRange(
              funNodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
            );
          }

          manager.timeline.elements = arr.Select(elem => {
            elem.element.timelineChannelIdx = elem.channel;
            return elem.element;
          }).ToArray();
        }

        if (funNodes.isEmpty()) manager.timeline.elements = new Element[0];
      }

      void doNewSettings(SettingsEvents settingsEvent) {
          switch (settingsEvent) {
            case SettingsEvents.AddTween:
              var newElement = new Element {
                startAt = Element.At.SpecificTime
              };
              var newNode = new TimelineNode(newElement, 0, "");

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
                  selectedNode.link(leftNode);
                  selectedNode.convert(Element.At.AfterLastElement);
                }
                
              }
              break;
            case SettingsEvents.Unlink:
              foreach (var selectedNode in rootSelectedNodeOpt) {
                if (selectedFunTweenManager.valueOut(out var ftm)) {
                  Undo.RegisterFullObjectHierarchyUndo(ftm, "unlinked Nodes");
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
        selectedFunTweenManager = Undo.AddComponent<FunTweenManager>(gameObject).some();
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

      static ImmutableArray<FunTweenManager> getFunTweenManagers(Option<GameObject> gameObjectOpt) =>
        gameObjectOpt.fold(
          () => ImmutableArray<FunTweenManager>.Empty,
          gameObject => gameObject.GetComponents<FunTweenManager>().ToImmutableArray()
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
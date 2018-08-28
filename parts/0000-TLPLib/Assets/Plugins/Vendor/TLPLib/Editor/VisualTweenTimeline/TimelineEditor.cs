#if ADV_INS_CHANGES
using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences.SerializedTweenTimeline.Element;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineEditor : EditorWindow {

	  enum snapType : byte { StartWithStart, StartWithEnd, EndWithEnd, EndWithStart }
	  
	  [Record]
	  public partial struct nodeSnappedTo {
		  public readonly TimelineNode node;
		  public readonly bool snappedToStart;
	  }

	  [PublicAccessor] List<TimelineNode> _funNodes = new List<TimelineNode>();
	  [PublicAccessor] Option<FunTweenManager> _funTweenManager = F.none_;
	  [PublicAccessor] bool _isStartSnapped, _isEndSnapped;
	  [PublicAccessor] Option<TimelineNode> _rootSelectedNodeOpt = F.none_;
	  [PublicAccessor] Option<nodeSnappedTo> _nodeSnappedToOpt;

	  const int SNAPPING_POWER = 10;
	  float timeClickOffset;
	  bool resizeNodeStart, resizeNodeEnd, dragNode, snapping;
		Vector2 settingsScroll;
	  TimelineVisuals timelineVisuals;
	  ExternalEditor advancedEditor;
	  Option<GameObject> selectedGameObjectOpt;
	  readonly List<float> diffList = new List<float>();
	  public readonly List<TimelineNode> selectedNodesList = new List<TimelineNode>();

		[MenuItem("TLP/TweenTimeline", false)]
		public static void ShowWindow() {
			var window = GetWindow<TimelineEditor>(false, "TweenTimeline");
			window.wantsMouseMove = true;
			DontDestroyOnLoad(window);
		}
	 
	  void OnPlaymodeStateChanged() { OnSelectionChange(); }

	  void OnLostFocus() { timelineVisuals.stopVisualization(); }

	  void OnDisable() {
		  EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
		  Undo.undoRedoPerformed -= undoCalback;
		  EditorSceneManager.sceneSaving -= EditorSceneManagerOnSceneSaving;
	  }

	  void OnEnable() {
		  EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
			Undo.undoRedoPerformed += undoCalback;
		  EditorSceneManager.sceneSaving += EditorSceneManagerOnSceneSaving;
			
			timelineVisuals = new TimelineVisuals(this);
			
			_isStartSnapped = false;
		  _isEndSnapped = false;
		  _funNodes.Clear();

			advancedEditor = CreateInstance<ExternalEditor>();
			_funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
			
			if (funTweenManager.isSome)       { importTimeline();    }
		  if (selectedGameObjectOpt.isNone) { OnSelectionChange(); }
		}

	  void EditorSceneManagerOnSceneSaving(Scene scene, string path) {
		  _funNodes.Clear();
		  timelineVisuals.stopVisualization();
	  }

	  void OnSelectionChange() {
		  selectedGameObjectOpt = Selection.activeGameObject.opt();
			_funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
		  
			funTweenManager.voidFold(
				() => _funNodes.Clear(),
				manager => {
					timelineVisuals = new TimelineVisuals(this);
					advancedEditor = CreateInstance<ExternalEditor>();
					advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
					_funNodes.Clear();
					importTimeline();
				}
			);

		  _rootSelectedNodeOpt = F.none_;
			Repaint();
		}
	  
	  void OnGUI() {
			GUI.enabled = selectedGameObjectOpt.isSome;

		  timelineVisuals.doTimeline(new Rect(0, 0, position.width, position.height), funTweenManager);

		  if (GUI.changed) {
			  importTimeline();
		  }
		  Repaint();
	  }

	  void undoCalback() {
		  selectedNodesList.Clear();
		  importTimeline();
		  refreshChannelNodes();
	  }

	  void manageSelectedNode(TimelineNode nodeToAdd, Event currentEvent) {
		  if (!selectedNodesList.isEmpty()) {
				selectedNodesList.find(selectedNode => selectedNode == nodeToAdd).voidFold(
				  () => {
					  if ( currentEvent.control ) {
						  selectedNodesList.Add(nodeToAdd);
					  }
					  else if ( selectedNodesList.Count <= 1 ) {
						  selectedNodesList.Clear();
						  selectedNodesList.Add(nodeToAdd);
					  }
				  },
				  selectedNode => {
					  if ( currentEvent.control ) {
					  	selectedNodesList.Remove(nodeToAdd);
				  	}
				  }
				);
		  }
		  else if ( selectedNodesList.Count <= 1 ) {
			  selectedNodesList.Clear();
			  selectedNodesList.Add(nodeToAdd);
		  }
	  }
		  
		public void doEvents() {
			var currEvent = Event.current;
			var snappingEnabled = !Event.current.shift && snapping;
			if (currEvent.control && currEvent.keyCode == KeyCode.A && currEvent.type == EventType.keyUp) {
				selectedNodesList.Clear();
					foreach (var node in _funNodes) {
						selectedNodesList.Add(node);
				}
			}

			if (currEvent.keyCode == KeyCode.Delete && currEvent.type == EventType.keyDown) {
				removeoAllSelectedNodes();
				selectedNodesList.Clear();
				importTimeline();
			}

			switch (currEvent.rawType) {
				case EventType.MouseDown:
					//Cleaning up nodes list from nodes without element.element
					_funNodes = _funNodes.Where(node => node.element.element != null).ToList();
					
						foreach (var node in _funNodes) {
							if (new Rect(timelineVisuals.secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20)
								.Contains(Event.current.mousePosition) && !node.isCallback
								) {
								foreach (var rootSelectedNode in _rootSelectedNodeOpt) {
									removeNodeIfHasNoElement(rootSelectedNode);
								}
								manageSelectedNode(node, Event.current);
								_rootSelectedNodeOpt = node.some();
								resizeNodeStart = true;
								EditorGUI.FocusTextInControl("");
								currEvent.Use();
							}

							if (new Rect(timelineVisuals.secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20)
								.Contains(
									Event.current.mousePosition) && !node.isCallback) {
								foreach (var rootSelectedNode in _rootSelectedNodeOpt) {
									removeNodeIfHasNoElement(rootSelectedNode);
								}
								manageSelectedNode(node, Event.current);
								_rootSelectedNodeOpt = node.some();
								resizeNodeEnd = true;
								EditorGUI.FocusTextInControl("");
								currEvent.Use();
							}

							if ( new Rect(
								timelineVisuals.secondsToGUI(node.startTime - (node.isCallback ? 0.5f : 0)),
								node.channel * 20,
								timelineVisuals.secondsToGUI(node.isCallback ? 1 : node.duration), 20
							).Contains(Event.current.mousePosition)) {

								switch (currEvent.button) {
									case 0:
										foreach (var rootSelectedNode in _rootSelectedNodeOpt) {
											removeNodeIfHasNoElement(rootSelectedNode);
										}
										timeClickOffset = node.startTime - timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
										dragNode = true;
										_rootSelectedNodeOpt = node.some();
										manageSelectedNode(node, Event.current);
										EditorGUI.FocusTextInControl("");
										break;
									case 1:
										_rootSelectedNodeOpt = node.some();
										var genericMenu = new GenericMenu();
										genericMenu.AddItem(new GUIContent("Remove"), false, removeSelectedNode, node);
										genericMenu.AddItem(new GUIContent("Unselect"), false, deselect, node);
										genericMenu.ShowAsContext();
										break;
								}

								currEvent.Use();
							}
						}
					
					//Deselect by clicking away
					if (!dragNode && !resizeNodeStart && !resizeNodeEnd && !currEvent.control
						&& timelineVisuals.drawRect.Contains(Event.current.mousePosition)
					) {
						selectedNodesList.Clear();
						currEvent.Use();
					}

					break;
				case EventType.MouseDrag:

					if (!selectedNodesList.isEmpty()) {
						foreach (var selectedNode in selectedNodesList) {

							if (resizeNodeStart) {
								if (_rootSelectedNodeOpt.valueOut(out var rootSelected) && selectedNode == rootSelected) {
									var selectedNodeEnd = selectedNode.getEnd();
									
									selectedNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
									selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, 0, float.MaxValue);
									
									if (selectedNode.startTime > 0 && !_isStartSnapped) {
										selectedNode.duration = selectedNodeEnd - selectedNode.startTime;
										selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);
									}

									if (snappingEnabled) { snapStart(selectedNode, selectedNodeEnd); }

									foreach (var node in selectedNodesList) {
										if (node != rootSelected) {
											var nodeEnd = node.getEnd();
											node.startTime = selectedNode.startTime;
											node.duration = nodeEnd - node.startTime;
											node.startTime = Mathf.Clamp(node.startTime, 0, float.MaxValue);
											node.duration = Mathf.Clamp(node.duration, 0.01f, float.MaxValue);
										}
									}

									currEvent.Use();
								}
							}

							if (resizeNodeEnd) {
								if (_rootSelectedNodeOpt.valueOut(out var rootSelected) && selectedNode == rootSelected) {
									
									selectedNode.duration =
										timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) - selectedNode.startTime;
									selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);

									if (snappingEnabled) { snapEnd(selectedNode); }

									foreach (var node in selectedNodesList) {
										if (node != rootSelected) {
											node.duration = rootSelected.duration - (node.startTime - rootSelected.startTime);
											node.duration = Mathf.Clamp(node.duration, 0.01f, float.MaxValue);
										}
										updateLinkedNodeStartTimes(node);
									}

									timelineVisuals.recalculateTimelineWidth(_funNodes.opt());

									currEvent.Use();
								}
							}

							//Draging the node
							if (dragNode && !resizeNodeStart && !resizeNodeEnd || resizeNodeEnd && resizeNodeStart) {
								if (_rootSelectedNodeOpt.valueOut(out var rootSelected) && selectedNode == rootSelected) {

									
									foreach (var selected in selectedNodesList) {
										diffList.Add(selected.startTime - rootSelected.startTime);
									}

									var clampLimit =
										selectedNodesList.find(node => node.startTime <= 0).isSome
											? selectedNode.startTime
											: 0;

									selectedNode.startTime =
										timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) + timeClickOffset;
									selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, clampLimit, float.MaxValue);

									_isEndSnapped = false;
									_isStartSnapped = false;

									if (snappingEnabled) {
										snapDrag(selectedNode, selectedNodesList);
									}

									//setting multiselected nodes starttimes
									for (var i = 0; i < selectedNodesList.Count; i++) {

										var node = selectedNodesList[i];
										node.startTime = rootSelected.startTime + diffList[i];

										updateLinkedNodeStartTimes(node);
										node.startTime = Mathf.Clamp(node.startTime, 0, float.MaxValue);
									}
									diffList.Clear();

									if (Event.current.mousePosition.y > selectedNode.channel * 20 + 25) {
										foreach (var node in selectedNodesList) {
											updateLinkedNodeChannels(node, _ => _.channel += 1);
											if (node == rootSelected) {
												node.unlink();
											}
										}
									}

									if (Event.current.mousePosition.y < selectedNode.channel * 20 - 5
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
										getLinkedRightNode(node).voidFold(
											() => { },
											rightNode => { updateLinkedNodeChannels(rightNode, changeChannel); });
										changeChannel(node);
									}
								}

								currEvent.Use();
							}
						}
					}
					break;
				
				case EventType.MouseUp:
					unlinkNodesWithBrokenLink();

					if (dragNode || resizeNodeEnd || resizeNodeStart) {
						foreach (var ftm in funTweenManager) {
							Undo.RegisterFullObjectHierarchyUndo(ftm, "Node changes");
							timelineVisuals.recalculateTimelineWidth(_funNodes.opt());
							refreshChannelNodes();
							exportTimelineToTweenManager();
							importTimeline();
						}
					}

					_isEndSnapped = false;
					_isStartSnapped = false;
					dragNode = false;
					resizeNodeStart = false;
					resizeNodeEnd = false;
					_nodeSnappedToOpt = F.none_;
					break;
				
				default: break;
			}
		}

	  void unlinkNodesWithBrokenLink() {
			  foreach (var node in _funNodes) {
				  if (node.linkedNode.valueOut(out var linkedNode)
					  && getLeftNode(node).valueOut(out var leftNode)
					  && linkedNode != leftNode
				  ) {
					  node.unlink();
				  }
			  }
	  }
	  
	  void updateLinkedNodeStartTimes(TimelineNode node) {
		  getLinkedRightNode(node).voidFold(
			  () => { },
			  rightNode => {
				  if (rightNode.linkedNode.valueOut(out var nodeLinkedTo) && nodeLinkedTo == node)  {
					  rightNode.startTime = node.getEnd() + rightNode.element.timeOffset;
				  }
				  updateLinkedNodeStartTimes(rightNode);
			  }
			);
	  }
	  
	  Option<TimelineNode> getLinkedRightNode(TimelineNode node) =>
		  getRightNode(node).flatMap(rightNode =>
			  rightNode.linkedNode.valueOut(out var linkedNode)
				&& linkedNode == node
			  && selectedNodesList.find(x => x == rightNode).isNone
				  ? rightNode.some()
				  : F.none_
		  );

	  void snapDrag(TimelineNode rootNode, List<TimelineNode> selectedNodes) {
				var nonSelectedNodes = _funNodes.Except(selectedNodes).ToList();
		  
				nonSelectedNodes.ForEach(earlierNode => _nodeSnappedToOpt.voidFold(
						() => snap(earlierNode),
						nodeSnapped => snap(nodeSnapped.node)
					));
		  
				void snap(TimelineNode nodeToSnapTo) {
					_nodeSnappedToOpt =	getSnapType(nodeToSnapTo).map(
						snapType => {
							switch (snapType) {
								case snapType.StartWithStart:
									return setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime, ref _isStartSnapped);
								case snapType.StartWithEnd:
									return setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd(), ref _isStartSnapped);
								case snapType.EndWithStart:
									return setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime - rootNode.duration, ref _isEndSnapped);
								case snapType.EndWithEnd:
									return setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd() - rootNode.duration, ref _isEndSnapped);
								default:
									throw new ArgumentOutOfRangeException(nameof(snapType), snapType, null);
							}
						}
					);
				}

		  nodeSnappedTo setSnapping(
				TimelineNode nodeToSnapTo, bool snappedToNodeStart, float timeToSet, ref bool sideToSnap
				) {
					rootNode.startTime = timeToSet;
					sideToSnap = true;
					return new nodeSnappedTo(nodeToSnapTo, snappedToNodeStart);
			}

		  Option<snapType> getSnapType(TimelineNode nodeSnapTo) {
			  var nodeStart = timelineVisuals.secondsToGUI(nodeSnapTo.startTime);
			  var nodeEnd = timelineVisuals.secondsToGUI(nodeSnapTo.getEnd());
			  var dragNodeStart = timelineVisuals.secondsToGUI(rootNode.startTime);
			  var dragNodeEnd = timelineVisuals.secondsToGUI(rootNode.getEnd());
			  
			  if (isInRangeOfSnap(nodeStart, dragNodeStart)) return snapType.StartWithStart.some();
			  if (isInRangeOfSnap(nodeEnd, dragNodeStart)) return snapType.StartWithEnd.some();
			  if (isInRangeOfSnap(nodeStart, dragNodeEnd)) return snapType.EndWithStart.some();
			  if (isInRangeOfSnap(nodeEnd, dragNodeEnd)) return snapType.EndWithEnd.some();
			  return F.none_;
		  }
	  }
	  
	  delegate float getNodePoint(TimelineNode node);
	  delegate bool compare(float a, float b);

	  void manageSnap(float selectedNodePoint, getNodePoint nodePoint, compare cmpr, Act<TimelineNode> fn) =>
		  _funNodes
			  .Except(selectedNodesList)
			  .Where(node => cmpr( selectedNodePoint, nodePoint(node) ))
			  .ToList()
			  .ForEach(earlierNode => _nodeSnappedToOpt.voidFold(
				  () => fn(earlierNode),
				  nodeSnapped => fn(nodeSnapped.node)
			  ));

	  public bool isInRangeOfSnap(float snapPoint, float positionToCheck) =>
		  positionToCheck < snapPoint + SNAPPING_POWER && positionToCheck > snapPoint - SNAPPING_POWER;

	  void snapEnd(TimelineNode selectedNode) {
		  
		  manageSnap(selectedNode.startTime, node => node.getEnd(), (a, b) => a <= b,
			  nodeToSnapTo => {
				  var snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.startTime);
				  var nodeEndPos = timelineVisuals.secondsToGUI(selectedNode.getEnd());
				  if ( isInRangeOfSnap(snapPoint, nodeEndPos) ) {
					  selectedNode.duration = nodeToSnapTo.startTime - selectedNode.startTime;
					  _isEndSnapped = true;
					  _nodeSnappedToOpt = new nodeSnappedTo(nodeToSnapTo, true).some();
				  }
				  else {
					  snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
					  if ( isInRangeOfSnap(snapPoint, nodeEndPos) ) {
						  selectedNode.duration = nodeToSnapTo.getEnd() - selectedNode.startTime;
						  _isEndSnapped = true;
						  _nodeSnappedToOpt = new nodeSnappedTo(nodeToSnapTo, false).some();
					  }
					  else {
						  _isEndSnapped = false;
						  _nodeSnappedToOpt = F.none_;
					  }
				  }
			  }
			);
	 	}

	  void snapStart(TimelineNode selectedNode, float initialEnd) {
		  
		  manageSnap(selectedNode.getEnd(), node => node.startTime, (a, b) => a >= b,
			  nodeToSnapTo => {
				  var end = selectedNode.getEnd();
				  var snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.startTime);
				  var nodeStartPos = timelineVisuals.secondsToGUI(selectedNode.startTime);
				
				  if ( isInRangeOfSnap(snapPoint, nodeStartPos) ) {
					  selectedNode.startTime = nodeToSnapTo.startTime;
					  if (!_isStartSnapped) {
						  selectedNode.duration = end - selectedNode.startTime;
						  _nodeSnappedToOpt = new nodeSnappedTo(nodeToSnapTo, true).some();
						  _isStartSnapped = true;
					  }
				  }
				  else {
					  snapPoint = timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd());
					  if ( isInRangeOfSnap(snapPoint, nodeStartPos) ) {
						  selectedNode.startTime = nodeToSnapTo.getEnd();
						  if (!_isStartSnapped) {
							  selectedNode.duration = end - selectedNode.startTime;
							  _nodeSnappedToOpt = new nodeSnappedTo(nodeToSnapTo, false).some();
							  _isStartSnapped = true;
						  }
					  }
					  else if (_isStartSnapped) {
						  selectedNode.duration = initialEnd - selectedNode.startTime;
						  _nodeSnappedToOpt = F.none_;
						  _isStartSnapped = false;
					  }
				  }
			  }
			);
	  }

	  //creates nodeList from elements info
	  void importTimeline() {
		  advancedEditor.Instances = new object[] { };
		  if (funTweenManager.valueOut(out var manager) && manager.timeline != null) {
			  var elements = manager.timeline.elements;

			  _funNodes = manager.timeline.elements.Select(
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

			  //Relinking linked nodes, since we dont serialize nodeLinkedTo
					foreach (var node in _funNodes) {
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

	  public Option<TimelineNode> getLeftNode(TimelineNode selectedNode) =>
		  _funNodes.Where(node => node.channel == selectedNode.channel
			  && node.startTime < selectedNode.startTime
		  ).ToList().noneIfEmpty().map(
			  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).Last()
		  );

	  Option<TimelineNode> getRightNode(TimelineNode selectedNode) =>
		  _funNodes.Where(node => node.channel == selectedNode.channel
			  && node.startTime > selectedNode.getEnd()
		  ).ToList().noneIfEmpty().map(
			  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).First()
		  );

	  void refreshChannelNodes() {
		  if (!selectedNodesList.isEmpty()) {
			  foreach (var selected in selectedNodesList) {
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
					);
			  }
		  }
	  }
	  
	  void exportTimelineToTweenManager() {
		  if (funTweenManager.valueOut(out var manager) && !_funNodes.isEmpty()) {
			  var arr = new List<TimelineNode>();
			  for (var i = 0; i <= _funNodes.Max(x => x.channel); i++) {
				  arr.AddRange(
					  _funNodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
				  );
			  }
			  manager.timeline.elements = arr.Select(elem => {
				  elem.element.timelineChannelIdx = elem.channel;
				  return elem.element;
			  }).ToArray();
		  }

		  if (_funNodes.isEmpty()) manager.timeline.elements = new Element[0];
	  }
	  
	  public void doSettings(float width, bool isVisualisation) {
		  if (funTweenManager.isSome) {
			  GUILayout.BeginVertical();
			  GUI.enabled = !isVisualisation;
			  
				  _funNodes.find(elem => elem.element.element == null).map(_ => GUI.enabled = false);

			  if (GUILayout.Button("Add Tween")) {
				  var newElement = new Element {
					  startAt = Element.At.SpecificTime
				  };
				  var newNode = new TimelineNode(newElement, 0, "");
				  
				  if (!_funNodes.isEmpty()) {
					  _funNodes.Insert(0, newNode);
				  }
				  else {
					  _funNodes = new List<TimelineNode>{newNode};
				  }
				  
				  selectedNodesList.Clear();
				  selectedNodesList.Add(newNode);
				  _rootSelectedNodeOpt = newNode.some();

				  exportTimelineToTweenManager();
				  importTimeline();
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
					  removeoAllSelectedNodes();
				  }

				  GUI.backgroundColor = snapping ? new Color(0, 1, 0, 0.5f) : Color.white;
				  if (GUILayout.Button(snapping ? "Snapping ON" : "Snapping OFF")) {
					  snapping = !snapping;
				  }
				  GUI.backgroundColor = Color.white;
				  
				  if (GUILayout.Button("DEBUG BUTTON")) {

					  if (!selectedNodesList.isEmpty()) {
						  foreach (var selected in selectedNodesList) {
							  Log.d.warn($"{selected.name} linkedTo {selected.linkedNode.get.name}");
						  }
					  }
				  }
				  
				  GUILayout.EndHorizontal();

				  if (oneNodeSelected && _rootSelectedNodeOpt.valueOut(out var selectedNode)) {
					  var linkButtonText = "LINK";
					  var unlinkButtonText = "UNLINK";
					  GUILayout.BeginHorizontal();
					  
					  if (selectedNode.linkedNode.isNone) {
						  GUI.enabled = false;
						  unlinkButtonText = "UNLINKED";
					  }

					  if (GUILayout.Button(unlinkButtonText)) {
						  selectedNode.unlink();
					  }
					  GUI.enabled = guiEnabled;
					  
					  if (selectedNode.linkedNode.isSome) {
						  GUI.enabled = false;
						  linkButtonText = "LINKED";
					  }

					  if (GUILayout.Button(linkButtonText)) {
						  if (getLeftNode(selectedNode).valueOut(out var leftNode)) {
							  selectedNode.link(leftNode);
							  selectedNode.convert(Element.At.AfterLastElement);
						  }
					  }

					  GUI.enabled = guiEnabled;
					  GUILayout.EndHorizontal();
				  }

			  }
			}
		  
		  if (funTweenManager.isNone) {
			  GUI.enabled = selectedGameObjectOpt.isSome;
			  if (GUILayout.Button("[Add manager]")) {
				  addFunTweenManagerComponent(Selection.activeGameObject);
				  EditorGUIUtility.ExitGUI();
			  }
		  }
		  
			GUILayout.Space(10);
		  GUI.enabled = !isVisualisation;
		  settingsScroll = GUILayout.BeginScrollView(settingsScroll);
		  if (funTweenManager.valueOut(out var ftm) && oneNodeSelected && !isVisualisation) {
			  drawElementSettings(ftm);
			  if (advancedEditor.Instances.Length > 0
				  && advancedEditor.Draw(new Rect(0, 0, width, position.height - 100))
			  	) {
				  Undo.RecordObject(ftm, "Tween Manager Changes");
				  Repaint(); 
			  }
		  }
		  
		  GUILayout.EndScrollView();
	  }

		void drawElementSettings(FunTweenManager manager) {
			foreach (var rootSelectedObject in _rootSelectedNodeOpt) {
				if (manager.timeline.elements != null
					&& advancedEditor.Instances.isEmpty()
					|| advancedEditor.Instances[0] != rootSelectedObject.element
					) { 
					advancedEditor.Instances = new object[] { rootSelectedObject.element };
				}
			}
		}

	  void deselect(object obj) {
		  var node = obj as TimelineNode;
		  selectedNodesList.Remove(node);
		  Repaint();
	  }
	  
		void removeSelectedNode(object obj) {
			var nawd = obj as TimelineNode;
			_funNodes = _funNodes.Where(node => node != nawd).ToList();
			
			exportTimelineToTweenManager();
			importTimeline();
			_rootSelectedNodeOpt = F.none_;
		}

	  void removeoAllSelectedNodes() {
			_funNodes = _funNodes.Where( node => 
				selectedNodesList.find(selectedNode => node == selectedNode).isNone).ToList();
			exportTimelineToTweenManager();
		  importTimeline();
			selectedNodesList.Clear();
		  _rootSelectedNodeOpt = F.none_;
	  }

	  void removeNodeIfHasNoElement(TimelineNode node) {
		  if (node.element.element == null && !_funNodes.isEmpty()) {
			  _funNodes.Remove(node);
			  exportTimelineToTweenManager();
			  importTimeline();
			  _rootSelectedNodeOpt = F.none_;
		  }
	  }

	  void addFunTweenManagerComponent(GameObject gameObject) {
		  gameObject.GetComponent<FunTweenManager>().opt().voidFold(
			  () => {
				  _funTweenManager = Undo.AddComponent<FunTweenManager>(gameObject).some();
				  importTimeline();
				  EditorUtility.SetDirty(gameObject);
			  },
			  fun => _funTweenManager = fun.some()
			);
	  }
  }
}
#endif
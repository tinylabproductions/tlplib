using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using GenerationAttributes;
using Harmony;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineEditor : EditorWindow {
    Timeline timelineVisuals;
	  ExternalEditor advancedEditor;
	  
	  bool isStartSnapped;
	  bool isEndSnapped;
	  Option<GameObject> selectedGameObjectOpt;
	  
	  [Record]
	  partial struct nodeSnappedTo {
		  public readonly FunSequenceNode node;
		  public readonly bool snappedToStart;
	  }

	  Option<nodeSnappedTo> nodeSnappedToOpt;

	  readonly List<FunSequenceNode> selectedNodesList = new List<FunSequenceNode>();

		Option<FunTweenManager> funTweenManager = F.none_;
	  Option<List<FunSequenceNode>> _funNodes;
	  Option<List<FunSequenceNode>> funNodes {
		  get => _funNodes;
		  set => _funNodes = value.valueOut(out var nodesList) && nodesList.nonEmpty() ? value : F.none_;
	  }
	  
	  Option<FunSequenceNode> rootSelectedNode => funNodes.flatMap(nodes =>
		  nodes.Count > selectedNodeIndex
			  ? nodes[selectedNodeIndex].some()
			  : F.none_
	  );

		int selectedNodeIndex;
	  const int SNAPPING_POWER = 10, OUTLINE_WIDTH = 3;
	  float settingsWidth, timeClickOffset;
	  bool resizeNodeStart, resizeNodeEnd, dragNode, snapping;
		Vector2 settingsScroll;
	  GameObject backupGameObject;

		[MenuItem("TLP/VisualTweenTimeline", false)]
		public static void ShowWindow() {
			var window = GetWindow<TimelineEditor>(false, "VisualTweenTimeline");
			window.wantsMouseMove = true;
			DontDestroyOnLoad(window);
		}
	  
		void OnEnable() {
			Undo.undoRedoPerformed += undoCalback;
			if (timelineVisuals == null) {
				timelineVisuals = new Timeline();
			}
			
			isStartSnapped = false;
			funNodes = F.none_;

			advancedEditor = CreateInstance<ExternalEditor>();
			funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
			
			if (funTweenManager.isSome) importTimeline();
			timelineVisuals.onSettingsGUI = onSettings;
			timelineVisuals.onTimelineGUI = drawFunNodes;
			if (selectedGameObjectOpt.isNone)
				OnSelectionChange();
		}
	  
		void OnSelectionChange() {
			if (timelineVisuals == null) {
				timelineVisuals = new Timeline();
			}
			selectedGameObjectOpt = Selection.activeGameObject.opt();

			funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
			
			funTweenManager.voidFold(
				() => funNodes = F.none_,
				manager  => {
					advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
					importTimeline();
				});
			
			selectedNodeIndex = 0;
			Repaint();
		}
	  
	  void OnGUI() {
			GUI.enabled = selectedGameObjectOpt.isSome;

		  timelineVisuals.DoTimeline(new Rect(0, 0, position.width, position.height), funTweenManager);

		  if (GUI.changed) {
			  importTimeline();
		  }
		  Repaint();
	  }

	  void undoCalback() {
		  importTimeline();
		  refreshChannelNodes();
		  Log.d.warn("undo/redo perfromed");
	  }

	  void drawFunNodes(Rect position) {
		  if (funNodes.valueOut(out var nodes) && funTweenManager.isSome) {
			  Option<Rect> snapIndicatorOpt = F.none_;
			  var indicatorColor = Color.green;
			  
			  foreach (var node in nodes) {
				  EditorGUIUtility.AddCursorRect(
					  new Rect(timelineVisuals.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20),
					  MouseCursor.ResizeHorizontal);
				  EditorGUIUtility.AddCursorRect(
					  new Rect(timelineVisuals.SecondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20),
					  MouseCursor.ResizeHorizontal);
				  EditorGUIUtility.AddCursorRect(
					  new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20,
						  timelineVisuals.SecondsToGUI(node.duration), 20),
					  MouseCursor.Pan);

					  var boxRect = new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20,
						  timelineVisuals.SecondsToGUI(node.duration), 20);

				  var currentNodeIsSelected = !selectedNodesList.isEmpty() &&
					  selectedNodesList.find(x => x == node).isSome;
				  
					  if (!selectedNodesList.isEmpty() &&
						  selectedNodesList.find(x => x == node).valueOut(out var selectedNode)
						  ) {
						  EditorGUI.DrawRect(boxRect, Color.magenta);
						  GUI.Box(new Rect(
							  boxRect.x + OUTLINE_WIDTH,
							  boxRect.y + OUTLINE_WIDTH,
							  boxRect.width - OUTLINE_WIDTH * 2,
							  boxRect.height - OUTLINE_WIDTH * 2
						  ), "", "TL LogicBar 0");

						  if (isEndSnapped || isStartSnapped) {
							  foreach (var nodeSnappedTo in nodeSnappedToOpt) {
								  var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
								  var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * 20;
								  snapIndicatorOpt =
									  selectedIsHigher
										  ? getIndicatorRect(selectedNode, isStartSnapped, distance).some()
										  : getIndicatorRect(nodeSnappedTo.node, nodeSnappedTo.snappedToStart, distance).some();
								  
								  Rect getIndicatorRect(FunSequenceNode nawd, bool isSnappedToStart, float dist) =>
									  new Rect(timelineVisuals.SecondsToGUI(
											  isSnappedToStart
												  ? nawd.startTime
												  : nawd.getEnd()),
										  nawd.channel * 20 - 10, 2, dist
									  );
							  }
							  
							  indicatorColor = isEndSnapped ? Color.yellow: Color.cyan;
						  }

					  }
					  else {
						  GUI.Box(boxRect, "", "TL LogicBar 0");
					  }

				  var style = new GUIStyle("Label");
				  style.fontSize = currentNodeIsSelected ? 12 : style.fontSize;
				  style.fontStyle = FontStyle.Bold;
				  var color = currentNodeIsSelected ? Color.magenta : Color.white;
				  color.a = currentNodeIsSelected ? 1.0f : 0.7f;
				  style.normal.textColor = color;
				  Vector3 size = style.CalcSize(new GUIContent($"content: {node.name}"));
				  var rect1 = new Rect(boxRect.x + boxRect.width / 2 - size.x / 4,
					  boxRect.y + boxRect.height * 0.5f - size.y * 0.5f, size.x, size.y);

				  GUI.Label(rect1, $"{node.name}", style);
				}

			  if (snapIndicatorOpt.valueOut(out var snapIndicator)) {
				  EditorGUI.DrawRect(snapIndicator, indicatorColor);
			  }

			  doEvents();
		  }
	  }

	  void addSelectedNode(FunSequenceNode nodeToAdd, Event currentEvent) {
		  if (!selectedNodesList.isEmpty()) {
				selectedNodesList.find(x => x == nodeToAdd).voidFold(
				  () => {
					  if ( currentEvent.control ) {
						  selectedNodesList.Add(nodeToAdd);
					  }
					  else {
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
		  else {
			  selectedNodesList.Clear();
			  selectedNodesList.Add(nodeToAdd);
		  }
	  }
		  
		void doEvents() {
			var ev = Event.current;
			var snappingEnabled = !Event.current.shift && snapping;
			switch (ev.rawType) {
				case EventType.MouseDown:
					
					foreach (var nodes in funNodes) {
						for (var i = 0; i < nodes.Count; i++) {
							var node = nodes[i];
							

							if (new Rect(timelineVisuals.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20).Contains(Event.current
								.mousePosition)) {
								removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
								
								addSelectedNode(node, Event.current);
								selectedNodeIndex = i;
								resizeNodeStart = true;
								EditorGUI.FocusTextInControl("");
								ev.Use();
							}

							if (new Rect(timelineVisuals.SecondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20).Contains(
								Event.current.mousePosition)) {
								removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
								addSelectedNode(node, Event.current);
								selectedNodeIndex = i;
								resizeNodeEnd = true;
								EditorGUI.FocusTextInControl("");
								ev.Use();
							}

							if (new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20, timelineVisuals.SecondsToGUI(node.duration), 20)
								.Contains(Event.current.mousePosition)) {
								switch (ev.button) {
									case 0:
										removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
										timeClickOffset = node.startTime - timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
										dragNode = true;
										selectedNodeIndex = i;
										addSelectedNode(node, Event.current);
										EditorGUI.FocusTextInControl("");
										break;
									case 1:
										selectedNodeIndex = i;
										var genericMenu = new GenericMenu();
										genericMenu.AddItem(new GUIContent("Remove"), false, removeSelectedNode, node);
										genericMenu.AddItem(new GUIContent("Unselect"), false, deselect, node);
										genericMenu.ShowAsContext();
										break;
								}
								ev.Use();
							}
						}
					}
					break;
				case EventType.MouseDrag:

					if (!selectedNodesList.isEmpty()) {
						foreach (var selectedNode in selectedNodesList) {

							if (resizeNodeStart) {
								if (rootSelectedNode.valueOut(out var rootSelected) && selectedNode == rootSelected) {
									var selectedNodeEnd = selectedNode.getEnd();
									selectedNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
									selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, 0, float.MaxValue);
									if (selectedNode.startTime > 0 && !isStartSnapped) {
										//TODO sometimes end moves abit
										selectedNode.duration -= timelineVisuals.GUIToSeconds(Event.current.delta.x);
										selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);
									}
									
									if (snappingEnabled) { snapStart(selectedNode, selectedNodesList, selectedNodeEnd); }
									
									foreach (var node in selectedNodesList) {
										if (node != rootSelected) {
											var nodeEnd = node.getEnd();
											node.startTime = selectedNode.startTime;
											node.duration = nodeEnd - node.startTime;
											node.startTime = Mathf.Clamp(node.startTime, 0, float.MaxValue);
											node.duration  = Mathf.Clamp(node.duration , 0.01f, float.MaxValue);
										}
									}

									ev.Use();
								}
							}

							if (resizeNodeEnd) {
								if (rootSelectedNode.valueOut(out var rootSelected) && selectedNode == rootSelected) {
									selectedNode.duration = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) - selectedNode.startTime;
									selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);

									if (snappingEnabled) { snapEnd(selectedNode); }
									
									foreach (var node in selectedNodesList) {
										if (node != rootSelected) {
											node.duration = rootSelected.duration - (node.startTime - rootSelected.startTime);
											node.duration = Mathf.Clamp(node.duration, 0.01f, float.MaxValue);
										}
									}

									selectedNode.changeDuration();
									recalculateTimelineWidth();

									ev.Use();
								}
							}

							//Draging the node
							if (dragNode && !resizeNodeStart && !resizeNodeEnd) {
								if (rootSelectedNode.valueOut(out var rootSelected) && selectedNode == rootSelected) {
									
									var diffList = new List<float>();
									foreach (var selected in selectedNodesList) {
										diffList.Add(selected.startTime - rootSelected.startTime);
									}
									var clampLimit = selectedNodesList.find(node => node.startTime <= 0).isSome ? selectedNode.startTime : 0;

									selectedNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) + timeClickOffset;
									selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, clampLimit, float.MaxValue);
									
									isEndSnapped = false;
									isStartSnapped = false;
									
									if (snappingEnabled) { snapDrag(selectedNode, selectedNodesList); }

									//setting multiselected nodes starttimes
									for (var i = 0; i < selectedNodesList.Count; i++) {
										var node = selectedNodesList[i];
										node.startTime = rootSelected.startTime + diffList[i];
										node.startTime = Mathf.Clamp(node.startTime, 0, float.MaxValue);
									}
									
									if (Event.current.mousePosition.y > selectedNode.channel * 20 + 25) {
										foreach (var node in selectedNodesList) {
											node.channel += 1;
										}
									}

									if (Event.current.mousePosition.y < selectedNode.channel * 20 - 5
										&& selectedNodesList.find(node => node.channel == 0).isNone) {
										foreach (var node in selectedNodesList) {
											node.channel -= 1;
										}
									}

									foreach (var node in selectedNodesList) {
										node.channel = Mathf.Clamp(node.channel, 0, int.MaxValue);
										node.element.timelineChannelIdx = node.channel;
									}
									
								}

								ev.Use();
							}
						}
						refreshChannelNodes();
					}

					break;
				case EventType.MouseUp:
					isEndSnapped = false;
					isStartSnapped = false;
					dragNode = false;
					resizeNodeStart = false;
					resizeNodeEnd = false;
					nodeSnappedToOpt = F.none_;
					recalculateTimelineWidth();
					exportTimelineToTweenManager();
					break;
			}
		}

	  void recalculateTimelineWidth() => timelineVisuals.lastNodeTime = timelineVisuals.SecondsToGUI(
		  funNodes.fold(
		  	() => 0,
		  	nodes => nodes.Max(node => node.getEnd())
			)
	  );

	  void snapDrag(FunSequenceNode rootNode, List<FunSequenceNode> selectedNodes) {
			if (funNodes.valueOut(out var nodes)) {
				var nonSelectedNodes = nodes.Except(selectedNodes).ToList();
				
				nonSelectedNodes.ForEach(earlierNode => nodeSnappedToOpt.voidFold(
						() => snap(earlierNode),
						nodeSnapped => snap(nodeSnapped.node)
					));


				void snap(FunSequenceNode nodeToSnapTo){
					//Snapping selected end with node start
					if ( isInRangeOfSnap( timelineVisuals.SecondsToGUI(nodeToSnapTo.startTime),
						timelineVisuals.SecondsToGUI(rootNode.getEnd()) )
					) {
						setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime - rootNode.duration, ref isEndSnapped);
					}
					//Snapping selected end with node end
					else if ( isInRangeOfSnap( timelineVisuals.SecondsToGUI(nodeToSnapTo.getEnd()),
						timelineVisuals.SecondsToGUI(rootNode.getEnd()) )
					) {
						setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd() - rootNode.duration, ref isEndSnapped);
					}
					//Snapping selected start with node start
					else if ( isInRangeOfSnap( timelineVisuals.SecondsToGUI(nodeToSnapTo.startTime),
						timelineVisuals.SecondsToGUI(rootNode.startTime) )
					) {
						setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime, ref isStartSnapped);
					}
					//Snapping selected start end with node end
					else if ( isInRangeOfSnap( timelineVisuals.SecondsToGUI(nodeToSnapTo.getEnd()),
						timelineVisuals.SecondsToGUI(rootNode.startTime) )
					) {
						setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd(), ref isStartSnapped);
					}
					else {
						nodeSnappedToOpt = F.none_;
					}
				}
			}

			bool isInRangeOfSnap(float snapPivot, float positionToCheck) =>
				positionToCheck < snapPivot + SNAPPING_POWER && positionToCheck > snapPivot - SNAPPING_POWER;
		
			void setSnapping(FunSequenceNode nodeToSnapTo, bool snappedToNodeStart, float resultToSet, ref bool sideToSnap) {
					nodeSnappedToOpt = new nodeSnappedTo(nodeToSnapTo, snappedToNodeStart).some();
					rootNode.startTime = resultToSet;
					sideToSnap = true;
			}
	  }

	  void snapEnd(FunSequenceNode selectedNode) {
		  if (funNodes.valueOut(out var nodes)) {
			  
			  nodes.Except(selectedNodesList).Where(node =>
				  selectedNode.startTime <= node.getEnd()
			  ).ToList().ForEach(earlierNode => nodeSnappedToOpt.voidFold(
				  () => snap(earlierNode),
				  nodeSnapped => snap(nodeSnapped.node)
			  ));
			  
				  void snap(FunSequenceNode laterNode){
						var snapPivot = timelineVisuals.SecondsToGUI(laterNode.startTime);
						var nodeEndPos = timelineVisuals.SecondsToGUI(selectedNode.getEnd());
						if ( nodeEndPos < snapPivot + SNAPPING_POWER && nodeEndPos > snapPivot - SNAPPING_POWER ) {
							selectedNode.duration = laterNode.startTime - selectedNode.startTime;
							isEndSnapped = true;
							nodeSnappedToOpt = new nodeSnappedTo(laterNode, true).some();
						}
						else {
							snapPivot = timelineVisuals.SecondsToGUI(laterNode.getEnd());
							if ( nodeEndPos < snapPivot + SNAPPING_POWER && nodeEndPos > snapPivot - SNAPPING_POWER ) {
								selectedNode.duration = laterNode.getEnd() - selectedNode.startTime;
								isEndSnapped = true;
								nodeSnappedToOpt = new nodeSnappedTo(laterNode, false).some();
							}
							else {
								isEndSnapped = false;
								nodeSnappedToOpt = F.none_;
							}
						}
			  	}
	  	}
	 	}

	  void snapStart(FunSequenceNode selectedNode, List<FunSequenceNode> selectedNodesList, float realEnd) {
		  foreach (var nodes in funNodes) {
			  //Finding earlier than selectedNode
			  //and trying to snap to every of these nodes if we are not already snapped
			  nodes.Except(selectedNodesList).Where(node =>
				  selectedNode.getEnd() >= node.startTime
			  ).ToList().ForEach(earlierNode => nodeSnappedToOpt.voidFold(
				  () => snap(earlierNode),
				  nodeSnapped => snap(nodeSnapped.node)
			  ));
			  
			  void snap(FunSequenceNode nodeToSnap) {
				  var end = selectedNode.getEnd();
				  var snapPivot = timelineVisuals.SecondsToGUI(nodeToSnap.startTime);
				  var nodeStartPos = timelineVisuals.SecondsToGUI(selectedNode.startTime);
				  if (nodeStartPos < snapPivot + SNAPPING_POWER && nodeStartPos > snapPivot - SNAPPING_POWER) {
					  selectedNode.startTime = nodeToSnap.startTime;
					  if (!isStartSnapped) {
						  selectedNode.duration = end - selectedNode.startTime;
						  nodeSnappedToOpt = new nodeSnappedTo(nodeToSnap, true).some();
						  isStartSnapped = true;
					  }
				  }
				  else {
					  snapPivot = timelineVisuals.SecondsToGUI(nodeToSnap.getEnd());
					  if (nodeStartPos < snapPivot + SNAPPING_POWER && nodeStartPos > snapPivot - SNAPPING_POWER) {
						  selectedNode.startTime = nodeToSnap.getEnd();
						  if (!isStartSnapped) {
							  selectedNode.duration = end - selectedNode.startTime;
							  nodeSnappedToOpt = new nodeSnappedTo(nodeToSnap, false).some();
							  isStartSnapped = true;
						  }
					  }
					  else if (isStartSnapped) {
						  selectedNode.duration = realEnd - selectedNode.startTime;
						  nodeSnappedToOpt = F.none_;
						  isStartSnapped = false;
					  }
				  }
			  }
			  
		  }
	  }

	  //imports element info from FunTweenManager element array
	  void importTimeline() {
		  advancedEditor.Instances = new object[] { };
		  if (funTweenManager.valueOut(out var manager) && manager.timeline != null) {
			  var elements = manager.timeline.elements;

			  funNodes = manager.timeline.elements.Select(
				  (element, idx) => {
					  if (idx != 0 && element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement
					  ) {
						  element.timelineChannelIdx = elements[idx - 1].timelineChannelIdx;
					  }

					  if (element.title == "") element.title = element.element == null ? "" : element.element.GetType().Name;

					  var newNode = new FunSequenceNode(element, whereToStart(idx), element.title);
					  //Might have unwanted consequences
					  if (funNodes.valueOut(out var nodes) && nodes.find(x => x.element == element).valueOut(out var foundNode)) {
						  foundNode.duration = newNode.duration;
						  foundNode.startTime = newNode.startTime;
						  foundNode.name = newNode.name;
						  return foundNode;
					  }
					  else return newNode;
					  
				  }
			  ).ToList().some();

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

	  Option<FunSequenceNode> getLeftNode(FunSequenceNode selectedNode) => 
		  funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime < selectedNode.startTime
				  ).ToList().noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).Last()
					)
		  
		);

	  void refreshChannelNodes() {
		  if (!selectedNodesList.isEmpty()) {
			  foreach (var selected in selectedNodesList) {
				  getLeftNode(selected).voidFold(
					  () => setSelectedNodeElementFields(selected, SerializedTweenTimeline.Element.At.SpecificTime),
					  _ => setSelectedNodeElementFields(selected, selected.element.startAt));
			  }
		  }
	  }
	  
	  void exportTimelineToTweenManager() {
		  if (funTweenManager.valueOut(out var manager) && funNodes.valueOut(out var nodes)) {
			  var arr = new List<FunSequenceNode>();
			  for (var i = 0; i <= nodes.Max(x => x.channel); i++) {
				  arr.AddRange(
					  nodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
				  );
			  }
			  manager.timeline.elements = arr.Select(elem => {
				  elem.element.timelineChannelIdx = elem.channel;
				  return elem.element;
			  }).ToArray();
		  }

		  if (funNodes.isNone) manager.timeline.elements = new SerializedTweenTimeline.Element[0];
			//We need to find new selected node index after importing our elements into FunTweenManager
			if (!selectedNodesList.isEmpty()){
				foreach (var idx in manager.timeline.elements.indexWhere(element => element == selectedNodesList.First().element)) {
						selectedNodeIndex = idx;
				}
			}
		  importTimeline();
	  }
	  
	  void setSelectedNodeElementFields(FunSequenceNode selected, SerializedTweenTimeline.Element.At newStartType) {
			  switch (newStartType) {
				  case SerializedTweenTimeline.Element.At.AfterLastElement: {
					  if (getLeftNode(selected).valueOut(out var leftNode)) {
						  selected.setTimeOffset(selected.startTime - leftNode.getEnd());
						  selected.element.startAt = newStartType;
						  selected.element.element.setDuration(selected.duration); 
					  }
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.SpecificTime: {
					  selected.setTimeOffset(selected.startTime);
					  selected.element.startAt = newStartType;
					  selected.element.element.setDuration(selected.duration); 
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.WithLastElement: {
					  if (getLeftNode(selected).valueOut(out var leftNode)) {
						  selected.setTimeOffset(selected.startTime - leftNode.startTime);
						  selected.element.startAt = newStartType;
						  selected.element.element.setDuration(selected.duration); 
					  }
					  break;
				  }
		  }
	  }

	  void onSettings(float width) {
		  if (funTweenManager.valueOut(out var manager)) {
			  GUILayout.BeginVertical();
			  GUI.enabled = true;
			  if (funNodes.valueOut(out var nodes)) {
				  nodes.find(elem => elem.element.element == null).map(_ => GUI.enabled = false);
			  }
		  
			  if (GUILayout.Button("Add Tween")) {
				  var newElement = new SerializedTweenTimeline.Element {
					  startAt = SerializedTweenTimeline.Element.At.SpecificTime
				  };
				  manager.timeline.elements = manager.timeline.elements.addOne(newElement);
				  importTimeline();
				  if (funNodes.valueOut(out var refreshedNodes)) {
					  foreach (var foundNode in refreshedNodes.find(x => x.element.element == null)) {
						  addSelectedNode(foundNode, Event.current);
					  }
					  exportTimelineToTweenManager();
				  }
			  }
			  GUILayout.EndVertical();
		  }
		  var guiEnabled = GUI.enabled;
		  var oneNodeSelected =  selectedNodesList.Count == 1;

		  if (funTweenManager.isSome) {
			  if (!selectedNodesList.isEmpty()) {
				  GUILayout.BeginHorizontal();
				  if (GUILayout.Button("Refresh")) {
					  refreshChannelNodes();
					  importTimeline();
					  exportTimelineToTweenManager();
				  }

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
							  Log.d.warn($"{selected.name}");
						  }
					  }
				  }
				  
				  GUILayout.EndHorizontal();

				  if (oneNodeSelected) {
					  GUILayout.BeginHorizontal();
					  if (selectedNodesList.First().element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement) {
						  GUI.enabled = false;
					  }

					  if (GUILayout.Button("After Last")) {
						  setSelectedNodeElementFields(selectedNodesList.First(), SerializedTweenTimeline.Element.At.AfterLastElement);
					  }

					  GUI.enabled = guiEnabled;

					  if (selectedNodesList.First().element.startAt == SerializedTweenTimeline.Element.At.WithLastElement) {
						  GUI.enabled = false;
					  }

					  if (GUILayout.Button("With Last")) {
						  setSelectedNodeElementFields(selectedNodesList.First(), SerializedTweenTimeline.Element.At.WithLastElement);
					  }

					  GUI.enabled = guiEnabled;

					  if (selectedNodesList.First().element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
						  GUI.enabled = false;
					  }

					  if (GUILayout.Button("Specific")) {
						  setSelectedNodeElementFields(selectedNodesList.First(), SerializedTweenTimeline.Element.At.SpecificTime);
					  }

					  GUI.enabled = guiEnabled;
					  GUILayout.EndHorizontal();
				  }

			  }
			}
		  else {
			  if (GUILayout.Button("[Add manager]")) {
				  addFunTweenManagerComponent(Selection.activeGameObject);
				  EditorGUIUtility.ExitGUI();
			  }
		  }
			GUILayout.Space(10);
		  GUI.enabled = true;
		  settingsScroll = GUILayout.BeginScrollView(settingsScroll);
		  if (funTweenManager.valueOut(out var ftm) && oneNodeSelected) {
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
				if (advancedEditor.Instances.isEmpty()
					|| advancedEditor.Instances[0] != manager.timeline.elements[selectedNodeIndex]
					) { 
					
					if (manager.timeline.elements != null && selectedNodeIndex < manager.timeline.elements.Length) {
						advancedEditor.Instances = new object[] {
							manager.timeline.elements[selectedNodeIndex]
						};
					}}
		}

	  void deselect(object obj) {
		  var nawd = obj as FunSequenceNode;
		  selectedNodesList.Remove(nawd);
		  Repaint();
	  }
	  
		void removeSelectedNode(object obj) {
			var nawd = obj as FunSequenceNode;
			if (funNodes.valueOut(out var nodes)) {
				if (nodes.Count == 1) {
					funNodes = new List<FunSequenceNode>().some();
				}
				else {
						nodes.Remove(nawd);
				}
				exportTimelineToTweenManager();
				selectedNodeIndex = 0;
			}
		}

	  void removeoAllSelectedNodes() {
		  if (funNodes.valueOut(out var nodes) && !selectedNodesList.isEmpty()) {
			  foreach (var selectedNode in selectedNodesList) {
				  nodes.Remove(selectedNode);
			  }
			  exportTimelineToTweenManager();
			  selectedNodeIndex = 0;
		  }
	  }

	  void removeNodeIfHasNoElement(FunSequenceNode node) {
		  if (node.element.element == null && funNodes.valueOut(out var nodes)) {
			  nodes.Remove(node);
			  exportTimelineToTweenManager();
			  selectedNodeIndex = 0;
		  }
	  }

	  void addFunTweenManagerComponent(GameObject gameObject) {
		  gameObject.GetComponent<FunTweenManager>().opt().voidFold(
			  () => {
				  funTweenManager = gameObject.AddComponent<FunTweenManager>().some();
				  importTimeline();
				  EditorUtility.SetDirty(gameObject);
			  },
			  fun => funTweenManager = fun.some()
			);
	  }

	  public void playForward() {
		  if (funTweenManager.valueOut(out var ftm)) {
			  ftm.run(FunTweenManager.Action.PlayForwards);
		  }
	  }
  }
}
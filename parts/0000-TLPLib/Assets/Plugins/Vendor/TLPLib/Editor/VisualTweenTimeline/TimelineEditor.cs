using System.Collections.Generic;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using GenerationAttributes;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineEditor : EditorWindow {
    Timeline timelineVisuals;
	  ExternalEditor advancedEditor;
	  
	  bool isStartSnapped;
	  bool isEndSnapped;
	  
	  [Record]
	  partial struct nodeSnappedTo {
		  public readonly FunSequenceNode node;
		  public readonly bool snappedToStart;
	  }

	  Option<nodeSnappedTo> nodeSnappedToOpt;
	  
		Option<GameObject> selectedGameObjectOpt;
	  Option<FunTweenManager> funTweenManager = F.none_;
	  Option<List<FunSequenceNode>> funNodes;
	  Option<FunSequenceNode> selectedNodeOpt => funNodes.flatMap(nodes =>
		  nodes.Count > selectedNodeIndex
		  	? nodes[selectedNodeIndex].some()
		  	: F.none_
	  );

		int selectedNodeIndex, snappingPower = 15, selectedNodeOutlineWidth = 3;
	  float settingsWidth, timeClickOffset;
	  bool resizeNodeStart, resizeNodeEnd, dragNode;
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
			
			if (funTweenManager.isSome) refreshTimeline();
			timelineVisuals.onSettingsGUI = onSettings;
			timelineVisuals.onTimelineGUI = drawFunNodes;
			if (selectedGameObjectOpt.isNone)
				OnSelectionChange();
		}
	  
		void OnSelectionChange() {
			selectedGameObjectOpt = Selection.activeGameObject.opt();

			funTweenManager = selectedGameObjectOpt.flatMap(selected => {
				Log.d.warn($"yes selection {selected.name}");
				return selected.GetComponent<FunTweenManager>().opt();
			});
			funTweenManager.voidFold(
				() => {
					Log.d.warn($"no ftm");
					funNodes = F.none_;
				},
				_ => {
					Log.d.warn($"yes ftm");
					refreshTimeline();
				}
			);
			
			selectedNodeIndex = 0;
			Repaint();
		}
	  
	  void OnGUI() {
			GUI.enabled = selectedGameObjectOpt.isSome;
			timelineVisuals.DoTimeline(new Rect(0, 0, position.width, position.height));

		  if (GUI.changed) {
			  refreshTimeline();
			  Log.d.warn("editor gui changed aliooo");
		  }
	  }

	  void undoCalback() {
		  refreshTimeline();
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

					  var currentNodeIsSelected = selectedNodeOpt.valueOut(out var selectedNode) && selectedNode == node;

					  if (currentNodeIsSelected) {
						  EditorGUI.DrawRect(boxRect, Color.magenta);
						  GUI.Box(new Rect(
							  boxRect.x + selectedNodeOutlineWidth,
							  boxRect.y + selectedNodeOutlineWidth,
							  boxRect.width - selectedNodeOutlineWidth * 2,
							  boxRect.height - selectedNodeOutlineWidth * 2
						  ), "", "TL LogicBar 0");

						  //Todo rewrite this maybe
						  if (isEndSnapped || isStartSnapped) {
							  foreach (var nodeSnappedTo in nodeSnappedToOpt) {
								  var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
								  var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * 20;
								  snapIndicatorOpt =
									  selectedIsHigher
										  ? new Rect(timelineVisuals.SecondsToGUI(isStartSnapped
											  ? selectedNode.startTime
											  : selectedNode.getEnd()
											  ), selectedNode.channel * 20 - 10, 2, distance).some()
										  : new Rect(timelineVisuals.SecondsToGUI(nodeSnappedTo.snappedToStart
											  ? nodeSnappedTo.node.startTime
											  : nodeSnappedTo.node.getEnd()), nodeSnappedTo.node.channel * 20 - 10, 2, distance).some();
							  }
							  indicatorColor = isEndSnapped ? Color.yellow: Color.cyan;
						  }

					  }
					  else {
						  GUI.Box(boxRect, "", "TL LogicBar 0");
					  }

					  var style = new GUIStyle("Label");
					  style.fontSize = currentNodeIsSelected ? 12 : style.fontSize;
					  style.fontStyle = currentNodeIsSelected ? FontStyle.Bold : FontStyle.Normal;
					  var color = currentNodeIsSelected ? Color.magenta : style.normal.textColor;
					  color.a = currentNodeIsSelected ? 1.0f : 0.7f;
					  style.normal.textColor = color;
					  Vector3 size = style.CalcSize(new GUIContent($"content: {node.name}"));
					  var rect1 = new Rect(boxRect.x + boxRect.width * 0.5f - size.x * 0.2f,
						  boxRect.y + boxRect.height * 0.5f - size.y * 0.5f, size.x, size.y);

					  GUI.Label(rect1, $"{node.name}", style);
				}

			  if (snapIndicatorOpt.valueOut(out var snapIndicator)) {
				  EditorGUI.DrawRect(snapIndicator, indicatorColor);
			  }

			  DoEvents();
		  }
	  }

		void DoEvents() {
			var ev = Event.current;
			switch (ev.rawType) {
				case EventType.MouseDown:

					foreach (var nodes in funNodes) {
						for (var i = 0; i < nodes.Count; i++) {
							var node = nodes[i];

							if (new Rect(timelineVisuals.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20).Contains(Event.current
								.mousePosition)) {
								removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
								selectedNodeIndex = i;
								resizeNodeStart = true;
								EditorGUI.FocusTextInControl("");
								ev.Use();
							}

							if (new Rect(timelineVisuals.SecondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20).Contains(
								Event.current.mousePosition)) {
								removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
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
										EditorGUI.FocusTextInControl("");
										break;
									case 1:
										selectedNodeIndex = i;
										var genericMenu = new GenericMenu();
										genericMenu.AddItem(new GUIContent("Remove"), false, removeSelectedNode, node);
										genericMenu.ShowAsContext();
										break;
								}

								ev.Use();
							}
						}
					}
					break;
				case EventType.MouseDrag:

					foreach (var selectedNode in selectedNodeOpt) {
						
						if (resizeNodeStart) {
							var selectedNodeEnd = selectedNode.getEnd();
							selectedNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
							selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, 0, float.MaxValue);
							if (selectedNode.startTime > 0 && !isStartSnapped) {
								//TODO sometimes end moves abit
								selectedNode.duration -= timelineVisuals.GUIToSeconds(Event.current.delta.x);
								selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);
							}

							snapStart(selectedNode, selectedNodeEnd);
							selectedNode.changeDuration();
							ev.Use();
						}

						if (resizeNodeEnd) {
							selectedNode.duration = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) - selectedNode.startTime;
							selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);
							
							snapEnd(selectedNode);
							selectedNode.changeDuration();
							recalculateTimelineWidth();

							ev.Use();
						}

						if (dragNode && !resizeNodeStart && !resizeNodeEnd) {
							selectedNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) + timeClickOffset;
							selectedNode.startTime = Mathf.Clamp(selectedNode.startTime, 0, float.MaxValue);

							isEndSnapped = false;
							isStartSnapped = false;

							if (Event.current.mousePosition.y > selectedNode.channel * 20 + 25) {
								selectedNode.channel += 1;
							}

							if (Event.current.mousePosition.y < selectedNode.channel * 20 - 5) {
								selectedNode.channel -= 1;
							}

							selectedNode.channel = Mathf.Clamp(selectedNode.channel, 0, int.MaxValue);
							selectedNode.element.timelineChannelIdx = selectedNode.channel;
							
							ev.Use();
					}
						refreshChannelNodes();
						refreshTimeline();
						exportTimelineToTweenManager();
					}

					break;
				case EventType.MouseUp:
					isEndSnapped = false;
					isStartSnapped = false;
					dragNode = false;
					resizeNodeStart = false;
					resizeNodeEnd = false;
					recalculateTimelineWidth();
					break;
			}
		}

	  void recalculateTimelineWidth() => timelineVisuals.lastNodeTime = timelineVisuals.SecondsToGUI(
		  funNodes.fold(
		  	() => 0,
		  	nodes => nodes.Max(node => node.getEnd())
			)
	  );

	  void snapDrag(FunSequenceNode selectedNode) {
			if (funNodes.valueOut(out var nodes)){
				nodes.Where(node => node != selectedNode).ToList()
					.ForEach(earlierNode => nodeSnappedToOpt.voidFold(
						() => snap(earlierNode),
						nodeSnapped => snap(nodeSnapped.node)
					));
				
				void snap(FunSequenceNode laterNode){
					//Snapping selected end with node start
					if ( isInRangeOfSnap(timelineVisuals.SecondsToGUI(laterNode.startTime),
						timelineVisuals.SecondsToGUI(selectedNode.getEnd()))
					) {
						setSnapping(laterNode.startTime - selectedNode.duration, ref isEndSnapped);
						nodeSnappedToOpt = new nodeSnappedTo(laterNode, true).some();
					}
					//Snapping selected end with node end
					else if (isInRangeOfSnap(timelineVisuals.SecondsToGUI(laterNode.getEnd()),
						timelineVisuals.SecondsToGUI(selectedNode.getEnd()))
					) {
						setSnapping(laterNode.getEnd() - selectedNode.duration, ref isEndSnapped);
						nodeSnappedToOpt = new nodeSnappedTo(laterNode, false).some();
					}
					//Snapping selected start with node start
					else if (isInRangeOfSnap(timelineVisuals.SecondsToGUI(laterNode.startTime),
						timelineVisuals.SecondsToGUI(selectedNode.startTime))
					) {
						setSnapping(laterNode.startTime, ref isStartSnapped);
						nodeSnappedToOpt = new nodeSnappedTo(laterNode, true).some();
					}
					//Snapping selected start end with node end
					else if (isInRangeOfSnap(timelineVisuals.SecondsToGUI(laterNode.getEnd()),
						timelineVisuals.SecondsToGUI(selectedNode.startTime))
					) {
						setSnapping(laterNode.getEnd(), ref isStartSnapped);
						nodeSnappedToOpt = new nodeSnappedTo(laterNode, false).some();
					}
					else {
						nodeSnappedToOpt = F.none_;
					}
					
				}
	}

	bool isInRangeOfSnap(float snapPivot, float positionToCheck) =>
		positionToCheck < snapPivot + snappingPower && positionToCheck > snapPivot - snappingPower;

	void setSnapping(float resultToSet, ref bool sideToSnap) {
			selectedNode.startTime = resultToSet;
			sideToSnap = true;
	}
	  }

	  void snapEnd(FunSequenceNode selectedNode) {
		  if (funNodes.valueOut(out var nodes)) {
			  nodes.Where(node =>
				  selectedNode.startTime <= node.getEnd() && node != selectedNode
				).ToList().ForEach(earlierNode => nodeSnappedToOpt.voidFold(
				  () => snap(earlierNode),
				  nodeSnapped => snap(nodeSnapped.node)
			  ));
			  
				  void snap(FunSequenceNode laterNode){
						var snapPivot = timelineVisuals.SecondsToGUI(laterNode.startTime);
						var nodeEndPos = timelineVisuals.SecondsToGUI(selectedNode.getEnd());
						if ( nodeEndPos < snapPivot + snappingPower && nodeEndPos > snapPivot - snappingPower ) {
							selectedNode.duration = laterNode.startTime - selectedNode.startTime;
							isEndSnapped = true;
							nodeSnappedToOpt = new nodeSnappedTo(laterNode, true).some();
						}
						else {
							snapPivot = timelineVisuals.SecondsToGUI(laterNode.getEnd());
							if ( nodeEndPos < snapPivot + snappingPower && nodeEndPos > snapPivot - snappingPower ) {
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

	  void snapStart(FunSequenceNode selectedNode, float realEnd) {
		  foreach (var nodes in funNodes) {
			  //Finding earlier than selected nodes
			  //and trying to snap to every of these nodes if we are not already snapped
			  nodes.Where(node =>
				  node != selectedNode
				  && selectedNode.getEnd() >= node.startTime
				  
				).ToList().ForEach(earlierNode => nodeSnappedToOpt.voidFold(
				  () => snap(earlierNode),
				  nodeSnapped => snap(nodeSnapped.node)
			  ));
			  
			  void snap(FunSequenceNode nodeToSnap) {
				  var end = selectedNode.getEnd();
				  var snapPivot = timelineVisuals.SecondsToGUI(nodeToSnap.startTime);
				  var nodeStartPos = timelineVisuals.SecondsToGUI(selectedNode.startTime);
				  if (nodeStartPos < snapPivot + snappingPower && nodeStartPos > snapPivot - snappingPower) {
					  selectedNode.startTime = nodeToSnap.startTime;
					  if (!isStartSnapped) {
						  selectedNode.duration = end - selectedNode.startTime;
						  nodeSnappedToOpt = new nodeSnappedTo(nodeToSnap, true).some();
						  isStartSnapped = true;
					  }
				  }
				  else {
					  snapPivot = timelineVisuals.SecondsToGUI(nodeToSnap.getEnd());
					  if (nodeStartPos < snapPivot + snappingPower && nodeStartPos > snapPivot - snappingPower) {
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
	  void refreshTimeline() {
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

					  return new FunSequenceNode(element, whereToStart(idx), element.title);
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

	  Option<FunSequenceNode> getLeftOfSelectedNode() => selectedNodeOpt.flatMap(
		  selectedNode => funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime < selectedNode.startTime
				  ).noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).Last()
					)
		  )
		);
	  Option<FunSequenceNode> getRightOfSelectedNode() => selectedNodeOpt.flatMap(
		  selectedNode => funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime > selectedNode.startTime
				  ).noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).First()
				  )
		  )
	  );

	  void refreshChannelNodes() {
		  if (selectedNodeOpt.valueOut(out var selected)) {
			  getLeftOfSelectedNode().voidFold(
				  () => setSelectedNodeStartType(SerializedTweenTimeline.Element.At.SpecificTime),
				  _  => setSelectedNodeStartType(selected.element.startAt) 
			  );
		  }
	  }

	  void exportTimelineToTweenManager() {
		  if (funTweenManager.valueOut(out var manager) && funNodes.valueOut(out var nodes))
					nodes.OrderBy(node => node.channel).noneIfEmpty().map(ordered => ordered.Last().channel).voidFold(
						() => manager.timeline.elements = new SerializedTweenTimeline.Element[0],
						channel => {
							var arr = new List<FunSequenceNode>();
							for (var i = 0; i <= channel; i++) {
								arr.AddRange(
									nodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
								);
							}
							manager.timeline.elements = arr.Select(elem => elem.element).ToArray();
						}
					);
			//We need to find new selected node index after importing our elements into FunTweenManager
			if (selectedNodeOpt.valueOut(out var selectedNode)){
				foreach (var idx in manager.timeline.elements.indexWhere(element => element == selectedNode.element)) {
					if (selectedNodeIndex != idx) {
						selectedNodeIndex = idx;
						refreshTimeline();
					}
				}
			}
	  }
	  
	  void setSelectedNodeStartType(SerializedTweenTimeline.Element.At newStartType) {
		  if (selectedNodeOpt.valueOut(out var selected)) {
			  switch (newStartType) {
				  case SerializedTweenTimeline.Element.At.AfterLastElement: {
					  if (getLeftOfSelectedNode().valueOut(out var leftNode)) {
						  selected.setTimeOffset(selected.startTime - leftNode.getEnd());
						  selected.element.startAt = newStartType;
					  }
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.SpecificTime: {
					  selected.setTimeOffset(selected.startTime);
					  selected.element.startAt = newStartType;
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.WithLastElement: {
					  if (getLeftOfSelectedNode().valueOut(out var leftNode)) {
						  selected.setTimeOffset(selected.startTime - leftNode.startTime);
						  selected.element.startAt = newStartType;
					  }
					  break;
				  }
			  }
		  }
	  }

	  void onSettings(float width) {
		  if (funTweenManager.valueOut(out var manager)) {
			  GUILayout.BeginVertical();
			  GUI.enabled = true;
			  funNodes.flatMap(nodes => nodes.find(elem => elem.element.element == null)).map(_ => GUI.enabled = false);
		  
			  if (GUILayout.Button("Add Tween")) {
				  var newElement = new SerializedTweenTimeline.Element {
					  startAt = SerializedTweenTimeline.Element.At.SpecificTime
				  };
				  manager.timeline.elements = manager.timeline.elements.addOne(newElement);
				  refreshTimeline();
				  selectedNodeIndex = manager.timeline.elements.Length - 1;
			  }
			  GUILayout.EndVertical();
		  }
		  
		  if (funTweenManager.isSome) {
			  if (selectedNodeOpt.valueOut(out var selectedNode)) {
				  GUILayout.BeginHorizontal();
				  if (GUILayout.Button("Refresh")) {
					  refreshTimeline();
					  refreshChannelNodes();
					  exportTimelineToTweenManager();
				  }


				  GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
				  if (GUILayout.Button("Remove Selected")) {
					  removeSelectedNode(this);
				  }

				  GUI.backgroundColor = Color.white;
				  GUILayout.EndHorizontal();
				  GUILayout.BeginHorizontal();
				  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement) {
					  GUI.enabled = false;
				  }

				  if (GUILayout.Button("After Last")) {
					  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.AfterLastElement);
				  }

				  GUI.enabled = true;

				  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.WithLastElement) {
					  GUI.enabled = false;
				  }

				  if (GUILayout.Button("With Last")) {
					  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.WithLastElement);
				  }

				  GUI.enabled = true;

				  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
					  GUI.enabled = false;
				  }

				  if (GUILayout.Button("Specific")) {
					  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.SpecificTime);
				  }

				  GUI.enabled = true;
				  GUILayout.EndHorizontal();
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
		  GUILayout.BeginScrollView(settingsScroll);
		  if (funTweenManager.isSome) {
			  drawElementSettings();
			  if (funTweenManager.valueOut(out var ftm)
				  && advancedEditor.Instances.Length > 0
				  && advancedEditor.Draw(new Rect(0, 0, width, position.height - 100))
			  ) {
				  Undo.RecordObject(ftm, "Tween Manager Changes");
				  Repaint(); 
			  }
		  }
		  
		  GUILayout.EndScrollView();
	  }

		void drawElementSettings() {
			foreach (var manager in funTweenManager) {
				if (manager.timeline.elements != null && selectedNodeIndex < manager.timeline.elements.Length) {
					advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
					advancedEditor.Instances = new object[] {
						manager.timeline.elements[selectedNodeIndex]
					};
				}
			}
		}
	  
	  
	  //TODO maybe action or something?
		void removeSelectedNode(object _) {
			if (funNodes.valueOut(out var nodes) && selectedNodeOpt.valueOut(out var selectedNode)) {
				if (nodes.Count == 1) {
					funNodes = new List<FunSequenceNode>().some();
				}
				else {
					nodes.Remove(selectedNode);
				}
				exportTimelineToTweenManager();
				selectedNodeIndex = 0;
				refreshTimeline();
			}
		}

	  void removeNodeIfHasNoElement(FunSequenceNode node) {
		  if (node.element.element == null && funNodes.valueOut(out var nodes)) {
			  nodes.Remove(node);
			  exportTimelineToTweenManager();
			  selectedNodeIndex = 0;
			  refreshTimeline();
		  }
	  }

	  void addFunTweenManagerComponent(GameObject gameObject) {
		  gameObject.GetComponent<FunTweenManager>().opt().voidFold(
			  () => {
				  funTweenManager = gameObject.AddComponent<FunTweenManager>().some();
				  refreshTimeline();
				  EditorUtility.SetDirty(gameObject);
			  },
			  fun => funTweenManager = fun.some()
			);
	  }
  }
}
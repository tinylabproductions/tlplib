using System.Collections.Generic;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class TimelineEditor : EditorWindow {
    Timeline timelineVisuals;
	  ExternalEditor advancedEditor;
	  
		Option<GameObject> selectedGameObjectOpt;
	  Option<FunTweenManager> funTweenManager = F.none_;
	  Option<List<FunSequenceNode>> funNodes;
	  Option<FunSequenceNode> selectedFunNodeOpt => funNodes.flatMap(
		  nodes => nodes[selectedNodeIndex].opt()
		);

		int selectedNodeIndex, snappingPower = 15, selectedNodeOutlineWidth = 3;
	  float settingsWidth, timeClickOffset;
	  bool resizeNodeStart, resizeNodeEnd, isLeftSnapped, isRightSnapped, dragNode;
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

			advancedEditor = CreateInstance<ExternalEditor>();
			funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
			
			if (funTweenManager.isSome) refreshTimeline();
			timelineVisuals.onSettingsGUI = onSettings;
			timelineVisuals.onTimelineGUI = drawFunNodes;
			selectedNodeIndex = int.MaxValue;
			if (selectedGameObjectOpt.isNone)
				onSelectionChange();
		}
	  
		void onSelectionChange() {
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
					refreshChannelNodes();
				}
			);
			
			selectedNodeIndex = 0;
			Repaint();
		}
	  
	  void OnGUI() {
			var enabled = GUI.enabled;
			GUI.enabled = selectedGameObjectOpt.isSome && !Application.isPlaying;
			timelineVisuals.DoTimeline(new Rect(0, 0, position.width, position.height));
			GUI.enabled = enabled;
			if (funTweenManager.valueOut(out var ftm)
				&& advancedEditor.Instances.Length > 0
				&& advancedEditor.Draw(new Rect(0, 45, settingsWidth, settingsWidth))
			) {
				Undo.RecordObject(ftm, "Tween Manager Changes");
				 Repaint(); 
			}
		  if (GUI.changed) {
			  refreshTimeline();
			  refreshChannelNodes();
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
				  foreach (var funNode in nodes) {
					  EditorGUIUtility.AddCursorRect(
						  new Rect(timelineVisuals.SecondsToGUI(funNode.startTime) - 5, funNode.channel * 20, 10, 20),
						  MouseCursor.ResizeHorizontal);
					  EditorGUIUtility.AddCursorRect(
						  new Rect(timelineVisuals.SecondsToGUI(funNode.startTime + funNode.duration) - 5, funNode.channel * 20, 10, 20),
						  MouseCursor.ResizeHorizontal);
					  EditorGUIUtility.AddCursorRect(
						  new Rect(timelineVisuals.SecondsToGUI(funNode.startTime), funNode.channel * 20,
							  timelineVisuals.SecondsToGUI(funNode.duration), 20),
						  MouseCursor.Pan);
				  }
			  	Option<Rect> snapIndicatorOpt = F.none_;
			  
				  foreach (var node in nodes) {
					  var boxRect = new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20,
						  timelineVisuals.SecondsToGUI(node.duration), 20);

					  var currentNodeIsSelected = selectedFunNodeOpt.valueOut(out var selectedNode) && selectedNode == node;
					  
					  if (currentNodeIsSelected) {
						  EditorGUI.DrawRect(boxRect, Color.magenta);
						  GUI.Box(new Rect(
							  boxRect.x + selectedNodeOutlineWidth,
							  boxRect.y + selectedNodeOutlineWidth,
							  boxRect.width - selectedNodeOutlineWidth * 2,
							  boxRect.height - selectedNodeOutlineWidth * 2
							), "", "TL LogicBar 0");

						  if (isLeftSnapped) {
							  snapIndicatorOpt = new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20 - 10, 2, 40).some();
						  }
						  if (isRightSnapped) {
							  snapIndicatorOpt = new Rect(timelineVisuals.SecondsToGUI(node.getEnd()), node.channel * 20 - 10, 2, 40).some();
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
				  EditorGUI.DrawRect(snapIndicator, Color.green);
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
										var genericMenu = new GenericMenu();
										genericMenu.AddItem(new GUIContent("Remove"), false, removeTween, node);
										genericMenu.ShowAsContext();
										break;
								}

								ev.Use();
							}
						}
					}
					break;
				case EventType.MouseDrag:

					foreach (var selectedFunNode in selectedFunNodeOpt) {
						
						if (resizeNodeStart) {
							snapToLeft(selectedFunNode, ev);
							selectedFunNode.changeDuration();
							ev.Use();
						}

						if (resizeNodeEnd) {
							selectedFunNode.duration = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) - selectedFunNode.startTime;
							selectedFunNode.duration = Mathf.Clamp(selectedFunNode.duration, 0.01f, float.MaxValue);
							
							snapToRight(selectedFunNode);
							selectedFunNode.changeDuration();
							recalculateTimelineWidth();

							ev.Use();
						}

						if (dragNode && !resizeNodeStart && !resizeNodeEnd) {
							selectedFunNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) + timeClickOffset;
							selectedFunNode.startTime = Mathf.Clamp(selectedFunNode.startTime, 0, float.MaxValue);
							isRightSnapped = false;
							isLeftSnapped = false;

							if (getRightOfSelectedNode().valueOut(out var rightNode)){
										setSnapping(
											snapPivot: timelineVisuals.SecondsToGUI(rightNode.startTime),
											positionToCheck: timelineVisuals.SecondsToGUI(selectedFunNode.getEnd()),
											resultToSet: rightNode.startTime - selectedFunNode.duration,
											sideToSnap: ref isRightSnapped
										);
									}

							if (getLeftOfSelectedNode().valueOut(out var leftNode)) {
									setSnapping(
										snapPivot: timelineVisuals.SecondsToGUI(leftNode.getEnd()),
										positionToCheck: timelineVisuals.SecondsToGUI(selectedFunNode.startTime),
										resultToSet: leftNode.getEnd(),
										sideToSnap: ref isLeftSnapped
									);
							}

							void setSnapping(float snapPivot, float positionToCheck, float resultToSet, ref bool sideToSnap) {
								if (positionToCheck < snapPivot + snappingPower && positionToCheck > snapPivot - snappingPower) {
									selectedFunNode.startTime = resultToSet;
									sideToSnap = true;
								}
							}

							if (Event.current.mousePosition.y > selectedFunNode.channel * 20 + 25) {
								selectedFunNode.channel += 1;
							}

							if (Event.current.mousePosition.y < selectedFunNode.channel * 20 - 5) {
								selectedFunNode.channel -= 1;
							}

							selectedFunNode.channel = Mathf.Clamp(selectedFunNode.channel, 0, int.MaxValue);
							selectedFunNode.element.timelineChannelIdx = selectedFunNode.channel;
							
							ev.Use();
					}
						refreshChannelNodes();
						refreshTimeline();
						exportTimelineToTweenManager();
					}

					break;
				case EventType.MouseUp:
					isLeftSnapped = false;
					isRightSnapped = false;
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
	  
	  void snapToRight(FunSequenceNode selectedFunNode) {
		  isRightSnapped = false;
		  if (getRightOfSelectedNode().valueOut(out var rightNode)) {
			  var startGUIseconds = timelineVisuals.SecondsToGUI(rightNode.startTime);
			  var snapPivot = timelineVisuals.SecondsToGUI(selectedFunNode.duration + selectedFunNode.startTime);
			  if ( snapPivot < startGUIseconds + snappingPower && snapPivot > startGUIseconds - snappingPower) {
				  selectedFunNode.duration = rightNode.startTime - selectedFunNode.startTime;
				  isRightSnapped = true;
			  }
		  }
	  }

	  //TODO: its pretty ugly
	  void snapToLeft(FunSequenceNode selectedFunNode, Event currentEvent) {
		  getLeftOfSelectedNode()
			  .voidFold(
				  () => {
					  selectedFunNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
					  selectedFunNode.startTime = Mathf.Clamp(selectedFunNode.startTime, 0, float.MaxValue);
					  if (selectedFunNode.startTime > 0) {
						  selectedFunNode.duration -= timelineVisuals.GUIToSeconds(currentEvent.delta.x);
						  selectedFunNode.duration = Mathf.Clamp(selectedFunNode.duration, 0.01f, float.MaxValue);
					  }
				  },
				  leftNode => {
					  var mousePosX = Event.current.mousePosition.x;
					  var end = selectedFunNode.getEnd();
					  var leftNodeEndGUIseconds = timelineVisuals.SecondsToGUI(leftNode.getEnd());
					  if (mousePosX < leftNodeEndGUIseconds + snappingPower && mousePosX > leftNodeEndGUIseconds - snappingPower) {
						  selectedFunNode.startTime = leftNode.getEnd();
						  selectedFunNode.startTime = Mathf.Clamp(selectedFunNode.startTime, 0, float.MaxValue);
						  if (!isLeftSnapped) {
							  selectedFunNode.duration = end - selectedFunNode.startTime;
							  isLeftSnapped = true;
						  }
					  }
					  else {
						  selectedFunNode.startTime = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
						  selectedFunNode.startTime = Mathf.Clamp(selectedFunNode.startTime, 0, float.MaxValue);
						  if (isLeftSnapped) {
							  selectedFunNode.duration = end - selectedFunNode.startTime;
							  isLeftSnapped = false;
						  }
						  else {
								  selectedFunNode.duration -= timelineVisuals.GUIToSeconds(currentEvent.delta.x);
							  }
							  selectedFunNode.duration = Mathf.Clamp(selectedFunNode.duration, 0.01f, float.MaxValue);
						  }
				  }
				);
	  }
	  
	  //imports element info from FunTweenManager element array
	  void refreshTimeline() {
		  advancedEditor.Instances = new object[] { };
		  if (funTweenManager.valueOut(out var manager)) {
			  var elements = manager.timeline.elements;

			  funNodes = manager.timeline.elements.Select(
				  (element, idx) => {
					  if (idx != 0 && element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement
					  ) {
						  element.timelineChannelIdx = elements[idx - 1].timelineChannelIdx;
					  }

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

	  Option<FunSequenceNode> getLeftOfSelectedNode() => selectedFunNodeOpt.flatMap(
		  selectedNode => funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime < selectedNode.startTime
				  ).noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).Last()
					)
		  )
		);
	  Option<FunSequenceNode> getRightOfSelectedNode() => selectedFunNodeOpt.flatMap(
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
		  if (selectedFunNodeOpt.valueOut(out var selected)) {
			  getLeftOfSelectedNode().voidFold(
				  () => {
					  selected.element.startAt = SerializedTweenTimeline.Element.At.SpecificTime;
					  selected.setTimeOffset(selected.startTime);
				  },
				  latestNode => {
					  switch (selected.element.startAt) {
						  case SerializedTweenTimeline.Element.At.SpecificTime:
							  selected.setTimeOffset(selected.startTime);
							  break;
						  case SerializedTweenTimeline.Element.At.AfterLastElement:
							  selected.setTimeOffset(selected.startTime - latestNode.getEnd());
							  break;
					  }
				  }
			  );
		  }
	  }

	  void exportTimelineToTweenManager() {
		  if (funTweenManager.valueOut(out var manager) && funNodes.valueOut(out var nodes))
				  foreach (var channel in
					  nodes.OrderBy(node => node.channel).noneIfEmpty().map(ordered => ordered.Last().channel)) {
							var arr = new List<FunSequenceNode>();
							for (var i = 0; i <= channel; i++) {
								arr.AddRange(
									nodes.FindAll(node => node.channel == i).OrderBy(node => node.startTime)
								);
							}
					  manager.timeline.elements = arr.Select(elem => elem.element).ToArray();
			  }
			//We need to find new selected node index after importing our elements into FunTweenManager
			if (selectedFunNodeOpt.valueOut(out var selectedNode)){
				foreach (var idx in manager.timeline.elements.indexWhere(element => element == selectedNode.element)) {
					if (selectedNodeIndex != idx) {
						selectedNodeIndex = idx;
						refreshTimeline();
					}
				}
			}
	  }
	  
	  void setSelectedNodeStartType(SerializedTweenTimeline.Element.At newStartType) {
		  if (selectedFunNodeOpt.valueOut(out var selected) && getLeftOfSelectedNode().valueOut(out var leftNode)) {
			  switch (newStartType) {
				  case SerializedTweenTimeline.Element.At.AfterLastElement: {
					  selected.setTimeOffset(selected.startTime - leftNode.getEnd());
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.SpecificTime: {
					  selected.setTimeOffset(selected.startTime);
					  break;
				  }
				  case SerializedTweenTimeline.Element.At.WithLastElement: {
					  selected.setTimeOffset(selected.startTime - leftNode.startTime);
					  break;
				  }
			  }
			  selected.element.startAt = newStartType;
		  }
		  
	  }
	  

	  void onSettings(float width) {
		  GUILayout.BeginHorizontal();
		  if (funTweenManager.isNone) {
			  if (GUILayout.Button("[Add manager]")) {
				  addFunTweenManagerComponent(Selection.activeGameObject);
				  EditorGUIUtility.ExitGUI();
			  }
		  }
		  else if (selectedFunNodeOpt.valueOut(out var selectedNode)) {
			  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement) {
				  GUI.enabled = false;
			  }

			  if (GUILayout.Button("After Last")) {
				  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.AfterLastElement); }
			  GUI.enabled = true;

			  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.WithLastElement) {
				  GUI.enabled = false;
			  }

			  if (GUILayout.Button("With Last")) {
				  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.WithLastElement); }
			  GUI.enabled = true;
			  
			  if (selectedNode.element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
				  GUI.enabled = false;
			  }

			  if (GUILayout.Button("Specific")) {
				  setSelectedNodeStartType(SerializedTweenTimeline.Element.At.SpecificTime); }
			  GUI.enabled = true;
		  }
		  GUILayout.EndHorizontal();
		  
			GUILayout.BeginVertical();
			settingsScroll = GUILayout.BeginScrollView(settingsScroll);
		  if (funTweenManager.isSome) {
			  drawElementSettings();
		  }

		  GUILayout.EndScrollView();

		  settingsWidth = width;
			GUILayout.FlexibleSpace();
		  GUILayout.BeginHorizontal();

		  GUILayout.EndHorizontal();

			var enabled = GUI.enabled;

		  if (GUILayout.Button("Add Tween")) {
			  foreach (var manager in funTweenManager) {
				  var newElement = new SerializedTweenTimeline.Element {startAt = SerializedTweenTimeline.Element.At.SpecificTime};
				  manager.timeline.elements = manager.timeline.elements.addOne(newElement);
				  refreshTimeline();
				  selectedNodeIndex = manager.timeline.elements.Length - 1;
			  }
			}
		  
		  if (GUILayout.Button("Refresh Timeline")) {
			  refreshTimeline();
		  }
		  
		  if (GUILayout.Button("Refresh manager")) {
			  exportTimelineToTweenManager();
		  }

			GUI.enabled = enabled;
			GUILayout.EndVertical();
		}

		void drawElementSettings() {
			foreach (var manager in funTweenManager) {
				if (selectedNodeIndex < manager.timeline.elements.Length) {
					advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
					advancedEditor.Instances = new object[] {
						manager.timeline.elements[selectedNodeIndex]
					};
				}
			}
		}
		void removeTween(object node) {
			var mNode = node as FunSequenceNode;
			funNodes.get.Remove(mNode);
			exportTimelineToTweenManager();
			selectedNodeIndex = 0;
			refreshTimeline();
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
using System;
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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineEditor : EditorWindow {
	  [Record]
	  partial struct nodeSnappedTo {
		  public readonly FunSequenceNode node;
		  public readonly bool snappedToStart;
	  }

	  [Record]
	  partial struct callbackVisuals {
		  public readonly Rect iconRect;
		  public readonly Rect labelRect;
		  public readonly GUIContent labelContent;

		  public callbackVisuals (Rect boxRect, GUIContent labelContent) {
			  iconRect = new Rect(boxRect.x - 10, boxRect.y, 20, boxRect.height);
			  labelRect = new Rect(boxRect.x - 30, boxRect.y, 60, boxRect.height);
			  this.labelContent = labelContent;
		  }
	  }

	  public readonly List<FunSequenceNode> selectedNodesList = new List<FunSequenceNode>();
	  readonly List<callbackVisuals> callbackVisualsList = new List<callbackVisuals>();

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
	  float timeClickOffset;
	  bool resizeNodeStart, resizeNodeEnd, dragNode, snapping, isStartSnapped, isEndSnapped;
		Vector2 settingsScroll;
	  Timeline timelineVisuals;
	  ExternalEditor advancedEditor;
	  Option<GameObject> selectedGameObjectOpt;
	  Option<nodeSnappedTo> nodeSnappedToOpt;

		[MenuItem("TLP/TweenTimeline", false)]
		public static void ShowWindow() {
			var window = GetWindow<TimelineEditor>(false, "Tween Timeline");
			window.wantsMouseMove = true;
			//DontDestroyOnLoad(window);
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

	  void EditorSceneManagerOnSceneSaving(Scene scene, string path) {
		  funNodes = F.none_;
		  timelineVisuals.stopVisualization();
	  }

	  void OnSelectionChange() {
		  selectedGameObjectOpt = Selection.activeGameObject.opt();
			funTweenManager = selectedGameObjectOpt.flatMap(selected => selected.GetComponent<FunTweenManager>().opt());
		  
			funTweenManager.voidFold(
				() => {
					funNodes = F.none_;
				},
				manager  => {
					timelineVisuals = new Timeline {
						onSettingsGUI = onSettings,
						onTimelineGUI = drawFunNodes
					};
					advancedEditor = CreateInstance<ExternalEditor>();
					advancedEditor.unityObjects = new UnityEngine.Object[] {manager};
					funNodes = F.none_;
					importTimeline();
				});
			
			selectedNodeIndex = 0;
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

	  void drawFunNodes(Rect position) {
		  //GUI.enabled = !timelineVisuals.visualizationMode;

		  if (funNodes.valueOut(out var nodes) && funTweenManager.isSome) {
			  Option<Rect> snapIndicatorOpt = F.none_;
			  callbackVisualsList.Clear();
			  var indicatorColor = Color.green;

			  foreach (var node in nodes) {
				  if (node.element.element != null) {
					  if (!node.isCallback) {
						  EditorGUIUtility.AddCursorRect(
							  new Rect(timelineVisuals.secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20),
							  MouseCursor.ResizeHorizontal);
						  EditorGUIUtility.AddCursorRect(
							  new Rect(timelineVisuals.secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20),
							  MouseCursor.ResizeHorizontal);
					  }
					  EditorGUIUtility.AddCursorRect(
						  new Rect(
							  timelineVisuals.secondsToGUI(node.startTime),
							  node.channel * 20,
							  timelineVisuals.secondsToGUI(node.duration <= 0.5f ? 1 : node.duration), 20
						  ),
						  MouseCursor.Pan);

					  var boxRect = new Rect(timelineVisuals.secondsToGUI(node.startTime), node.channel * 20,
						  timelineVisuals.secondsToGUI(node.duration), 20);

					  var currentNodeIsSelected = !selectedNodesList.isEmpty() &&
						  selectedNodesList.find(x => x == node).isSome;

					  var iconRect = new Rect(boxRect.x - 10, boxRect.y, 20, boxRect.height);
					  var tooltip = new GUIContent(EditorGUIUtility.FindTexture("tranp"), node.name);

					  void drawOutline(Rect aroundRect, Color outlineColor) {
							  
						  if (!node.isCallback) {
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
								drawCallbackIcon(new callbackVisuals(boxRect, tooltip));
						  }
					  }

					  if (!selectedNodesList.isEmpty() &&
						  selectedNodesList.find(x => x == node).valueOut(out var selectedNode)
					  ) {
						  drawOutline(boxRect, Color.magenta);
						  
						  if ( (isEndSnapped || isStartSnapped)
							  && rootSelectedNode.valueOut(out var rootNode)
							  && rootNode == node
							  ) {
							  foreach (var nodeSnappedTo in nodeSnappedToOpt) {
								  var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
								  var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * 20;
								  snapIndicatorOpt =
									  selectedIsHigher
										  ? getIndicatorRect(selectedNode, isStartSnapped, distance).some()
										  : getIndicatorRect(nodeSnappedTo.node, nodeSnappedTo.snappedToStart, distance).some();

								  Rect getIndicatorRect(FunSequenceNode nawd, bool isSnappedToStart, float dist) =>
									  new Rect(timelineVisuals.secondsToGUI(
											  isSnappedToStart
												  ? nawd.startTime
												  : nawd.getEnd()),
										  nawd.channel * 20 - 10, 2, dist
									  );
							  }

							  indicatorColor = isEndSnapped ? Color.yellow : Color.cyan;
						  }
					  }
					  else if (!node.isCallback) {
						  GUI.Box(boxRect, "", "TL LogicBar 0");
					  }
					  
					  if (node.isCallback) {
						  callbackVisualsList.Add(new callbackVisuals(boxRect, tooltip));
					  }

					  if (node.element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement) {
						  drawOutline(boxRect, Color.green);
						  if (getLeftNode(node).valueOut(out var leftNode) && node.linkedNode.valueOut(out var linkedTo) && leftNode == linkedTo) {
							  EditorGUI.DrawRect(
								  new Rect(timelineVisuals.secondsToGUI(leftNode.getEnd()),
									  node.channel * 20 + 10, timelineVisuals.secondsToGUI(node.startTime - leftNode.getEnd()), 2),
								  Color.green
							  );
							  EditorGUI.DrawRect(
								  new Rect(timelineVisuals.secondsToGUI(leftNode.getEnd()),
									  node.channel * 20, 3, 20),
								  Color.green
							  );
						  }
					  }
					  
					  if ( (node.element.element.getTargets().Length == 0
							  || node.element.element.getTargets().Any(target => target == null)) && !node.isCallback
					  ) {
						  drawOutline(boxRect, Color.red);
					  }

					  var style = new GUIStyle("Label");
					  style.fontSize = currentNodeIsSelected ? 12 : style.fontSize;
					  style.fontStyle = FontStyle.Bold;
					  var color = currentNodeIsSelected ? Color.magenta : node.nodeTextColor;
					  color.a = currentNodeIsSelected ? 1.0f : 0.7f;
					  style.normal.textColor = color;
					  Vector3 size = style.CalcSize(new GUIContent($"content: {node.name}"));
					  var labelPosX = Mathf.Clamp(boxRect.x + boxRect.width / 2 - size.x / 3 , boxRect.x, boxRect.x + boxRect.width);
					  var rect1 = new Rect(labelPosX,
						  boxRect.y + boxRect.height * 0.5f - size.y * 0.5f, Mathf.Clamp(size.x, 0, boxRect.width + (boxRect.x - labelPosX)), size.y);

					  GUI.Label(rect1, $"{node.name}", style);
				  }

				  if (snapIndicatorOpt.valueOut(out var snapIndicator)) {
					  EditorGUI.DrawRect(snapIndicator, indicatorColor);
				  }

				  foreach (var callbackVisual in callbackVisualsList) {
					  drawCallbackIcon(callbackVisual);
				  }
				  
				  void drawCallbackIcon(callbackVisuals visuals) {
					  GUI.color = Color.yellow;
					  GUI.DrawTexture(visuals.iconRect,
						  EditorGUIUtility.FindTexture("d_animationkeyframe"));
					  GUI.Label(visuals.labelRect, visuals.labelContent);
					  GUI.color = Color.white;
				  }

				  doEvents();
			  }
		  }
	  }

	  void addSelectedNode(FunSequenceNode nodeToAdd, Event currentEvent) {
		  if (!selectedNodesList.isEmpty()) {
				selectedNodesList.find(selectedNode => selectedNode == nodeToAdd).voidFold(
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
			var currEvent = Event.current;
			var snappingEnabled = !Event.current.shift && snapping;
			if (currEvent.control && currEvent.keyCode == KeyCode.A && currEvent.type == EventType.keyUp) {
				selectedNodesList.Clear();
				foreach (var nodes in funNodes) {
					foreach (var node in nodes) {
						selectedNodesList.Add(node);
					}
				}
			}

			if (currEvent.keyCode == KeyCode.Delete && currEvent.type == EventType.keyDown) {
				removeoAllSelectedNodes();
				selectedNodesList.Clear();
				importTimeline();
			}

			if (!timelineVisuals.visualizationMode) {
				switch (currEvent.rawType) {
					case EventType.MouseDown:
						//Clearing up nodes list from nodes without element.element
						funNodes = funNodes.map(nodes => nodes.Where(node => node.element.element != null).ToList());

						foreach (var nodes in funNodes) {
							for (var i = 0; i < nodes.Count; i++) {
								var node = nodes[i];

								if (new Rect(timelineVisuals.secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20).Contains(Event.current
									.mousePosition) && !node.isCallback) {
									removeNodeIfHasNoElement(nodes[selectedNodeIndex]);

									addSelectedNode(node, Event.current);
									selectedNodeIndex = i;
									resizeNodeStart = true;
									EditorGUI.FocusTextInControl("");
									currEvent.Use();
								}

								if (new Rect(timelineVisuals.secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20)
									.Contains(
										Event.current.mousePosition) && !node.isCallback) {
									removeNodeIfHasNoElement(nodes[selectedNodeIndex]);
									addSelectedNode(node, Event.current);
									selectedNodeIndex = i;
									resizeNodeEnd = true;
									EditorGUI.FocusTextInControl("");
									currEvent.Use();
								}

								if ( new Rect(
									timelineVisuals.secondsToGUI(node.startTime),
									node.channel * 20,
									timelineVisuals.secondsToGUI(node.duration <= 0.5f ? 1 : node.duration), 20
								).Contains(Event.current.mousePosition)) {

									switch (currEvent.button) {
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

									currEvent.Use();
								}

								if (rootSelectedNode.valueOut(out var root) && root == node
									&& !dragNode && !resizeNodeStart && !resizeNodeEnd && !currEvent.control
									&& timelineVisuals.drawRect.Contains(Event.current.mousePosition)
								) {
//									removeNodeIfHasNoElement(node);
									selectedNodesList.Clear();
									currEvent.Use();
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
											selectedNode.duration = selectedNodeEnd - selectedNode.startTime;
											selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);
										}

										if (snappingEnabled) {
											snapStart(selectedNode, selectedNodesList, selectedNodeEnd);
										}

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
									if (rootSelectedNode.valueOut(out var rootSelected) && selectedNode == rootSelected) {
										selectedNode.duration = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x) - selectedNode.startTime;
										selectedNode.duration = Mathf.Clamp(selectedNode.duration, 0.01f, float.MaxValue);

										if (snappingEnabled) {
											snapEnd(selectedNode);
										}

										foreach (var node in selectedNodesList) {
											if (node != rootSelected) {
												node.duration = rootSelected.duration - (node.startTime - rootSelected.startTime);
												node.duration = Mathf.Clamp(node.duration, 0.01f, float.MaxValue);
											}
											updateLinkedNodeStartTimes(node);

										}

										timelineVisuals.recalculateTimelineWidth(funNodes);
										

										currEvent.Use();
									}
								}

								//Draging the node
								if (dragNode && !resizeNodeStart && !resizeNodeEnd || resizeNodeEnd && resizeNodeStart) {
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

										void updateLinkedNodeChannels(FunSequenceNode node, Act<FunSequenceNode> changeChannel) {
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
								timelineVisuals.recalculateTimelineWidth(funNodes);
								refreshChannelNodes();
								exportTimelineToTweenManager();
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
		}

	  void unlinkNodesWithBrokenLink() {
		  if (funNodes.valueOut(out var nodes)) {
			  foreach (var node in nodes) {
				  if (node.linkedNode.valueOut(out var linkedNode)
					  && getLeftNode(node).valueOut(out var leftNode)
					  && linkedNode != leftNode
				  ) {
					  node.unlink();
				  }
			  }
		  }
	  }
	  
	  void updateLinkedNodeStartTimes(FunSequenceNode node) {
		  getLinkedRightNode(node).voidFold(
			  () => { },
			  rightNode => {
				  if (rightNode.linkedNode.valueOut(out var nodeLinkedTo) && nodeLinkedTo == node)  {
					  rightNode.startTime = node.getEnd() + rightNode.element.timeOffset;
				  }

				  updateLinkedNodeStartTimes(rightNode);
			  });
	  }
	  
	  Option<FunSequenceNode> getLinkedRightNode(FunSequenceNode node) =>
		  getRightNode(node).flatMap(rightNode =>
			  rightNode.linkedNode.valueOut(out var linkedNode)
				&& linkedNode == node
			  && selectedNodesList.find(x => x == rightNode).isNone
				  ? rightNode.some()
				  : F.none_
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
					if ( isInRangeOfSnap( timelineVisuals.secondsToGUI(nodeToSnapTo.startTime),
						timelineVisuals.secondsToGUI(rootNode.getEnd()) )
					) {
						setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime - rootNode.duration, ref isEndSnapped);
					}
					//Snapping selected end with node end
					else if ( isInRangeOfSnap( timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd()),
						timelineVisuals.secondsToGUI(rootNode.getEnd()) )
					) {
						setSnapping(nodeToSnapTo, false, nodeToSnapTo.getEnd() - rootNode.duration, ref isEndSnapped);
					}
					//Snapping selected start with node start
					else if ( isInRangeOfSnap( timelineVisuals.secondsToGUI(nodeToSnapTo.startTime),
						timelineVisuals.secondsToGUI(rootNode.startTime) )
					) {
						setSnapping(nodeToSnapTo, true, nodeToSnapTo.startTime, ref isStartSnapped);
					}
					//Snapping selected start end with node end
					else if ( isInRangeOfSnap( timelineVisuals.secondsToGUI(nodeToSnapTo.getEnd()),
						timelineVisuals.secondsToGUI(rootNode.startTime) )
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
						var snapPivot = timelineVisuals.secondsToGUI(laterNode.startTime);
						var nodeEndPos = timelineVisuals.secondsToGUI(selectedNode.getEnd());
						if ( nodeEndPos < snapPivot + SNAPPING_POWER && nodeEndPos > snapPivot - SNAPPING_POWER ) {
							selectedNode.duration = laterNode.startTime - selectedNode.startTime;
							isEndSnapped = true;
							nodeSnappedToOpt = new nodeSnappedTo(laterNode, true).some();
						}
						else {
							snapPivot = timelineVisuals.secondsToGUI(laterNode.getEnd());
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
			  //Filtering out unviable to snap start nodes
			  //and looking for a node to snap to if we are not already snapped
			  nodes.Except(selectedNodesList).Where(node =>
				  selectedNode.getEnd() >= node.startTime
			  ).ToList().ForEach(earlierNode => nodeSnappedToOpt.voidFold(
				  () => snap(earlierNode),
				  nodeSnapped => snap(nodeSnapped.node)
			  ));
			  
			  void snap(FunSequenceNode nodeToSnap) {
				  var end = selectedNode.getEnd();
				  var snapPivot = timelineVisuals.secondsToGUI(nodeToSnap.startTime);
				  var nodeStartPos = timelineVisuals.secondsToGUI(selectedNode.startTime);
				  if (nodeStartPos < snapPivot + SNAPPING_POWER && nodeStartPos > snapPivot - SNAPPING_POWER) {
					  selectedNode.startTime = nodeToSnap.startTime;
					  if (!isStartSnapped) {
						  selectedNode.duration = end - selectedNode.startTime;
						  nodeSnappedToOpt = new nodeSnappedTo(nodeToSnap, true).some();
						  isStartSnapped = true;
					  }
				  }
				  else {
					  snapPivot = timelineVisuals.secondsToGUI(nodeToSnap.getEnd());
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

	  //creates nodeList from elements info
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

					  if (element.title == "") element.title =
						  element.element == null
							  ? "" :
							  element.element.GetType().Name;
					  
					  var newNode = new FunSequenceNode(element, whereToStart(idx), element.title);
					  //Might have unwanted consequences
					  if (funNodes.flatMap(nodes => nodes.find(x => x.element == element)).valueOut(out var foundNode)) {
						  foundNode.duration = newNode.duration;
						  foundNode.startTime = newNode.startTime;
						  foundNode.name = newNode.name;
						  foundNode.channel = newNode.channel;
						  foundNode.isCallback = newNode.isCallback;
						  return foundNode;
					  }
					  else {
						  return newNode;
					  }
				  }
			  ).ToList().some();

			  //Relinking linked nodes, since we dont serialize nodeLinkedTo
			  foreach (var nodes in funNodes) {
					foreach (var node in nodes) {
						if (getLeftNode(node).valueOut(out var leftNode)) {
							node.reLink(leftNode);
							node.refreshColor();							
						}
					}
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

	  Option<FunSequenceNode> getLeftNode(FunSequenceNode selectedNode) => 
		  funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime < selectedNode.startTime
				  ).ToList().noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).Last()
					)
			);

	  Option<FunSequenceNode> getRightNode(FunSequenceNode selectedNode) =>
		  funNodes.flatMap(
			  nodes =>
				  nodes.Where(node => node.channel == selectedNode.channel
					  && node.startTime > selectedNode.getEnd()
					).ToList().noneIfEmpty().map(
					  channelNodes => channelNodes.OrderBy(channelNode => channelNode.startTime).First()
					)
			);

	  void refreshChannelNodes() {
		  if (!selectedNodesList.isEmpty()) {
			  foreach (var selected in selectedNodesList) {
				  getLeftNode(selected).voidFold(
					  () => selected.setSelectedNodeElementFields(SerializedTweenTimeline.Element.At.SpecificTime),
					  leftNode => {
						  if (selected.linkedNode.valueOut(out var linkedNode) && linkedNode == leftNode) {
							  selected.setSelectedNodeElementFields(SerializedTweenTimeline.Element.At.AfterLastElement);
						  }
						  else {
							  selected.setSelectedNodeElementFields(SerializedTweenTimeline.Element.At.SpecificTime);
						  }
					  });
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
	  
	  void onSettings(float width) {
		  if (funTweenManager.isSome) {
			  GUILayout.BeginVertical();
			  GUI.enabled = !timelineVisuals.visualizationMode;
			  
			  foreach (var nodes in funNodes) {
				  nodes.find(elem => elem.element.element == null).map(_ => GUI.enabled = false);
			  }

			  if (GUILayout.Button("Add Tween")) {
				  var newElement = new SerializedTweenTimeline.Element {
					  startAt = SerializedTweenTimeline.Element.At.SpecificTime
				  };
				  var newNode = new FunSequenceNode(newElement, 0, "");
				  
				  if (funNodes.valueOut(out var nodes)) {
					  nodes.Insert(0, newNode);
				  }
				  else {
					  funNodes = new List<FunSequenceNode>{newNode}.some();
				  }
				  selectedNodesList.Clear();
				  selectedNodesList.Add(newNode);
				  exportTimelineToTweenManager();
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

				  if (oneNodeSelected && rootSelectedNode.valueOut(out var selectedNode)) {
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
							  selectedNode.linkNodeTo(leftNode);
							  selectedNode.setSelectedNodeElementFields(SerializedTweenTimeline.Element.At.AfterLastElement);
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
		  GUI.enabled = !timelineVisuals.visualizationMode;
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
					}
				}
		}

	  void deselect(object obj) {
		  var node = obj as FunSequenceNode;
		  selectedNodesList.Remove(node);
		  Repaint();
	  }
	  
		void removeSelectedNode(object obj) {
			var nawd = obj as FunSequenceNode;
			funNodes = funNodes.flatMap(nodes =>
				nodes.Where(node => node != nawd).ToList().noneIfEmpty()
			);
			
			exportTimelineToTweenManager();
			selectedNodeIndex = 0;
		}

	  void removeoAllSelectedNodes() {
			funNodes = funNodes.flatMap(nodes =>
				nodes.Where(
					node => selectedNodesList.find(selectedNode => node == selectedNode).isNone).ToList().noneIfEmpty()
			);
			exportTimelineToTweenManager();
			selectedNodesList.Clear();
		  selectedNodeIndex = 0;
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
				  funTweenManager = Undo.AddComponent<FunTweenManager>(gameObject).some();
				  importTimeline();
				  EditorUtility.SetDirty(gameObject);
			  },
			  fun => funTweenManager = fun.some()
			);
	  }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Data.typeclasses;
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
		Option<GameObject> selectedGameObject;
		Tweener tweener;
	  Option<FunTweenManager> funTweenManager;
	  Option<List<FunSequenceNode>> funNodes;
	  
	  int selectedSequenceIndex;
	  int selectedTimelineIndex;
	  SequenceWrap wrap;

		Option<Sequence> sequence {
			get {
				if (tweener != null && selectedSequenceIndex < tweener.sequences.Count) {
					return tweener.sequences[0].some();
				}

				return F.none_;
			}
		}

		int selectedNodeIndex;
	  int snappingPower = 15;

	  Option<SequenceNode> selectedNodeOpt => sequence.map(seq => seq.nodes[selectedNodeIndex]);
	  Option<FunSequenceNode> selectedFunNodeOpt => funNodes.map(nodes => nodes[selectedNodeIndex]);

	  bool isPlaying;
		bool isRecording;
	  float playStartTime;
	  bool resizeNodeStart;
		bool resizeNodeEnd;
		bool dragNode;
		float timeClickOffset;
		Vector2 settingsScroll;
	  GameObject backupGameObject;
		int selectedEventIndex;

		 EventNode selectedEvent {
			get {
				if (sequence != null && selectedEventIndex < sequence.get.events.Count) {
					return sequence.get.events[selectedEventIndex];
				}

				return null;
			}
		}

		[MenuItem("TLP/VisualTweenTimeline", false)]
		public static void ShowWindow() {
			var window = GetWindow<TimelineEditor>(false, "VisualTweenTimeline");
			window.wantsMouseMove = true;
			DontDestroyOnLoad(window);
		}

	  ExternalEditor advancedEditor;
	  
		void OnEnable() {
			Undo.undoRedoPerformed += undoCalback;
			if (timelineVisuals == null) {
				timelineVisuals = new Timeline();
			}

			advancedEditor = CreateInstance<ExternalEditor>();
			funTweenManager = selectedGameObject.map(selected => selected.GetComponent<FunTweenManager>());
			
			if (funTweenManager.isSome) refreshTimeline();
			timelineVisuals.onRecord = OnRecord;
			timelineVisuals.onPlay = OnPlay;
			timelineVisuals.onSettingsGUI = OnSettings;
			timelineVisuals.onTimelineGUI = drawFunNodes;
			timelineVisuals.onTimelineClick = OnTimelineClick;
			timelineVisuals.onAddEvent = OnAddEvent;
			timelineVisuals.onEventGUI = OnEventGUI;
			selectedEventIndex = int.MaxValue;
			selectedNodeIndex = int.MaxValue;
			if (selectedGameObject.isNone)
				OnSelectionChange();

			EditorApplication.playmodeStateChanged += OnPlayModeStateChange;
		}

		void OnAddEvent() {
			addTweenerComponent(Selection.activeGameObject);
			if (sequence.isNone) {
				AddSequence(tweener);
				addTimeline(tweener.opt());
			}

			if (sequence.get.events == null) {
				sequence.get.events = new List<EventNode>();
			}

			GenericMenu menu = new GenericMenu();
			//Component[] components=selectedGameObject.GetComponents<Component>();
			List<Type> types = new List<Type>();
			//types.AddRange (components.Select (x => x.GetType ()));
			types.AddRange(GetSupportedTypes());
			foreach (Type type in types) {
				List<MethodInfo> functions = GetValidFunctions(type,
					!(type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(MonoBehaviour))) ||
					selectedGameObject.get.GetComponent(type) == null);
				foreach (MethodInfo mi in functions) {
					if (mi != null) {
						EventNode node = new EventNode();
						node.time = timelineVisuals.CurrentTime;
						node.SerializedType = type;
						node.method = mi.Name;
						node.arguments = GetMethodArguments(mi);
						menu.AddItem(new GUIContent(type.Name + "/" + mi.Name), false, AddEvent, node);
					}
				}
			}

			menu.ShowAsContext();
		}


		public List<MethodInfo> GetValidFunctions(Type type, bool staticOnly) {
			List<MethodInfo> validMethods = new List<MethodInfo>();
			List<MethodInfo> mMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();
			if (!staticOnly) {
				mMethods.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Instance));
			}

			for (int b = 0; b < mMethods.Count; ++b) {
				MethodInfo mi = mMethods[b];

				string name = mi.Name;
				if (name == "Invoke") continue;
				if (name == "InvokeRepeating") continue;
				if (name == "CancelInvoke") continue;
				if (name == "StopCoroutine") continue;
				if (name == "StopAllCoroutines") continue;
				if (name.StartsWith("get_")) continue;
				if (mi.ReturnType != typeof(void)) continue;
				if (mi.IsGenericMethod) continue;
				if (!IsArgumentValid(mi)) continue;

				validMethods.Add(mi);
			}

			return validMethods;
		}

		private bool isArgumentValid(Type type) {
			switch (type.ToString()) {
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.Single":
				case "System.Double":
				case "System.String":
				case "System.Boolean":
				case "UnityEngine.Vector2":
				case "UnityEngine.Vector3":
				case "UnityEngine.Vector4":
				case "UnityEngine.Color":
				case "UnityEngine.Rect":
				case "UnityEngine.AnimationCurve":
				case "UnityEngine.Space":
				case "UnityEngine.Quaternion":
				case "UnityEngine.SendMessageOptions":
					return true;
				default:
					if (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object))) {
						return true;
					}

					return false;
			}
		}

		private bool IsArgumentValid(MethodInfo mi) {
			ParameterInfo[] info = mi.GetParameters();
			foreach (ParameterInfo p in info) {
				return isArgumentValid(p.ParameterType);
			}

			return true;
		}

		private List<MethodArgument> GetMethodArguments(MethodInfo mi) {
			ParameterInfo[] pi = mi.GetParameters();
			List<MethodArgument> args = new List<MethodArgument>();
			foreach (ParameterInfo info in pi) {
				MethodArgument arg = new MethodArgument(info.Name, info.ParameterType.ToString());
				args.Add(arg);
			}

			return args;
		}

		public List<Type> GetSupportedTypes() {
			List<Type> types = new List<Type>();
			/*types.Add(typeof(Application));
			types.Add (typeof(GameObject));
			types.Add (typeof(Debug));*/

			types.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(t => t.GetTypes())
				.Where(t => t.Namespace == "UnityEngine"));

			return types;
		}

		private void AddEvent(object data) {
			EventNode node = data as EventNode;
			sequence.get.events.Add(node);
			EditorUtility.SetDirty(tweener);
		}

		void OnEventGUI(Rect rect) {
			foreach (var seq in sequence) {
				for (int i = 0; i < seq.events.Count; i++) {
					Rect rect1 = new Rect(timelineVisuals.SecondsToGUI(seq.events[i].time) - timelineVisuals.scroll.x + rect.x - 5f, rect.y, 17,
						20);
					if (rect1.x + 6f > rect.x) {
						Color color = GUI.color;
						if (i == selectedEventIndex) {
							Color mColor = Color.blue;
							GUI.color = mColor;
						}

						GUI.Label(rect1, "", (GUIStyle) "Grad Up Swatch");
						GUI.color = color;
					}
				}
			}
		}

		void OnPlayModeStateChange() { timelineVisuals.Stop(); }

		void OnDestroy() { UndoObject(); }

		void OnSelectionChange() {
			timelineVisuals.Stop();
			selectedGameObject = Selection.activeGameObject.opt();
			selectedGameObject.voidFold(
				() => tweener = null,
				selected => {
					tweener = selected.GetComponent<Tweener>();
					funTweenManager = selected.GetComponent<FunTweenManager>().opt();
					if (funTweenManager.isSome) refreshTimeline();
				});

			selectedNodeIndex = 0;
			Repaint();
		}

		bool playForward;
		float time;
		bool stop;

		void Update() {
			if (!Application.isPlaying) {
				if (isPlaying && !stop) {
					if ((float) EditorApplication.timeSinceStartup > time) {
						switch (wrap) {
							case SequenceWrap.PingPong:
								playForward = !playForward;
								time = (float) EditorApplication.timeSinceStartup + GetSequenceEnd();
								if (playForward) {
									timelineVisuals.CurrentTime = 0;
									playStartTime = (float) EditorApplication.timeSinceStartup;
								}

								break;
							case SequenceWrap.Once:
								sequence.get.Stop(false);
								playStartTime = (float) EditorApplication.timeSinceStartup;
								timelineVisuals.CurrentTime = 0;
								stop = true;
								break;
							case SequenceWrap.ClampForever:
								sequence.get.Stop(true);
								stop = true;
								break;
							case SequenceWrap.Loop:
								sequence.get.Stop(false);
								playStartTime = (float) EditorApplication.timeSinceStartup;
								timelineVisuals.CurrentTime = 0;
								stop = false;
								time = (float) EditorApplication.timeSinceStartup + GetSequenceEnd();
								break;
						}
					}

					timelineVisuals.CurrentTime = (playForward
						? ((float) EditorApplication.timeSinceStartup - playStartTime)
						: time - (float) EditorApplication.timeSinceStartup);
					//->
					//timelineVisuals.CurrentTime = (float)EditorApplication.timeSinceStartup - playStartTime;
					EditorUpdate(timelineVisuals.CurrentTime);
					Repaint();
				}

				if (isRecording) {
					EditorUpdate(timelineVisuals.CurrentTime);
				}
			}
			else {
				foreach (var seq in sequence) {
					if (tweener != null && tweener.IsPlaying(seq.name)) {
						timelineVisuals.CurrentTime = seq.passedTime;
						Repaint();
					}
				}
				
			}
		}

		public float GetSequenceEnd() {
			if (sequence.isNone) {
				return Mathf.Infinity;
			}

			float sequenceEnd = 0;
			foreach (var node in sequence.get.nodes) {
				if (sequenceEnd < (node.startTime + node.duration)) {
					sequenceEnd = node.startTime + node.duration;
				}
			}

			return sequenceEnd;
		}

	  void OnGUI() {
			var enabled = GUI.enabled;
			GUI.enabled = selectedGameObject.isSome && !Application.isPlaying;
			timelineVisuals.DoTimeline(new Rect(0, 0, position.width, position.height));
			GUI.enabled = enabled;
			if (funTweenManager.valueOut(out var ftm)
				&& advancedEditor.Instances.Length > 0
				&& advancedEditor.Draw(new Rect(0, 20, settingsWidth, settingsWidth))
			) {
				Undo.RecordObject(ftm, "Tween Manager Changes");
				 Repaint(); 
			}
		  if (GUI.changed) {
			  refreshTimeline();
			  refreshNodes();
			  Log.d.warn("editor gui changed aliooo");
		  }
	  }

	  void undoCalback() {
		  refreshTimeline();
		  refreshNodes();
		  Log.d.warn("undo/redo perfromed");
	  }

		void EditorUpdate(float time) {
			foreach (var seq in sequence) {
				seq.nodes = seq.nodes.OrderBy(x => x.startTime).ToList();
				foreach (SequenceNode node in seq.nodes) {
					node.UpdateTween(time);
				}
				//Canvas.ForceUpdateCanvases();
				EditorUtility.SetDirty(tweener);
			}
		}

		void OnTimelineClick(float time) {
			foreach (var seq in sequence) {
				foreach (SequenceNode node in seq.nodes) {
					if (time < node.startTime) {
						node.UpdateValue(0.0f);
					}

					if (time > node.startTime + node.duration) {
						node.UpdateValue(1.0f);
					}
				}
			}
		}

	  int selectedNodeOutlineWidth = 3;

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
							  snapIndicatorOpt = new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20 - 10, 4, 40).some();
						  }
						  if (isRightSnapped) {
							  snapIndicatorOpt = new Rect(timelineVisuals.SecondsToGUI(node.getEnd()), node.channel * 20 - 10, 4, 40).some();
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

		private bool dragEvent;
	  bool isLeftSnapped;
	  bool isRightSnapped;
		void DoEvents() {
			var ev = Event.current;
			switch (ev.rawType) {
				case EventType.MouseDown:
					for (var j = 0; j < sequence.__unsafeGetValue.events.Count; j++) {
						var rect1 = new Rect(timelineVisuals.SecondsToGUI(sequence.__unsafeGetValue.events[j].time) - 5f, -15, 17, 20);
						if (rect1.Contains(Event.current.mousePosition)) {
							selectedEventIndex = j;
							selectedNodeIndex = int.MaxValue;
							if (ev.button == 0) {
								dragEvent = true;
							}

							if (ev.button == 1) {
								var genericMenu = new GenericMenu();
								genericMenu.AddItem(new GUIContent("Remove"), false,
									delegate() { sequence.__unsafeGetValue.events.RemoveAt(selectedEventIndex); });
								genericMenu.ShowAsContext();
							}

							ev.Use();
						}
					}

					for (var i = 0; i < funNodes.get.Count; i++) {
						var node = funNodes.get[i];

						if (new Rect(timelineVisuals.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20).Contains(Event.current
							.mousePosition)) {
							selectedNodeIndex = i;
							selectedEventIndex = int.MaxValue;
							resizeNodeStart = true;
							EditorGUI.FocusTextInControl("");
							ev.Use();
						}

						if (new Rect(timelineVisuals.SecondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20).Contains(
							Event.current.mousePosition)) {
							selectedNodeIndex = i;
							selectedEventIndex = int.MaxValue;
							resizeNodeEnd = true;
							EditorGUI.FocusTextInControl("");
							ev.Use();
						}

						if (new Rect(timelineVisuals.SecondsToGUI(node.startTime), node.channel * 20, timelineVisuals.SecondsToGUI(node.duration), 20)
							.Contains(Event.current.mousePosition)) {
							if (ev.button == 0) {
								timeClickOffset = node.startTime - timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
								dragNode = true;
								selectedNodeIndex = i;
								selectedEventIndex = int.MaxValue;
								EditorGUI.FocusTextInControl("");
							}

							if (ev.button == 1) {
								var genericMenu = new GenericMenu();
								genericMenu.AddItem(new GUIContent("Remove"), false, RemoveTween, node);
								genericMenu.ShowAsContext();
							}

							ev.Use();
						}
					}

					break;
				case EventType.MouseDrag:
					if (dragEvent) {
						selectedEvent.time = timelineVisuals.GUIToSeconds(Event.current.mousePosition.x);
						selectedEvent.time = Mathf.Clamp(selectedEvent.time, 0, float.MaxValue);
						ev.Use();
					}

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
							if (ev.delta.x > 0) {
								if (getRightOfSelectedNode().valueOut(out var rightNode)){
											setSnapping(
												snapPivot: timelineVisuals.SecondsToGUI(rightNode.startTime),
												positionToCheck: timelineVisuals.SecondsToGUI(selectedFunNode.getEnd()),
												resultToSet: rightNode.startTime - selectedFunNode.duration,
												sideToSnap: ref isRightSnapped
											);
										}
							}
							else {
								if (getLeftOfSelectedNode().valueOut(out var leftNode)) {
										setSnapping(
											snapPivot: timelineVisuals.SecondsToGUI(leftNode.getEnd()),
											positionToCheck: timelineVisuals.SecondsToGUI(selectedFunNode.startTime),
											resultToSet: leftNode.getEnd(),
											sideToSnap: ref isLeftSnapped
										);
								}
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
						refreshNodes();
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
					dragEvent = false;
					recalculateTimelineWidth();
					break;
			}
		}

	  void recalculateTimelineWidth() => timelineVisuals.lastNodeTime = timelineVisuals.SecondsToGUI(
		  funNodes.fold(
		  	() => 0,
		  	nodes => nodes.Max(node => node.startTime + node.duration)
			)
	  );

	  bool backup;

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
	  
	  //imports info element info from FunTweenManager element array
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
	  
	  void refreshNodes() {
		  if (selectedFunNodeOpt.valueOut(out var selectedNode) && funNodes.valueOut(out var nodes)) {
			  nodes.Where(node => node.channel == selectedNode.channel
				  && node.startTime + node.duration < selectedNode.startTime
			  ).noneIfEmpty().voidFold(
				  () => {
					  selectedNode.element.startAt = SerializedTweenTimeline.Element.At.SpecificTime;
					  selectedNode.setTimeOffset(selectedNode.startTime);
				  },
				  channelNodes => {
					  var latestNode = channelNodes.OrderBy(channelNode => channelNode.startTime).Last();
					  selectedNode.setTimeOffset(selectedNode.element.startAt == SerializedTweenTimeline.Element.At.SpecificTime
						  ? selectedNode.startTime
						  : selectedNode.startTime - (latestNode.startTime + latestNode.duration)
					  );
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

		void OnRecord(bool isRecording) {
			if (tweener == null && isRecording) {
				foreach (var selected in selectedGameObject) {
					tweener = selected.AddComponent<Tweener>();
					AddSequence(tweener);
					//sequence.nodes = new List<SequenceNodeTemp>();
					EditorUtility.SetDirty(tweener);
				}
			}

			this.isRecording = isRecording;
			if (!isRecording) {
				OnTimelineClick(0.0f);
				UndoObject();
			}
			else {
				RecordObject();
			}

		}

		private void OnPlay(bool isPlaying) {
			playStartTime = (float) EditorApplication.timeSinceStartup;
			time = (float) EditorApplication.timeSinceStartup + GetSequenceEnd();
			timelineVisuals.CurrentTime = 0;

			this.isPlaying = isPlaying;
			if (!isPlaying) {
				OnTimelineClick(0.0f);
				UndoObject();
			}
			else {
				stop = false;
				playForward = true;
				RecordObject();
			}
		}

		private void RecordObject() {
			if (backup) {
				return;
			}

			backupGameObject = (GameObject) Instantiate(selectedGameObject.get);
			backupGameObject.transform.SetParent(selectedGameObject.get.transform.parent, false);
			backupGameObject.name = selectedGameObject.get.name;
			backupGameObject.SetActive(false);
			backupGameObject.hideFlags = HideFlags.HideInHierarchy;
			backup = true;
		}

		private void UndoObject() {
			if (!backup) {
				return;
			}

			Tweener mTweener = backupGameObject.GetComponent<Tweener>();
			mTweener.sequences = tweener.sequences;
			for (int i = 0; i < tweener.sequences.Count; i++) {
				for (int j = 0; j < tweener.sequences[i].nodes.Count; j++) {
					SequenceNode node = tweener.sequences[i].nodes[j];
					mTweener.sequences[i].nodes[j].target = mTweener.GetComponent(node.target.GetType());
				}
			}

			DestroyImmediate(selectedGameObject.get);
			selectedGameObject = backupGameObject.some();
			selectedGameObject.get.hideFlags = HideFlags.None;
			selectedGameObject.get.SetActive(true);
			Selection.activeGameObject = selectedGameObject.get;
			backup = false;
		}

	  float settingsWidth;

	  void OnSettings(float width) {

		  if (funTweenManager.isNone) {
			  GUILayout.BeginHorizontal();
			  if (GUILayout.Button("[Add manager]")) {
				  addTweenerComponent(Selection.activeGameObject);
				  addFunTweenManagerComponent(Selection.activeGameObject);
				  AddSequence(tweener);
				  addTimeline(tweener.opt());
				  EditorGUIUtility.ExitGUI();
			  }
			  GUILayout.EndHorizontal();
		  }

		  //-->
			GUILayout.BeginVertical();
			settingsScroll = GUILayout.BeginScrollView(settingsScroll);
			if (tweener != null) {
				drawElementSettings();
			}
		  
			GUILayout.EndScrollView();
		  settingsWidth = width;
			GUILayout.FlexibleSpace();
			var enabled = GUI.enabled;
			GUI.enabled = sequence.isSome;

		  if (GUILayout.Button("Add Tween")) {
			  foreach (var manager in funTweenManager) {
				  var oneMore = manager.timeline.elements.addOne(new SerializedTweenTimeline.Element());
				  manager.timeline.elements = oneMore;
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
		void RemoveTween(object node) {
			var mNode = node as FunSequenceNode;
			funNodes.get.Remove(mNode);
			funTweenManager.get.timeline.elements = funNodes.get.Select(funNode => funNode.element).ToArray();
			selectedNodeIndex = 0;
			refreshTimeline();
			EditorUtility.SetDirty(tweener);
		}

		void addTweenerComponent(GameObject gameObject) {
			gameObject.GetComponent<Tweener>().opt().voidFold(
				() => {
					tweener = gameObject.AddComponent<Tweener>();
					tweener.sequences = new List<Sequence>();
					//tweener.serializedTweenTimeline = new SerializedTweenTimeline();
					EditorUtility.SetDirty(gameObject);
				},
				_ => { }
			);
		}

	  void addFunTweenManagerComponent(GameObject gameObject) {
		  gameObject.GetComponent<FunTweenManager>().opt().voidFold(
			  () => {
				  funTweenManager = gameObject.AddComponent<FunTweenManager>().some();
				  refreshTimeline();
				  EditorUtility.SetDirty(gameObject);
			  },
			  fun => { funTweenManager = fun.some();} 
			);
	  }

		void addTimeline(Option<Tweener> tweenerTempOpt) {
			foreach (var tweenerTemp in tweenerTempOpt) {
				if (tweenerTemp.serializedTweenTimeline == null) {
					//tweenerTemp.serializedTweenTimeline = new SerializedTweenTimeline();
				}
				//tweenerTemp.serializedTweenTimeline = new SerializedTweenTimeline();
				Log.d.warn("timelineVisuals added");
			}
		}
	

	  void AddSequence(Tweener tweener) {
		  if (tweener != null) {
			  if (tweener.sequences == null) {
				  tweener.sequences = new List<Sequence>();
			  }

			  Sequence sequence = new Sequence();
			  sequence.nodes = new List<SequenceNode>();
			  sequence.events = new List<EventNode>();
			  int cnt = 0;
			  while (tweener.sequences.Find(x => x.name == "New SequenceTemp " + cnt.ToString()) != null) {
				  cnt++;
			  }

			  sequence.name = "New SequenceTemp " + cnt.ToString();
			  tweener.sequences.Add(sequence);
			  selectedSequenceIndex = tweener.sequences.Count - 1;
		  }
		  Log.d.warn("sequence added");
		  
	  }
  }
}
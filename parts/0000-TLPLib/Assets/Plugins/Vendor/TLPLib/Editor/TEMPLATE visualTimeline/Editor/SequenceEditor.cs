using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	public class SequenceEditor : EditorWindow {
		private Timeline timeline;
		private GameObject selectedGameObject;
		private TweenerTemp tweenerTemp;

		private SequenceTemp SequenceTemp {
			get {
				if (tweenerTemp != null && selectedSequenceIndex < tweenerTemp.sequences.Count) {
					return tweenerTemp.sequences[selectedSequenceIndex];
				}

				return null;
			}
		}

		private int selectedNodeIndex;

		private SequenceNodeTemp SelectedNodeTemp {
			get {
				if (SequenceTemp != null && selectedNodeIndex < SequenceTemp.nodes.Count) {
					return SequenceTemp.nodes[selectedNodeIndex];
				}

				return null;
			}
		}


		private bool isPlaying;
		private bool isRecording;
		private float playStartTime;
		private bool resizeNodeStart;
		private bool resizeNodeEnd;
		private bool dragNode;
		private float timeClickOffset;
		private Vector2 settingsScroll;
		private GameObject backupGameObject;
		private int selectedEventIndex;

		private EventNodeTemp selectedEvent {
			get {
				if (SequenceTemp != null && selectedEventIndex < SequenceTemp.events.Count) {
					return SequenceTemp.events[selectedEventIndex];
				}

				return null;
			}
		}

		[MenuItem("Window/Zerano Assets/Visual Tween", false)]
		public static void ShowWindow() {
			SequenceEditor window = EditorWindow.GetWindow<SequenceEditor>(false, "Tweener");
			window.wantsMouseMove = true;
			UnityEngine.Object.DontDestroyOnLoad(window);
		}

		private void OnEnable() {
			if (timeline == null) {
				timeline = new Timeline();
			}

			timeline.onRecord = OnRecord;
			timeline.onPlay = OnPlay;
			timeline.onSettingsGUI = OnSettings;
			timeline.onTimelineGUI = DrawNodes;
			timeline.onTimelineClick = OnTimelineClick;
			timeline.onAddEvent = OnAddEvent;
			timeline.onEventGUI = OnEventGUI;
			selectedEventIndex = int.MaxValue;
			selectedNodeIndex = int.MaxValue;
			if (selectedGameObject == null)
				OnSelectionChange();

			EditorApplication.playmodeStateChanged += OnPlayModeStateChange;
		}

		private void OnAddEvent() {
			AddTweener(Selection.activeGameObject);
			if (SequenceTemp == null) {
				AddSequence(tweenerTemp);
			}

			if (SequenceTemp.events == null) {
				SequenceTemp.events = new List<EventNodeTemp>();
			}

			GenericMenu menu = new GenericMenu();
			//Component[] components=selectedGameObject.GetComponents<Component>();
			List<Type> types = new List<Type>();
			//types.AddRange (components.Select (x => x.GetType ()));
			types.AddRange(GetSupportedTypes());
			foreach (Type type in types) {
				List<MethodInfo> functions = GetValidFunctions(type,
					!(type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(MonoBehaviour))) ||
					selectedGameObject.GetComponent(type) == null);
				foreach (MethodInfo mi in functions) {
					if (mi != null) {
						EventNodeTemp nodeTemp = new EventNodeTemp();
						nodeTemp.time = timeline.CurrentTime;
						nodeTemp.SerializedType = type;
						nodeTemp.method = mi.Name;
						nodeTemp.arguments = GetMethodArguments(mi);
						menu.AddItem(new GUIContent(type.Name + "/" + mi.Name), false, AddEvent, nodeTemp);
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

		private List<MethodArgumentTemp> GetMethodArguments(MethodInfo mi) {
			ParameterInfo[] pi = mi.GetParameters();
			List<MethodArgumentTemp> args = new List<MethodArgumentTemp>();
			foreach (ParameterInfo info in pi) {
				MethodArgumentTemp arg = new MethodArgumentTemp(info.Name, info.ParameterType.ToString());
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
			EventNodeTemp nodeTemp = data as EventNodeTemp;
			SequenceTemp.events.Add(nodeTemp);
			EditorUtility.SetDirty(tweenerTemp);
		}

		private void OnEventGUI(Rect rect) {
			if (SequenceTemp != null) {
				for (int i = 0; i < SequenceTemp.events.Count; i++) {
					Rect rect1 = new Rect(timeline.SecondsToGUI(SequenceTemp.events[i].time) - timeline.scroll.x + rect.x - 5f, rect.y, 17,
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

		private void OnPlayModeStateChange() { timeline.Stop(); }

		private void OnDestroy() { UndoObject(); }

		private void OnSelectionChange() {
			timeline.Stop();
			selectedGameObject = Selection.activeGameObject;
			if (selectedGameObject != null) {
				tweenerTemp = selectedGameObject.GetComponent<TweenerTemp>();
			}
			else {
				tweenerTemp = null;
			}

			selectedNodeIndex = 0;
			Repaint();
		}

		private bool playForward;
		private float time;
		private bool stop;

		private void Update() {
			if (!Application.isPlaying) {
				if (isPlaying && !stop) {
					if ((float) EditorApplication.timeSinceStartup > time) {
						switch (wrap) {
							case SequenceWrap.PingPong:
								playForward = !playForward;
								time = (float) EditorApplication.timeSinceStartup + GetSequenceEnd();
								if (playForward) {
									timeline.CurrentTime = 0;
									playStartTime = (float) EditorApplication.timeSinceStartup;
								}

								break;
							case SequenceWrap.Once:
								SequenceTemp.Stop(false);
								playStartTime = (float) EditorApplication.timeSinceStartup;
								timeline.CurrentTime = 0;
								stop = true;
								break;
							case SequenceWrap.ClampForever:
								SequenceTemp.Stop(true);
								stop = true;
								break;
							case SequenceWrap.Loop:
								SequenceTemp.Stop(false);
								playStartTime = (float) EditorApplication.timeSinceStartup;
								timeline.CurrentTime = 0;
								stop = false;
								time = (float) EditorApplication.timeSinceStartup + GetSequenceEnd();
								break;
						}
					}

					timeline.CurrentTime = (playForward
						? ((float) EditorApplication.timeSinceStartup - playStartTime)
						: time - (float) EditorApplication.timeSinceStartup);
					//->
					//timeline.currentTime = (float)EditorApplication.timeSinceStartup - playStartTime;
					EditorUpdate(timeline.CurrentTime);
					Repaint();
				}

				if (isRecording) {
					EditorUpdate(timeline.CurrentTime);
				}
			}
			else {
				if (tweenerTemp != null && SequenceTemp != null && tweenerTemp.IsPlaying(SequenceTemp.name)) {
					timeline.CurrentTime = SequenceTemp.passedTime;
					Repaint();
				}
			}
		}

		public float GetSequenceEnd() {
			if (SequenceTemp == null) {
				return Mathf.Infinity;
			}

			float sequenceEnd = 0;
			foreach (SequenceNodeTemp node in SequenceTemp.nodes) {
				if (sequenceEnd < (node.startTime + node.duration)) {
					sequenceEnd = node.startTime + node.duration;
				}
			}

			return sequenceEnd;
		}

		private void OnGUI() {
			bool enabled = GUI.enabled;
			GUI.enabled = selectedGameObject != null && !Application.isPlaying;
			timeline.DoTimeline(new Rect(0, 0, this.position.width, this.position.height));
			GUI.enabled = enabled;
		}

		private void EditorUpdate(float time) {
			if (SequenceTemp != null) {
				SequenceTemp.nodes = SequenceTemp.nodes.OrderBy(x => x.startTime).ToList();
				foreach (SequenceNodeTemp node in SequenceTemp.nodes) {
					node.UpdateTween(time);
				}

				//Canvas.ForceUpdateCanvases();
				EditorUtility.SetDirty(tweenerTemp);
			}
		}

		private void OnTimelineClick(float time) {
			if (SequenceTemp == null) {
				return;
			}

			foreach (SequenceNodeTemp node in SequenceTemp.nodes) {
				if (time < node.startTime) {
					node.UpdateValue(0.0f);
				}

				if (time > node.startTime + node.duration) {
					node.UpdateValue(1.0f);
				}
			}
		}

		private void DrawNodes(Rect position) {
			if (SequenceTemp == null) {
				return;
			}

			foreach (SequenceNodeTemp node in SequenceTemp.nodes) {
				EditorGUIUtility.AddCursorRect(new Rect(timeline.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20),
					MouseCursor.ResizeHorizontal);
				EditorGUIUtility.AddCursorRect(
					new Rect(timeline.SecondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20),
					MouseCursor.ResizeHorizontal);
				EditorGUIUtility.AddCursorRect(
					new Rect(timeline.SecondsToGUI(node.startTime), node.channel * 20, timeline.SecondsToGUI(node.duration), 20),
					MouseCursor.Pan);
			}

			foreach (SequenceNodeTemp node in SequenceTemp.nodes) {
				Rect boxRect = new Rect(timeline.SecondsToGUI(node.startTime), node.channel * 20,
					timeline.SecondsToGUI(node.duration), 20);
				GUI.Box(boxRect, "", "TL LogicBar 0");

				GUIStyle style = new GUIStyle("Label");
				style.fontSize = (SelectedNodeTemp == node ? 12 : style.fontSize);
				style.fontStyle = (SelectedNodeTemp == node ? FontStyle.Bold : FontStyle.Normal);
				Color color = style.normal.textColor;
				color.a = (SelectedNodeTemp == node ? 1.0f : 0.7f);
				style.normal.textColor = color;
				Vector3 size = style.CalcSize(new GUIContent(node.target.GetType().Name + "." + node.property));
				Rect rect1 = new Rect(boxRect.x + boxRect.width * 0.5f - size.x * 0.5f,
					boxRect.y + boxRect.height * 0.5f - size.y * 0.5f, size.x, size.y);
				GUI.Label(rect1, node.target.GetType().Name + "." + node.property, style);
			}

			DoEvents();
		}


		private bool dragEvent;

		private void DoEvents() {
			Event ev = Event.current;
			switch (ev.rawType) {
				case EventType.MouseDown:
					for (int j = 0; j < SequenceTemp.events.Count; j++) {
						Rect rect1 = new Rect(timeline.SecondsToGUI(SequenceTemp.events[j].time) - 5f, -15, 17, 20);
						if (rect1.Contains(Event.current.mousePosition)) {
							selectedEventIndex = j;
							selectedNodeIndex = int.MaxValue;
							if (ev.button == 0) {
								dragEvent = true;
							}

							if (ev.button == 1) {
								GenericMenu genericMenu = new GenericMenu();
								genericMenu.AddItem(new GUIContent("Remove"), false,
									delegate() { SequenceTemp.events.RemoveAt(selectedEventIndex); });
								genericMenu.ShowAsContext();
							}

							ev.Use();
						}
					}

					for (int i = 0; i < SequenceTemp.nodes.Count; i++) {
						SequenceNodeTemp nodeTemp = SequenceTemp.nodes[i];

						if (new Rect(timeline.SecondsToGUI(nodeTemp.startTime) - 5, nodeTemp.channel * 20, 10, 20).Contains(Event.current
							.mousePosition)) {
							selectedNodeIndex = i;
							selectedEventIndex = int.MaxValue;
							resizeNodeStart = true;
							EditorGUI.FocusTextInControl("");
							ev.Use();
						}

						if (new Rect(timeline.SecondsToGUI(nodeTemp.startTime + nodeTemp.duration) - 5, nodeTemp.channel * 20, 10, 20).Contains(
							Event.current.mousePosition)) {
							selectedNodeIndex = i;
							selectedEventIndex = int.MaxValue;
							resizeNodeEnd = true;
							EditorGUI.FocusTextInControl("");
							ev.Use();
						}

						if (new Rect(timeline.SecondsToGUI(nodeTemp.startTime), nodeTemp.channel * 20, timeline.SecondsToGUI(nodeTemp.duration), 20)
							.Contains(Event.current.mousePosition)) {
							if (ev.button == 0) {
								timeClickOffset = nodeTemp.startTime - timeline.GUIToSeconds(Event.current.mousePosition.x);
								dragNode = true;
								selectedNodeIndex = i;
								selectedEventIndex = int.MaxValue;
								EditorGUI.FocusTextInControl("");
							}

							if (ev.button == 1) {
								GenericMenu genericMenu = new GenericMenu();
								genericMenu.AddItem(new GUIContent("Remove"), false, this.RemoveTween, nodeTemp);
								genericMenu.ShowAsContext();
							}

							ev.Use();
						}
					}

					break;
				case EventType.MouseDrag:
					if (dragEvent) {
						selectedEvent.time = timeline.GUIToSeconds(Event.current.mousePosition.x);
						selectedEvent.time = Mathf.Clamp(selectedEvent.time, 0, float.MaxValue);
						ev.Use();
					}

					if (resizeNodeStart) {
						SelectedNodeTemp.startTime = timeline.GUIToSeconds(Event.current.mousePosition.x);
						SelectedNodeTemp.startTime = Mathf.Clamp(SelectedNodeTemp.startTime, 0, float.MaxValue);
						if (SelectedNodeTemp.startTime > 0) {
							SelectedNodeTemp.duration -= timeline.GUIToSeconds(ev.delta.x);
							SelectedNodeTemp.duration = Mathf.Clamp(SelectedNodeTemp.duration, 0.01f, float.MaxValue);
						}

						ev.Use();
					}

					if (resizeNodeEnd) {
						SelectedNodeTemp.duration = (timeline.GUIToSeconds(Event.current.mousePosition.x) - SelectedNodeTemp.startTime);
						SelectedNodeTemp.duration = Mathf.Clamp(SelectedNodeTemp.duration, 0.01f, float.MaxValue);
						ev.Use();
					}

					if (dragNode && !resizeNodeStart && !resizeNodeEnd) {
						SelectedNodeTemp.startTime = timeline.GUIToSeconds(Event.current.mousePosition.x) + timeClickOffset;
						SelectedNodeTemp.startTime = Mathf.Clamp(SelectedNodeTemp.startTime, 0, float.MaxValue);
						if (Event.current.mousePosition.y > SelectedNodeTemp.channel * 20 + 25) {
							SelectedNodeTemp.channel += 1;
						}

						if (Event.current.mousePosition.y < SelectedNodeTemp.channel * 20 - 5) {
							SelectedNodeTemp.channel -= 1;
						}

						SelectedNodeTemp.channel = Mathf.Clamp(SelectedNodeTemp.channel, 0, int.MaxValue);
						ev.Use();
					}

					break;
				case EventType.MouseUp:
					dragNode = false;
					resizeNodeStart = false;
					resizeNodeEnd = false;
					dragEvent = false;
					break;
			}
		}

		private bool backup;

		private void OnRecord(bool isRecording) {
			if (tweenerTemp == null && selectedGameObject != null && isRecording) {
				tweenerTemp = selectedGameObject.AddComponent<TweenerTemp>();
				AddSequence(tweenerTemp);
				//sequenceTemp.nodes = new List<SequenceNodeTemp>();
				EditorUtility.SetDirty(tweenerTemp);
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
			timeline.CurrentTime = 0;

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

			backupGameObject = (GameObject) Instantiate(selectedGameObject);
			backupGameObject.transform.SetParent(selectedGameObject.transform.parent, false);
			backupGameObject.name = selectedGameObject.name;
			backupGameObject.SetActive(false);
			backupGameObject.hideFlags = HideFlags.HideInHierarchy;
			backup = true;
		}

		private void UndoObject() {
			if (!backup) {
				return;
			}

			TweenerTemp mTweenerTemp = backupGameObject.GetComponent<TweenerTemp>();
			mTweenerTemp.sequences = tweenerTemp.sequences;
			for (int i = 0; i < tweenerTemp.sequences.Count; i++) {
				for (int j = 0; j < tweenerTemp.sequences[i].nodes.Count; j++) {
					SequenceNodeTemp nodeTemp = tweenerTemp.sequences[i].nodes[j];
					mTweenerTemp.sequences[i].nodes[j].target = mTweenerTemp.GetComponent(nodeTemp.target.GetType());
				}
			}

			DestroyImmediate(selectedGameObject);
			selectedGameObject = backupGameObject;
			selectedGameObject.hideFlags = HideFlags.None;
			selectedGameObject.SetActive(true);
			Selection.activeGameObject = selectedGameObject;
			backup = false;
		}

		private int selectedSequenceIndex;
		private SequenceWrap wrap;

		private void OnSettings(float width) {
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(SequenceTemp != null ? SequenceTemp.name : "[None Selected]", EditorStyles.toolbarDropDown,
				GUILayout.Width(width * 0.5f))) {
				GenericMenu toolsMenu = new GenericMenu();
				if (tweenerTemp != null) {
					for (int i = 0; i < tweenerTemp.sequences.Count; i++) {
						int mIndex = i;
						toolsMenu.AddItem(new GUIContent(tweenerTemp.sequences[mIndex].name), false,
							delegate() { selectedSequenceIndex = mIndex; });
					}
				}

				toolsMenu.AddItem(new GUIContent("[New SequenceTemp]"), false, delegate() {
					AddTweener(Selection.activeGameObject);
					AddSequence(tweenerTemp);
				});
				GUIUtility.keyboardControl = 0;
				toolsMenu.DropDown(new Rect(3, 37, 0, 0));
				EditorGUIUtility.ExitGUI();
			}

			if (SequenceTemp != null) {
				wrap = SequenceTemp.wrap;
			}

			wrap = (SequenceWrap) EditorGUILayout.EnumPopup(wrap, EditorStyles.toolbarDropDown, GUILayout.Width(width * 0.5f));
			if (SequenceTemp != null) {
				SequenceTemp.wrap = wrap;
			}

			GUILayout.EndHorizontal();

			//-->
			GUILayout.BeginVertical();
			settingsScroll = GUILayout.BeginScrollView(settingsScroll);
			if (tweenerTemp != null) {
				DoSequence();
			}

			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();
			bool enabled = GUI.enabled;
			GUI.enabled = SequenceTemp != null;
			if (GUILayout.Button("Add Tween")) {
				GenericMenu genericMenu = new GenericMenu();
				Component[] components = selectedGameObject.GetComponents<Component>();
				for (int i = 0; i < components.Length; i++) {
					PropertyInfo[] properties = components[i].GetType().GetProperties();
					for (int j = 0; j < properties.Length; j++) {
						if (properties[j].CanWrite) {
							if (IsSupportedType(properties[j].PropertyType)) {
								genericMenu.AddItem(new GUIContent(components[i].GetType().Name + "/" + properties[j].Name), false, AddTween,
									new SequenceNodeTemp(components[i], properties[j].Name));
							}
						}
					}

					FieldInfo[] fields = components[i].GetType().GetFields();
					for (int j = 0; j < fields.Length; j++) {
						if (IsSupportedType(fields[j].FieldType)) {
							genericMenu.AddItem(new GUIContent(components[i].GetType().Name + "/" + fields[j].Name), false, AddTween,
								new SequenceNodeTemp(components[i], fields[j].Name));
						}
					}
				}

				genericMenu.ShowAsContext();
			}

			GUI.enabled = enabled;
			GUILayout.EndVertical();
		}

		private void DoSequence() {
			EditorGUIUtility.labelWidth = 63;
			SerializedObject serializedObject = new SerializedObject(tweenerTemp);
			serializedObject.Update();
			SerializedProperty sequenceArray = serializedObject.FindProperty("sequences");

			if (selectedSequenceIndex < sequenceArray.arraySize) {
				SerializedProperty sequenceProperty = sequenceArray.GetArrayElementAtIndex(selectedSequenceIndex);
				SerializedProperty sequenceName = sequenceProperty.FindPropertyRelative("name");
				SerializedProperty playAutomatically = sequenceProperty.FindPropertyRelative("playAutomatically");
				EditorGUILayout.PropertyField(sequenceName);
				EditorGUILayout.PropertyField(playAutomatically);
				SerializedProperty nodesArray = sequenceProperty.FindPropertyRelative("nodes");
				if (selectedNodeIndex < nodesArray.arraySize) {
					SerializedProperty nodeProperty = nodesArray.GetArrayElementAtIndex(selectedNodeIndex);
					EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("startTime"));
					EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("duration"));
					SerializedProperty fromProperty = null;
					SerializedProperty toProperty = null;
					if (SelectedNodeTemp.PropertyType == typeof(float)) {
						fromProperty = nodeProperty.FindPropertyRelative("fromFloat");
						toProperty = nodeProperty.FindPropertyRelative("toFloat");
					}
					else if (SelectedNodeTemp.PropertyType == typeof(Vector2)) {
						fromProperty = nodeProperty.FindPropertyRelative("fromVector2");
						toProperty = nodeProperty.FindPropertyRelative("toVector2");
					}
					else if (SelectedNodeTemp.PropertyType == typeof(Vector3)) {
						fromProperty = nodeProperty.FindPropertyRelative("fromVector3");
						toProperty = nodeProperty.FindPropertyRelative("toVector3");
					}
					else if (SelectedNodeTemp.PropertyType == typeof(Color)) {
						fromProperty = nodeProperty.FindPropertyRelative("fromColor");
						toProperty = nodeProperty.FindPropertyRelative("toColor");
					}

					if (fromProperty != null && toProperty != null) {
						EditorGUILayout.PropertyField(fromProperty, new GUIContent("From"));
						EditorGUILayout.PropertyField(toProperty, new GUIContent("To"));
					}

					EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("ease"));
				}

				SerializedProperty eventsArray = sequenceProperty.FindPropertyRelative("events");
				if (selectedEventIndex < eventsArray.arraySize) {
					SerializedProperty eventProperty = eventsArray.GetArrayElementAtIndex(selectedEventIndex);
					SerializedProperty methodProperty = eventProperty.FindPropertyRelative("method");
					//SerializedProperty typeProperty=eventProperty.FindPropertyRelative("type");
					SerializedProperty argumentsArray = eventProperty.FindPropertyRelative("arguments");

					EventNodeTemp eventNodeTemp = SequenceTemp.events[selectedEventIndex];

					if (GUILayout.Button(eventNodeTemp.SerializedType.Name + "." + methodProperty.stringValue, "DropDown")) {
						GenericMenu menu = new GenericMenu();
						Component[] components = selectedGameObject.GetComponents<Component>();
						List<Type> types = new List<Type>();
						types.AddRange(components.Select(x => x.GetType()));
						types.AddRange(GetSupportedTypes());
						foreach (Type type in types) {
							List<MethodInfo> functions = GetValidFunctions(type,
								!(type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(MonoBehaviour))) ||
								selectedGameObject.GetComponent(type) == null);
							foreach (MethodInfo mi in functions) {
								if (mi != null) {
									EventNodeTemp nodeTemp = new EventNodeTemp();
									nodeTemp.time = timeline.CurrentTime;
									nodeTemp.SerializedType = type;
									nodeTemp.method = mi.Name;
									nodeTemp.arguments = GetMethodArguments(mi);
									nodeTemp.time = eventNodeTemp.time;
									menu.AddItem(new GUIContent(type.Name + "/" + mi.Name), false, delegate() {
										SequenceTemp.events[selectedEventIndex] = nodeTemp;
										EditorUtility.SetDirty(tweenerTemp);
									});
								}
							}
						}

						menu.ShowAsContext();
					}

					for (int i = 0; i < argumentsArray.arraySize; i++) {
						SerializedProperty argumentProperty = argumentsArray.GetArrayElementAtIndex(i);
						EditorGUILayout.PropertyField(argumentProperty.FindPropertyRelative(eventNodeTemp.arguments[i].GetValueName()),
							new GUIContent("Parameter"));
					}

				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void AddTween(object node) {
			SequenceNodeTemp mNodeTemp = node as SequenceNodeTemp;
			SequenceTemp.nodes.Add(mNodeTemp);
			mNodeTemp.SetDefaultValue();
			EditorUtility.SetDirty(tweenerTemp);
		}

		private void RemoveTween(object node) {
			SequenceNodeTemp mNodeTemp = node as SequenceNodeTemp;
			SequenceTemp.nodes.Remove(mNodeTemp);
			EditorUtility.SetDirty(tweenerTemp);
		}

		private void AddTweener(GameObject gameObject) {
			if (gameObject.GetComponent<TweenerTemp>() == null) {
				tweenerTemp = gameObject.AddComponent<TweenerTemp>();
				tweenerTemp.sequences = new List<SequenceTemp>();
				EditorUtility.SetDirty(gameObject);
			}
		}

		private void AddSequence(TweenerTemp tweenerTemp) {
			if (tweenerTemp != null) {
				if (tweenerTemp.sequences == null) {
					tweenerTemp.sequences = new List<SequenceTemp>();
				}

				SequenceTemp sequenceTemp = new SequenceTemp();
				sequenceTemp.nodes = new List<SequenceNodeTemp>();
				sequenceTemp.events = new List<EventNodeTemp>();
				int cnt = 0;
				while (tweenerTemp.sequences.Find(x => x.name == "New SequenceTemp " + cnt.ToString()) != null) {
					cnt++;
				}

				sequenceTemp.name = "New SequenceTemp " + cnt.ToString();
				tweenerTemp.sequences.Add(sequenceTemp);
				selectedSequenceIndex = tweenerTemp.sequences.Count - 1;
			}
		}

		private bool IsSupportedType(Type type) {
			if (type == typeof(float) || type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(Color)) {
				return true;
			}

			return false;
		}
	}
}
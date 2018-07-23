using UnityEngine;
using UnityEditor;
using System.Collections;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	[System.Serializable]
	public class Timeline {
		public delegate void PlayCallback(bool isPlaying);

		public Timeline.PlayCallback onPlay;

		public delegate void RecordCallback(bool isRecording);

		public Timeline.RecordCallback onRecord;

		public delegate void TimelineGUICallback(Rect position);

		public Timeline.TimelineGUICallback onTimelineGUI;

		public delegate void SettingsGUICallback(float width);

		public Timeline.SettingsGUICallback onSettingsGUI;

		public delegate void OnTimelineClick(float time);

		public Timeline.OnTimelineClick onTimelineClick;

		public delegate void AddEventCallback();

		public Timeline.AddEventCallback onAddEvent;

		public delegate void EventGUICallback(Rect position);

		public Timeline.EventGUICallback onEventGUI;

		private Rect timeRect;
		private Rect drawRect;
		public Rect eventRect;
		private float timelineOffset = 170;
		private int[] timeFactor = new int[] {1, 5, 10, 30, 60, 300, 600, 1800, 3600, 7200};
		public int TimeFactor { get { return timeFactor[timeIndexFactor]; } }

		public float CurrentTime {
			get { return GUIToSeconds(timePosition - timelineOffset); }
			set { timePosition = SecondsToGUI(value) + timelineOffset; }
		}

		private int timeIndexFactor = 1;
		private float timeZoomFactor = 1;
		public Vector2 scroll;
		private float zoom = 20;
		private Vector2 expandView;
		private bool isRecording;

		public bool isPlaying;
		private bool changeOffset;
		private float timePosition;
		private bool changeTime;
		private float clickOffset;


		public void DoTimeline(Rect position) {
			drawRect = position;
			timeRect = new Rect(position.x + timelineOffset, position.y, position.width - 15, 20);
			eventRect = new Rect(position.x + timelineOffset - 1, position.y + 19, position.width, 16);

			DoCursor();
			DoToolbarGUI(position);
			DoTicksGUI();

			DoEvents();


			scroll = GUI.BeginScrollView(
				new Rect(position.x + timelineOffset, timeRect.height + eventRect.height, position.width - timelineOffset,
					position.height - timeRect.height - eventRect.height), scroll,
				new Rect(0, 0, position.width + expandView.x - timelineOffset, position.height - 40 + expandView.y), true, true);
			DoLines();
			if (onTimelineGUI != null) {
				onTimelineGUI(new Rect(scroll.x, scroll.y, position.width - timelineOffset - 15 + scroll.x,
					drawRect.height + scroll.y));
			}

			GUI.EndScrollView();
			DoEventsGUI();
			DoTimelineGUI();

		}

		private void DoLines() {
			Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			for (int y = 0; y < (int) drawRect.height + scroll.y; y += 20) {
				Handles.DrawLine(new Vector3(0, y, 0), new Vector3(drawRect.width + scroll.x, y, 0));
			}

			Handles.color = Color.white;
		}

		private void DoTimelineGUI() {
			if ((isRecording || isPlaying || Application.isPlaying) && timePosition - scroll.x >= timelineOffset &&
				timePosition - scroll.x < drawRect.width - 15) {
				Color color = Color.red;
				color.a = Application.isPlaying ? 0.6f : 1.0f;
				Handles.color = color;
				Handles.DrawLine(new Vector3(timePosition - scroll.x, 0, 0),
					new Vector3(timePosition - scroll.x, drawRect.height - 15, 0));
				Handles.color = Color.white;

			}
		}

		private void DoCursor() {
			EditorGUIUtility.AddCursorRect(new Rect(timelineOffset - 5, 37, 10, drawRect.height), MouseCursor.ResizeHorizontal);
		}

		private void DoToolbarGUI(Rect position) {
			GUIStyle style = new GUIStyle("ProgressBarBack");
			style.padding = new RectOffset(0, 0, 0, 0);
			GUILayout.BeginArea(new Rect(position.x, position.y, timelineOffset, position.height), GUIContent.none, style);

			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			//bool enabled = GUI.enabled;
			//GUI.enabled = !isPlaying && !Application.isPlaying;
			GUI.backgroundColor = isRecording ? Color.red : Color.white;
			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Animation.Record"), EditorStyles.toolbarButton) && !isPlaying) {
				isRecording = !isRecording;
				if (onRecord != null) {
					onRecord(isRecording);
				}

				CurrentTime = 0.0f;
			}

			GUI.backgroundColor = Color.white;
			//GUI.enabled = !isRecording && !Application.isPlaying;

			if (GUILayout.Button(
				isPlaying ? EditorGUIUtility.FindTexture("d_PlayButton On") : EditorGUIUtility.FindTexture("d_PlayButton"),
				EditorStyles.toolbarButton) && !isRecording) {
				isPlaying = !isPlaying;
				if (onPlay != null) {
					onPlay(isPlaying);
				}
			}
			//	GUI.enabled = enabled;

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Animation.AddEvent"), EditorStyles.toolbarButton)) {
				if (onAddEvent != null) {
					onAddEvent();
				}
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			if (onSettingsGUI != null) {
				onSettingsGUI(timelineOffset - 1.5f);
			}

			GUILayout.EndVertical();
			GUILayout.Space(1.5f);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

		}

		public void Stop() {
			StopRecord();
			StopPlay();
		}

		public void StopRecord() {
			if (isRecording) {
				isRecording = false;
				if (onRecord != null) {
					onRecord(isRecording);
				}
			}
		}

		public void StopPlay() {
			if (isPlaying) {
				isPlaying = false;
				if (onPlay != null) {
					onPlay(isPlaying);
				}

				CurrentTime = 0.0f;
			}
		}

		private void DoEventsGUI() {
			if (Event.current.type == EventType.Repaint) {
				((GUIStyle) "AnimationEventBackground").Draw(eventRect, GUIContent.none, 0);
			}

			if (onEventGUI != null) {
				onEventGUI(eventRect);
			}

		}

		private void DoTicksGUI() {
			if (Event.current.type == EventType.Repaint) {
				EditorStyles.toolbar.Draw(timeRect, GUIContent.none, 0);
			}

			Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
			int count = 0;
			for (float x = timeRect.x - scroll.x; x < timeRect.width; x += zoom * timeZoomFactor) {
				Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
				if (x >= timelineOffset) {
					if (count % 5 == 0) {
						Handles.DrawLine(new Vector3(x, 7, 0), new Vector3(x, 17, 0));
						int displayMinutes = Mathf.FloorToInt((count / 5.0f) * timeFactor[timeIndexFactor] / 60.0f);
						int displaySeconds = Mathf.FloorToInt((count / 5.0f) * timeFactor[timeIndexFactor] % 60.0f);
						GUIContent content = new GUIContent(string.Format("{0:0}:{1:00}", displayMinutes, displaySeconds));
						Vector2 size = ((GUIStyle) "Label").CalcSize(content);
						size.x = Mathf.Clamp(size.x, 0.0f, timeRect.width - x);
						GUI.Label(new Rect(x, -2, size.x, size.y), content);
					}
					else {
						Handles.DrawLine(new Vector3(x, 13, 0), new Vector3(x, 17, 0));
					}
				}

				count++;
			}

			Handles.color = Color.white;
		}

		private void DoEvents() {
			if (!GUI.enabled) {
				return;
			}

			Event ev = Event.current;
			switch (ev.rawType) {
				case EventType.MouseDown:
					if (new Rect(timelineOffset - 5, 37, 10, drawRect.height).Contains(ev.mousePosition)) {
						changeOffset = true;
						clickOffset = timePosition - timelineOffset;
					}

					if (new Rect(timelineOffset, 0, drawRect.width, 17).Contains(Event.current.mousePosition) &&
						Event.current.button == 0 && !isPlaying) {
						timePosition = Event.current.mousePosition.x + scroll.x;
						changeTime = true;

						if (!isRecording && onRecord != null) {
							onRecord(true);
						}

						isRecording = true;

						if (onTimelineClick != null) {
							onTimelineClick(CurrentTime);
						}

						ev.Use();
					}

					break;
				case EventType.MouseUp:
					changeOffset = false;
					changeTime = false;
					break;
				case EventType.MouseDrag:
					switch (ev.button) {
						case 0:
							if (changeOffset) {
								timelineOffset = ev.mousePosition.x;
								timelineOffset = Mathf.Clamp(timelineOffset, 170, drawRect.width - 170);
								timePosition = timelineOffset + clickOffset;
								timePosition = Mathf.Clamp(timePosition, timelineOffset, Mathf.Infinity);
								ev.Use();
							}

							if (changeTime) {
								timePosition = Event.current.mousePosition.x + scroll.x;
								timePosition = Mathf.Clamp(timePosition, timelineOffset, Mathf.Infinity);
								ev.Use();
							}

							break;
						case 1:
							break;
						case 2:
							scroll -= ev.delta;
							scroll.x = Mathf.Clamp(scroll.x, 0f, Mathf.Infinity);
							scroll.y = Mathf.Clamp(scroll.y, 0f, Mathf.Infinity);

							expandView -= ev.delta;
							expandView.x = Mathf.Clamp(expandView.x, 20f, Mathf.Infinity);
							expandView.y = Mathf.Clamp(expandView.y, 20f, Mathf.Infinity);
							ev.Use();
							break;
					}

					break;
				case EventType.ScrollWheel:
					float f = timeZoomFactor;
					if (timeIndexFactor == timeFactor.Length - 1 && f < 0.5f) {
						f = 0.5f;
					}
					else {
						f += ev.delta.y / 100;
					}

					if (f < 0.5f && timeIndexFactor < timeFactor.Length - 1) {
						timeIndexFactor++;
						f = 1;
					}

					if (f > 1.5f && timeIndexFactor > 0) {
						timeIndexFactor--;
						f = 1;
					}

					timeZoomFactor = f;

					ev.Use();
					break;
			}
		}

		public float SecondsToGUI(float seconds) {
			float guiSecond = (seconds / timeFactor[timeIndexFactor]) * zoom * timeZoomFactor * 5.0f;
			return guiSecond;
		}

		public float GUIToSeconds(float x) {
			float guiSecond = zoom * timeZoomFactor * 5.0f / timeFactor[timeIndexFactor];
			float res = (x) / guiSecond;
			return res;
		}
	}
}

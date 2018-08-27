#if ADV_INS_CHANGES
using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using GenerationAttributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
	public partial class TimelineVisuals {
		
		public TimelineVisuals(TimelineEditor timelineEditor) {
			this.timelineEditor = timelineEditor;
			timeFactor = new []{1,5,10,30,60,300,600,1800,3600,7200};
			onSettingsGUI = timelineEditor.doSettings;
		}
		
		public delegate void SettingsGUICallback(float width, bool isVisualiisation);

		readonly SettingsGUICallback onSettingsGUI;
		
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
		
		readonly List<callbackVisuals> callbackVisualsList = new List<callbackVisuals>();
	
		Rect blackBarRect;
		const float ZOOM = 20;
		const int OUTLINE_WIDTH = 3;
		readonly int[] timeFactor;
		int timeIndexFactor = 1;

		Rect timeRect;
		[PublicAccessor] Rect _drawRect;
		
		float currentTime {
			get => GUIToSeconds(timePosition - timelineOffset);
			set => timePosition = secondsToGUI (value) + timelineOffset;
		}
	
		Vector2 scroll, expandView;
		bool isAnimationPlaying, changeTime, changeOffset, applicationPlaying, playingBackwards, visualizationMode;
		float timePosition, clickOffset, timeZoomFactor = 1, lastNodeTime, timelineOffset = 450;
		double lastSeconds;
	
		readonly TimelineEditor timelineEditor;
	
		Option<EditorApplication.CallbackFunction> updateDelegateOpt;
	
		public void doTimeline(Rect position, Option<FunTweenManager> funTweenManager) {
			applicationPlaying = Application.isPlaying;
			_drawRect = position;
			timeRect = new Rect (position.x + timelineOffset, position.y, position.width - 15, 20);
			blackBarRect = new Rect (position.x + timelineOffset - 1, position.y + 19, position.width, 16);
	
			if (funTweenManager.valueOut(out var ftm) && (visualizationMode || applicationPlaying)) {
				currentTime = ftm.timeline.timeline.timePassed;
			}
			
			if (Event.current.isKey && !applicationPlaying) {
				stopVisualization();
				isAnimationPlaying = false;
				return;
			}
	
			doCursor ();
			doToolbarGUI (position, funTweenManager);
			drawTicksGUI ();
			doTimelineEvents (funTweenManager);
		
			scroll = GUI.BeginScrollView(
				new Rect(position.x + timelineOffset,
					timeRect.height + blackBarRect.height,
					position.width - timelineOffset,
					position.height - timeRect.height - blackBarRect.height),
				scroll,
				new Rect(0, 0,
					position.width + lastNodeTime + expandView.x - timelineOffset,
					position.height + 400 + expandView.y),
				true, true
			);
			
			doLines ();
			drawNodes();
			if (!visualizationMode) {
				timelineEditor.doEvents();
			}
	
			GUI.EndScrollView();
			
			doBlackBar ();
			doTimelineGUI ();
		}
		
		void drawNodes() {
			GUI.enabled = !visualizationMode;
	
			Option<Rect> snapIndicatorOpt = F.none_;
			callbackVisualsList.Clear();
			var indicatorColor = Color.green;
	
			foreach (var node in timelineEditor.funNodes) {
				if (node.element.element != null) {
					if (!node.isCallback) {
						EditorGUIUtility.AddCursorRect(
							new Rect(secondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20),
							MouseCursor.ResizeHorizontal);
						EditorGUIUtility.AddCursorRect(
							new Rect(secondsToGUI(node.startTime + node.duration) - 5, node.channel * 20, 10, 20),
							MouseCursor.ResizeHorizontal);
					}
					EditorGUIUtility.AddCursorRect(
						new Rect(
							secondsToGUI(node.startTime - (node.isCallback ? 0.5f : 0)),
							node.channel * 20,
							secondsToGUI(node.isCallback ? 1 : node.duration), 20
						),
						MouseCursor.Pan);
	
					var boxRect = new Rect(secondsToGUI(node.startTime), node.channel * 20,
						secondsToGUI(node.duration), 20);
	
					var currentNodeIsSelected = !timelineEditor.selectedNodesList.isEmpty() &&
						timelineEditor.selectedNodesList.find(x => x == node).isSome;
	
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
	
					if (!timelineEditor.selectedNodesList.isEmpty() &&
						timelineEditor.selectedNodesList.find(x => x == node).valueOut(out var selectedNode)
					) {
						drawOutline(boxRect, Color.magenta);
						
						if ( (timelineEditor.isEndSnapped || timelineEditor.isStartSnapped)
							&& timelineEditor.rootSelectedNodeOpt.valueOut(out var rootNode)
							&& rootNode == node
							) {
							if (timelineEditor.nodeSnappedToOpt.valueOut(out var nodeSnappedTo)) {
								var selectedIsHigher = selectedNode.channel < nodeSnappedTo.node.channel;
								var distance = (Mathf.Abs(selectedNode.channel - nodeSnappedTo.node.channel) + 2) * 20;
								snapIndicatorOpt =
									selectedIsHigher
										? getIndicatorRect(selectedNode, timelineEditor.isStartSnapped, distance).some()
										: getIndicatorRect(nodeSnappedTo.node, nodeSnappedTo.snappedToStart, distance).some();
	
								Rect getIndicatorRect(TimelineNode nawd, bool isSnappedToStart, float dist) =>
									new Rect(secondsToGUI(
											isSnappedToStart
												? nawd.startTime
												: nawd.getEnd()),
										nawd.channel * 20 - 10, 2, dist
									);
							}
	
							indicatorColor = timelineEditor.isEndSnapped ? Color.yellow : Color.cyan;
						}
					}
					else if (!node.isCallback) {
						GUI.Box(boxRect, "", "TL LogicBar 0");
					}
					
					if (node.isCallback) {
						callbackVisualsList.Add(new callbackVisuals(boxRect, tooltip));
					}
	
					if (node.linkedNode.valueOut(out var linkedNode)) {
						drawOutline(boxRect, Color.green);
						EditorGUI.DrawRect(
							new Rect(secondsToGUI(linkedNode.getEnd()),
								node.channel * 20 + 10, secondsToGUI(node.startTime - linkedNode.getEnd()), 2),
							Color.green
						);
						EditorGUI.DrawRect(
							new Rect(secondsToGUI(linkedNode.getEnd()),
								node.channel * 20, 3, 20),
							Color.green
						);
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
			}
		}
	
		void doLines(){
			Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			for (var y = 0; y < (int)_drawRect.height + scroll.y; y += 20) {
				Handles.DrawLine(new Vector3(0, y, 0), new Vector3(_drawRect.width+scroll.x, y, 0));	
			}
			Handles.color = Color.white;
		}
		
		void doTimelineGUI(){
			if ((changeTime || isAnimationPlaying || Application.isPlaying || visualizationMode)
				&& timePosition - scroll.x >= timelineOffset && timePosition - scroll.x < _drawRect.width - 15) {
				var color = Color.red;
				color.a = Application.isPlaying ? 0.6f : 1.0f;
				Handles.color = color;
				
				var style = new GUIStyle("Label") {
					fontSize = 12,
					fontStyle = FontStyle.Bold,
					normal = {textColor = color}
				};
				Vector3 size = style.CalcSize(new GUIContent($"content: {currentTime:F2}s"));
				var rect1 = new Rect(timePosition - scroll.x, 19, size.x, size.y);
				GUI.Label(rect1, $"{currentTime:F2}s", style);
				Handles.DrawLine(new Vector3(timePosition - scroll.x, 0, 0),
					new Vector3(timePosition - scroll.x, _drawRect.height - 15, 0)
				);
				Handles.color = Color.white;
			}
		}
		
		public static double currentRealSeconds() {
			var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var currentSeconds = (DateTime.UtcNow - epochStart).TotalSeconds;
	 
			return currentSeconds;
		}
		
		void doCursor(){
			EditorGUIUtility.AddCursorRect(new Rect(timelineOffset - 5, 37, 10, _drawRect.height), MouseCursor.ResizeHorizontal);
		}
		
		void doBlackBar(){
			if (Event.current.type == EventType.Repaint) {
				((GUIStyle)"AnimationEventBackground").Draw(blackBarRect, GUIContent.none, 0);
			}
		}
	
		void updateAnimation(FunTweenManager ftm) {
			var currentSeconds = currentRealSeconds();
			if (lastSeconds < 1) lastSeconds = currentSeconds;
			var timeDiff = (float)(currentSeconds - lastSeconds);
			lastSeconds = currentSeconds;
			
			ftm.timeline.timeline.timePassed += timeDiff * (playingBackwards ? -1 : 1);
		}
	
		void startUpdatingTime(FunTweenManager ftm) {
			if (!EditorApplication.isCompiling) {
				startVisualization(ftm);
				lastSeconds = currentRealSeconds();
				updateDelegateOpt.voidFold(
					() => {
						EditorApplication.CallbackFunction updateDelegate = () => updateAnimation(ftm);
						EditorApplication.update += updateDelegate;
						updateDelegateOpt = updateDelegate.some();
					},
					updateDelegate => EditorApplication.update += updateDelegate
				);
			}
		}
	
		void stopTimeUpdate() {
			updateDelegateOpt.map(updateDelegate => EditorApplication.update -= updateDelegate);
		}
	
		void startVisualization(FunTweenManager ftm) {
			if (!visualizationMode && getTimelineTargets(ftm).valueOut(out var data)) {
				Undo.RegisterCompleteObjectUndo(data, "Animating targets");
				ftm.recreate();
				visualizationMode = true;
			}
		}
	
		public void stopVisualization() {
			if (visualizationMode) {
				stopTimeUpdate();
				Undo.PerformUndo();
				visualizationMode = false;
			}
		}
		
		void doToolbarGUI(Rect position, Option<FunTweenManager> funTweenManager){
			var style = new GUIStyle("ProgressBarBack") {padding = new RectOffset(0, 0, 0, 0)};
			GUILayout.BeginArea (new Rect (position.x, position.y, timelineOffset, position.height), GUIContent.none, style);
			
			GUILayout.BeginHorizontal (EditorStyles.toolbar);

			GUI.enabled = !EditorApplication.isCompiling;
			if (funTweenManager.valueOut(out var ftm)) {
				
				if (GUILayout.Button(EditorGUIUtility.FindTexture("d_beginButton"), EditorStyles.toolbarButton)) {
					ftm.timeline.timeline.timePassed = 0;
					if (!applicationPlaying) {
						stopTimeUpdate();
						startVisualization(ftm);
					}
					else {
						ftm.run(FunTweenManager.Action.Stop);
					}
					isAnimationPlaying = false;
				}
	
				GUI.backgroundColor = new Color(0, 0.8f, 1, 0.5f);
				if (GUILayout.Button(EditorGUIUtility.FindTexture("d_StepButton"), EditorStyles.toolbarButton)) {
					if (!applicationPlaying) {
						if (!isAnimationPlaying) {
							startUpdatingTime(ftm);
						}
						
						ftm.timeline.timeline.timePassed = 0;
	
						if (!visualizationMode) {
							startVisualization(ftm);
						}
						playingBackwards = false;
					}
					else {
						ftm.run(FunTweenManager.Action.PlayForwards);
					}
					isAnimationPlaying = true;
				}
				GUI.backgroundColor = Color.white;
	
				if (GUILayout.Button(
					!isAnimationPlaying
						? EditorGUIUtility.FindTexture("d_PlayButton")
						: EditorGUIUtility.FindTexture("d_PlayButton On"), EditorStyles.toolbarButton)
				) {
					if (isAnimationPlaying) {
						if (!applicationPlaying) {
							stopTimeUpdate();
							playingBackwards = false;
						}
						else {
							ftm.run(FunTweenManager.Action.Stop);
						}
						isAnimationPlaying = false;
					}
					else {
						if (!applicationPlaying) {
							
							if (!visualizationMode) {
								startVisualization(ftm);
							}
							startUpdatingTime(ftm);
							playingBackwards = false;
						}
						else {
							ftm.run(FunTweenManager.Action.Resume);
						}
						isAnimationPlaying = true;
					}
				}
	
				if (GUILayout.Button(isAnimationPlaying
					? EditorGUIUtility.FindTexture("d_PauseButton")
					: EditorGUIUtility.FindTexture("d_PauseButton On"), EditorStyles.toolbarButton)
				) {
					if (!applicationPlaying){
						stopTimeUpdate();
					}
					else {
						ftm.run(FunTweenManager.Action.Stop);
					}
					isAnimationPlaying = false;
	
				}
	
				GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
				if (GUILayout.Button(EditorGUIUtility.FindTexture("d_StepLeftButton"), EditorStyles.toolbarButton)) {
					if (!applicationPlaying) {
						if (!visualizationMode)  { startVisualization(ftm); }
						ftm.timeline.timeline.timePassed = ftm.timeline.timeline.duration;
						if (!isAnimationPlaying) { startUpdatingTime(ftm);  }
						playingBackwards = true;
					}
					else {
						ftm.run(FunTweenManager.Action.PlayBackwards);
					}
					isAnimationPlaying = true;
				}
	
				GUI.backgroundColor = Color.white;
	
				if (GUILayout.Button(EditorGUIUtility.FindTexture("d_endButton"), EditorStyles.toolbarButton)) {
					if (!applicationPlaying) {
						if (!visualizationMode) {
							startVisualization(ftm);
						}
						ftm.timeline.timeline.timePassed = ftm.timeline.timeline.duration;
						stopTimeUpdate();
					}
					else {
						ftm.timeline.timeline.timePassed = ftm.timeline.timeline.duration;
						ftm.run(FunTweenManager.Action.Stop);
					}
					isAnimationPlaying = false;
				}
	
				GUILayout.Space(10f);
	
				if (GUILayout.Button(EditorGUIUtility.FindTexture("d_playLoopOff"), EditorStyles.toolbarButton)) {
					if (!applicationPlaying) {
						playingBackwards = !playingBackwards;
					}
					else {
						ftm.run(FunTweenManager.Action.Reverse);
					}
				}
	
				GUILayout.FlexibleSpace();
				
				if (applicationPlaying || !visualizationMode) GUI.enabled = false;
				GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
				if (GUILayout.Button(EditorGUIUtility.FindTexture("P4_DeletedLocal"), EditorStyles.toolbarButton)) {
					stopVisualization();
					isAnimationPlaying = false;
					return;
				}
			}
	
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
			if (onSettingsGUI != null) {
				onSettingsGUI(timelineOffset - 1.5f, visualizationMode);			
			}
			GUILayout.EndVertical ();
			GUILayout.Space (1.5f);
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
		}
		
		void drawTicksGUI(){
			if (Event.current.type == EventType.Repaint) {
				EditorStyles.toolbar.Draw (timeRect, GUIContent.none, 0);
			}
			Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
			var count = 0;
			for (var x = timeRect.x - scroll.x; x < timeRect.width; x += ZOOM * timeZoomFactor) {
				Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
				if(x >= timelineOffset){
					if(count % 5 == 0){ 
						Handles.DrawLine(new Vector3(x, 7, 0), new Vector3(x, 17, 0));
						var displayMinutes = Mathf.FloorToInt(count / 5.0f * timeFactor[timeIndexFactor] / 60.0f);
						var	displaySeconds = Mathf.FloorToInt(count / 5.0f * timeFactor[timeIndexFactor] % 60.0f);
						var content = new GUIContent($"{displayMinutes:0}:{displaySeconds:00}");
						var size = ((GUIStyle)"Label").CalcSize(content);
						size.x = Mathf.Clamp(size.x, 0.0f, timeRect.width - x);
						GUI.Label(new Rect(x, -2, size.x, size.y), content);
					} else {
						Handles.DrawLine(new Vector3(x, 13, 0), new Vector3(x, 17, 0));
					}
				}
				count++;
			}
			Handles.color = Color.white;
		}
	
		static Option<Object[]> getTimelineTargets(FunTweenManager ftm) =>
			ftm.timeline.elements.opt().map( elements =>
					elements
						.map(elem => elem.element.getTargets())
						.Aggregate((acc, curr) => acc.concat(curr))
			);
	
		void doTimelineEvents(Option<FunTweenManager> funTweenManager){
			if (!GUI.enabled && !visualizationMode) {
				return;
			}
			var ev = Event.current;

			switch (ev.rawType) {
			case EventType.MouseDown:
				
				if (new Rect(timelineOffset - 5, 37, 10, _drawRect.height).Contains(ev.mousePosition)){
					changeOffset = true;
					clickOffset = timePosition - timelineOffset;
				}
				
				if (new Rect (timelineOffset, 0, _drawRect.width, 37).Contains(Event.current.mousePosition) && Event.current.button == 0) {
	
					if (funTweenManager.valueOut(out var ftm) && getTimelineTargets(ftm).valueOut(out var data) &&
					data.All(target => target != null)) {
						
						timePosition = Event.current.mousePosition.x + scroll.x;
						changeTime = true;
	
						if (!applicationPlaying) {
							if (visualizationMode) {
								stopTimeUpdate();
							}
							else {
								Undo.RegisterCompleteObjectUndo(data, "targets saved");
								ftm.recreate();
							}
						}
						else {
							ftm.run(FunTweenManager.Action.Stop);	
						}
						
						ftm.timeline.timeline.timePassed = currentTime;
					}
					else {
						Log.d.warn($"Set targets before evaluating!");
					}
	
					ev.Use();
				}
				break;
			case EventType.MouseUp:
				if (changeTime && !visualizationMode) {
					Undo.RevertAllInCurrentGroup();
				}
				changeTime = false;
				changeOffset = false;
	
				break;
			case EventType.MouseDrag:
				if (changeTime && Event.current.button == 0) {
					timePosition = Event.current.mousePosition.x + scroll.x;
	
					if (funTweenManager.valueOut(out var ftm)) {
						if (!applicationPlaying) {
							ftm.timeline.timeline.timePassed = Mathf.Clamp(currentTime, 0, ftm.timeline.timeline.duration);
						}
						else {
							ftm.timeline.timeline.timePassed = Mathf.Clamp(currentTime, 0, ftm.timeline.timeline.duration);
							ftm.run(FunTweenManager.Action.Stop);
						}
						isAnimationPlaying = false;
					}
				}
				switch (ev.button) {
				case 0:
					if (changeOffset) {
						timelineOffset = ev.mousePosition.x;
						timelineOffset = Mathf.Clamp(timelineOffset, 170, _drawRect.width - 170);
						timePosition = timelineOffset + clickOffset;
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
				var f = timeZoomFactor;
				if (timeIndexFactor == timeFactor.Length - 1 && f < 0.5f) {
					f = 0.5f;
				} else {
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
				
				ev.Use ();
				break;
			default: break;
			}
		}
	
		public float secondsToGUI(float seconds){
			return seconds / timeFactor[timeIndexFactor] * ZOOM * timeZoomFactor * 5.0f;
		}
		
		public float GUIToSeconds(float x){
			var guiSecond = ZOOM * timeZoomFactor * 5.0f / timeFactor[timeIndexFactor];
			return x / guiSecond;
		}
		
		public void recalculateTimelineWidth(Option<List<TimelineNode>> funNodes) => lastNodeTime = secondsToGUI(
			funNodes.fold(
				() => 0,
				nodes => nodes.Max(node => node.getEnd())
			)
		);
	}
}
#endif

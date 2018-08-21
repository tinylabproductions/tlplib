using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
[Serializable]
public class Timeline  {
	public delegate void TimelineGUICallback(Rect position);
	public TimelineGUICallback onTimelineGUI;
	public delegate void SettingsGUICallback(float width);
	public SettingsGUICallback onSettingsGUI;
	public delegate void OnTimelineClick(float time);
	public OnTimelineClick onTimelineClick;

	Rect _timeRect, _drawRect, blackBarRect;
	const float ZOOM = 20;
	readonly int[] timeFactor = {1,5,10,30,60,300,600,1800,3600,7200};
	
	public Rect timeRect {
		get => _timeRect;
		private set => _timeRect = value;
	}
	
	public Rect drawRect {
		get => _drawRect;
		private set => _drawRect = value;
	}

	float currentTime {
		get => GUIToSeconds(timePosition - timelineOffset);
		set => timePosition = secondsToGUI (value) + timelineOffset;
	}

	public bool visualizationMode {
		get => _visualizationMode;
		private set => _visualizationMode = value;
	}

	float timelineOffset = 300;
	int timeIndexFactor = 1;
	Vector2 scroll, expandView;
	bool isAnimationPlaying, changeTime, changeOffset, isInPlayMode, _visualizationMode;
	float timePosition, clickOffset, timeZoomFactor = 1, lastNodeTime;
	double lastSeconds;
	
	Option<EditorApplication.CallbackFunction> updateDelegateOpt;


	public void doTimeline(Rect position, Option<FunTweenManager> funTweenManager) {
		isInPlayMode = Application.isPlaying;
		drawRect = position;
		timeRect = new Rect (position.x + timelineOffset, position.y, position.width - 15, 20);
		blackBarRect = new Rect (position.x + timelineOffset - 1, position.y + 19, position.width, 16);

		if (funTweenManager.valueOut(out var ftm)) {
			currentTime = ftm.currentTime;
		}


		doCursor ();
		doToolbarGUI (position, funTweenManager);
		drawTicksGUI ();
		doEvents (funTweenManager);
	
		scroll = GUI.BeginScrollView(
			new Rect(position.x + timelineOffset,
				timeRect.height + blackBarRect.height,
				position.width - timelineOffset,
				position.height - timeRect.height - blackBarRect.height),
			scroll,
			new Rect(0, 0, position.width + lastNodeTime + expandView.x - timelineOffset, position.height + 400 + expandView.y), true, true
		);
		
		doLines ();
		
		if (onTimelineGUI != null) {
			onTimelineGUI(
				new Rect(scroll.x, scroll.y,
					position.width - timelineOffset - 15 + scroll.x,
					drawRect.height + scroll.y)
			);			
		}

		GUI.EndScrollView();
		doBlackBar ();
		doTimelineGUI ();
	}

	void doLines(){
		Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
		for (var y = 0; y < (int)drawRect.height + scroll.y; y += 20) {
			Handles.DrawLine(new Vector3(0, y, 0), new Vector3(drawRect.width+scroll.x, y, 0));	
		}
		Handles.color = Color.white;
	}
	
	void doTimelineGUI(){
		if ((changeTime || isAnimationPlaying || Application.isPlaying || visualizationMode)
			&& timePosition - scroll.x >= timelineOffset && timePosition - scroll.x < drawRect.width - 15) {
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
			Handles.DrawLine (new Vector3 (timePosition - scroll.x, 0, 0), new Vector3 (timePosition - scroll.x, drawRect.height - 15, 0));
			Handles.color = Color.white;
		}
	}
	
	public static double currentRealSeconds() {
		var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var currentSeconds = (DateTime.UtcNow - epochStart).TotalSeconds;
 
		return currentSeconds;
	}
	
	void doCursor(){
		EditorGUIUtility.AddCursorRect(new Rect(timelineOffset - 5, 37, 10, drawRect.height), MouseCursor.ResizeHorizontal);
	}
	
	void doBlackBar(){
		if (Event.current.type == EventType.Repaint) {
			((GUIStyle)"AnimationEventBackground").Draw(blackBarRect, GUIContent.none, 0);
		}
	}

	void updateAnimation(FunTweenManager ftm) {
		var currentSeconds = currentRealSeconds();
		//Log.d.warn($"current time {currentSeconds} last time {lastSeconds}");
		if (lastSeconds < 1) lastSeconds = currentSeconds;
		var timeDiff = (float)(currentSeconds - lastSeconds);
		lastSeconds = currentSeconds;
		ftm.manager.update(timeDiff);
	}

	void startUpdatingTime(FunTweenManager ftm) {
		ftm.recreate();
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

	void stopTimeUpdate() {
		updateDelegateOpt.map(updateDelegate => EditorApplication.update -= updateDelegate);
	}

	void startVisualization(FunTweenManager ftm) {
		if (!visualizationMode && getTimelineTargets(ftm).valueOut(out var data)) {
			
			visualizationMode = true;
			Undo.RegisterCompleteObjectUndo(data, "Animating targets");
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

		GUI.enabled = true;
		if (funTweenManager.valueOut(out var ftm)) {
			
			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_beginButton"), EditorStyles.toolbarButton)) {
				if (!isInPlayMode) {
					stopTimeUpdate();
					startVisualization(ftm);
				}
				ftm.run(FunTweenManager.Action.Stop);
				ftm.timeline.timeline.timePassed = 0;
				isAnimationPlaying = false;
			}

			GUI.backgroundColor = new Color(0, 0.8f, 1, 0.5f);
			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_StepButton"), EditorStyles.toolbarButton)) {
				if (!isInPlayMode && !isAnimationPlaying) {
					startUpdatingTime(ftm);
				}
				ftm.run(FunTweenManager.Action.PlayForwards);
				isAnimationPlaying = true;
			}
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button(
				!isAnimationPlaying
					? EditorGUIUtility.FindTexture("d_PlayButton")
					: EditorGUIUtility.FindTexture("d_PlayButton On"), EditorStyles.toolbarButton)
			) {
				if (isAnimationPlaying) {
					if (!isInPlayMode) { stopTimeUpdate(); } 
					ftm.run(FunTweenManager.Action.Stop);
					isAnimationPlaying = false;
				}
				else {
					if (!isInPlayMode) {
						startUpdatingTime(ftm);
					}
					
					ftm.run(FunTweenManager.Action.Resume);
					isAnimationPlaying = true;
				}
			}

			if (GUILayout.Button(isAnimationPlaying
				? EditorGUIUtility.FindTexture("d_PauseButton")
				: EditorGUIUtility.FindTexture("d_PauseButton On"), EditorStyles.toolbarButton)
			) {
				if (!isInPlayMode){
					stopTimeUpdate();
					startVisualization(ftm);
				}
				else {
					ftm.run(FunTweenManager.Action.Stop);
				}
				isAnimationPlaying = false;

			}

			GUI.backgroundColor = new Color(1, 0, 0, 0.5f);
			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_StepLeftButton"), EditorStyles.toolbarButton)) {
				if (!isInPlayMode && !isAnimationPlaying) {
					startUpdatingTime(ftm);
				}

				ftm.run(FunTweenManager.Action.PlayBackwards);
				isAnimationPlaying = true;
			}

			GUI.backgroundColor = Color.white;

			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_endButton"), EditorStyles.toolbarButton)) {
				if (!isInPlayMode) {
					stopTimeUpdate();
					startVisualization(ftm);
				}
				ftm.run(FunTweenManager.Action.Stop);
				ftm.timeline.timeline.timePassed = ftm.timeline.timeline.duration;
				isAnimationPlaying = false;
			}

			GUILayout.Space(10f);

			if (GUILayout.Button(EditorGUIUtility.FindTexture("d_playLoopOff"), EditorStyles.toolbarButton)) {
				ftm.run(FunTweenManager.Action.Reverse);
			}

			GUILayout.FlexibleSpace();
			
			if (isInPlayMode || !visualizationMode) GUI.enabled = false;
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
			onSettingsGUI(timelineOffset - 1.5f);			
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

	void doEvents(Option<FunTweenManager> funTweenManager){
		if (!GUI.enabled) {
			return;
		}
		var ev = Event.current;
		
		switch (ev.rawType) {
		case EventType.MouseDown:
			
			if (new Rect(timelineOffset - 5, 37, 10, drawRect.height).Contains(ev.mousePosition)){
				changeOffset = true;
				clickOffset = timePosition - timelineOffset;
			}
			
			if (new Rect (timelineOffset, 0, drawRect.width, 37).Contains(Event.current.mousePosition) && Event.current.button == 0) {

				if (funTweenManager.valueOut(out var ftm) && getTimelineTargets(ftm).valueOut(out var data) &&
				data.All(target => target != null)) {
					
					timePosition = Event.current.mousePosition.x + scroll.x;
					changeTime = true;

					if (onTimelineClick != null){
						onTimelineClick(currentTime);
					}
					
					if (!Application.isPlaying && !visualizationMode) {
						ftm.recreate();
						Undo.RegisterCompleteObjectUndo(data, "targets saved");
					}

					if (!isInPlayMode) {
						stopTimeUpdate();
						isAnimationPlaying = false;
					}
					
					ftm.timeline.timeline.timePassed = currentTime;
					//ftm.run(FunTweenManager.Action.Stop);
				}
				else {
					Log.d.warn($"Set targets before evaluating!");
				}

				ev.Use();
			}
			break;
		case EventType.MouseUp:
			if (changeTime) {
				Undo.RevertAllInCurrentGroup();
			}

			changeTime = false;
			changeOffset = false;

			break;
		case EventType.MouseDrag:
			if (changeTime && Event.current.button == 0) {
				timePosition = Event.current.mousePosition.x + scroll.x;

				if (funTweenManager.valueOut(out var ftm)) {
					ftm.timeline.timeline.timePassed = currentTime;
				//	ftm.run(FunTweenManager.Action.Stop);
				}
			}
			switch (ev.button) {
			case 0:
				
				if(changeOffset){
					timelineOffset = ev.mousePosition.x;
					timelineOffset = Mathf.Clamp (timelineOffset, 170, drawRect.width - 170);
					timePosition = timelineOffset + clickOffset;
					timePosition = Mathf.Clamp(timePosition, timelineOffset, Mathf.Infinity);
					ev.Use();
				}
		
				if(changeTime){
					timePosition = Event.current.mousePosition.x+scroll.x;
					timePosition=Mathf.Clamp(timePosition,timelineOffset,Mathf.Infinity);
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
		}
		
	}

	public float secondsToGUI(float seconds){
		return seconds / timeFactor[timeIndexFactor] * ZOOM * timeZoomFactor * 5.0f;
	}
	
	public float GUIToSeconds(float x){
		var guiSecond = ZOOM * timeZoomFactor * 5.0f / timeFactor[timeIndexFactor];
		return x / guiSecond;
	}
	
	public void recalculateTimelineWidth(Option<List<FunSequenceNode>> funNodes) => lastNodeTime = secondsToGUI(
		funNodes.fold(
			() => 0,
			nodes => nodes.Max(node => node.getEnd())
		)
	);
}

}

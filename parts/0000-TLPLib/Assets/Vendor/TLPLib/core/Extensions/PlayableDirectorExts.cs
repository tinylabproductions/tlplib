using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using UnityEngine.Playables;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class PlayableDirectorExts {
    public static IEnumerator play(
      this PlayableDirector director, PlayableAsset asset, bool logErrorIfInactive = true
    ) {
      // Directors do not play if the game object is not active.
      if (!director.isActiveAndEnabled) {
        if (logErrorIfInactive) logError(director, $"Wanted to play {asset} on director");
      }
      else {
        playSimple(director, asset, logErrorIfInactive = false);
        while (!Mathf.Approximately((float) director.time, (float) asset.duration))
          yield return null;
      }
    }

    /// <summary>
    /// Plays to end while `continuePlaying` is true.
    /// Stops and goes to last frame when `continuePlaying` becomes false
    /// </summary>
    public static IEnumerator playWhile(
      this PlayableDirector director, PlayableAsset asset, Func<bool> continuePlaying, bool logErrorIfInactive = true
    ) {
      // Directors do not play if the game object is not active.
      if (!director.isActiveAndEnabled) {
        if (logErrorIfInactive) logError(director, $"Wanted to play {asset} on director");
      }
      else {
        playSimple(director, asset, logErrorIfInactive = false);
        while (!Mathf.Approximately((float) director.time, (float) asset.duration)) {
          if (!continuePlaying()) {
            director.jumpToEnd();
            break;
          }
          yield return null;
        }
      }
    }

    public static void playSimple(
      this PlayableDirector director, PlayableAsset asset, bool logErrorIfInactive = true
    ) {
      if (!director.isActiveAndEnabled) {
        if (logErrorIfInactive) logError(director, $"Wanted to play {asset} on director");
      }
      director.gameObject.SetActive(true);
      director.enabled = true;
      director.Play(asset, DirectorWrapMode.Hold);
      director.Evaluate();
    }

    public static void jumpToEnd(this PlayableDirector director, bool logErrorIfInactive = true) {
      if (!director.isActiveAndEnabled) {
        if (logErrorIfInactive) logError(director, $"Wanted to jump to end on director");
      }
      else {
        var asset = director.playableAsset;
        if (asset) {
          director.time = asset.duration;
        }
      }
    }

    public static bool isPlaying(this PlayableDirector director) =>
      !Mathf.Approximately((float) director.time, (float) director.playableAsset.duration);

    public static IEnumerator play(this PlayableDirector director) {
       yield return director.play(director.playableAsset);
    }
    
    public static void setInitial(this PlayableDirector director, PlayableAsset asset, bool logErrorIfInactive = true) {
      if (!director.isActiveAndEnabled) {
        if (logErrorIfInactive) logError(director, $"Wanted to set initial state for {asset}");
      }
      director.Play(asset, DirectorWrapMode.Hold);
      director.Evaluate();
      director.Stop();
    }

    static void logError(PlayableDirector director, string message) {
      Log.d.error(
        message + "\nThis does not work, ensure the director is active and enabled.",
        director
      );
    }
  }
}
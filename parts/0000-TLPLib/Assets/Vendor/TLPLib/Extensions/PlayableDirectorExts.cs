using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class PlayableDirectorExts {
    public static IEnumerator play(this PlayableDirector director, PlayableAsset asset) {
      director.Play(asset, DirectorWrapMode.Hold);
      director.Evaluate();
      while (director.time < asset.duration) yield return null;
    }
    public static void setInitial(this PlayableDirector director, PlayableAsset asset) {
      director.Play(asset, DirectorWrapMode.Hold);
      director.Evaluate();
      director.Stop();
    }
  }
}
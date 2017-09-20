using System;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.video {
  public interface IVideoPlayer {
    void playFromStreamingAssets(string fileName, Url clickUrl);
  }

  public static class VideoPlayer {
    public static IVideoPlayer create(Action onStartShow, Action onVideoComplete) {
#if UNITY_ANDROID && !UNITY_EDITOR
      return new AndroidVideoPlayer(onStartShow, onVideoComplete);
#else
      return new VideoPlayerNoOp();
#endif
    }
  }

  public class VideoPlayerNoOp : IVideoPlayer {
    public void playFromStreamingAssets(string fileName, Url clickUrl) { }
  }
}
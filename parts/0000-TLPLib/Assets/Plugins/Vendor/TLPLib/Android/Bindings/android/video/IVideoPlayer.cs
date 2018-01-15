using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Logger;

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
    public void playFromStreamingAssets(string fileName, Url clickUrl) {
      if (Log.d.isDebug()) Log.d.debug(
        $"{nameof(VideoPlayerNoOp)}#{nameof(playFromStreamingAssets)}({fileName}, {clickUrl})"
      );
    }
  }
}
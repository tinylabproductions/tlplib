using System;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.video {
  public interface IVideoPlayer {
    void show(string fileName, Url clickUrl);
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
    public void show(string fileName, Url clickUrl) { }
  }

  public interface IVideoListener {
    Action showed { get; }
    Action canceled { get; }
    Action completed { get; }
    Action clicked { get; }
  }
}
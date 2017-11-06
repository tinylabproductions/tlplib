#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Ads;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.video {
  public class AndroidVideoPlayer : IVideoPlayer {
    readonly MediaPlayerBinding binding;
    readonly Action onStartShow, onVideoComplete;

    public AndroidVideoPlayer(Action onStartShow, Action onVideoComplete) {
      binding = new MediaPlayerBinding();
      this.onStartShow = onStartShow;
      this.onVideoComplete = onVideoComplete;
    }

    public void playFromStreamingAssets(string fileName, Url clickUrl) {
      var listener = new VideoListener();
      if (Log.d.isDebug()) {
        listener.canceled += () => logDebug("canceled");
        listener.videoCompleted += () => logDebug("completed");
        listener.clicked += () => logDebug("clicked");
      }
      onStartShow();
      listener.videoCompleted += onVideoComplete;
      binding.showVideo(fileName, clickUrl.url, listener);
    }

    static void logDebug(string msg) {
      Log.d.debug($"{nameof(AndroidVideoPlayer)}|{msg}");
    }

    class MediaPlayerBinding : Binding {
      public MediaPlayerBinding() 
        : base(new AndroidJavaObject("com.tinylabproductions.tlplib.video_player.VideoPlayerBridge")) { }

      public void showVideo(string fileName, string clickUrl, VideoListener listener) 
        => java.CallStatic("playFromStreamingAssets", fileName, clickUrl, listener); 
    }

    public class VideoListener : BaseAdListener {
      public VideoListener() : base("com.tinylabproductions.tlplib.video_player.VideoPlayerListener") { }
      public event Action canceled, videoCompleted, clicked;

      [UsedImplicitly] void onCancel() => invoke(() => canceled);
      [UsedImplicitly] void onVideoComplete() => invoke(() => videoCompleted);
      [UsedImplicitly] void onVideoClick() => invoke(() => clicked);
    }
  }
}
#endif
package com.tinylabproductions.tlplib.video_player;

import android.app.Activity;
import android.content.Intent;

import com.unity3d.player.UnityPlayer;

@SuppressWarnings("unused")
public class VideoPlayerBridge {
  public static void showVideo(String name, String url, VideoPlayerListener listener) {
    AndroidVideoPlayer.setListener(listener);
    Activity activity = UnityPlayer.currentActivity;
    Intent intent = new Intent(activity, AndroidVideoPlayer.class);
    intent.setFlags(Intent.FLAG_ACTIVITY_NO_HISTORY);
    intent.putExtra(AndroidVideoPlayer.FILE_NAME, name);
    intent.putExtra(AndroidVideoPlayer.URL_TO_OPEN, url);
    activity.startActivity(intent);
  }
}

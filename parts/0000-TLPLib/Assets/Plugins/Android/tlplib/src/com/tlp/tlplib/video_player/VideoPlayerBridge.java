package com.tlp.tlplib.video_player;

import android.app.Activity;
import android.content.Intent;

import com.unity3d.player.UnityPlayer;

@SuppressWarnings("unused")
public class VideoPlayerBridge {
  public static void playFromStreamingAssets(
      String fileFromStreamingAssets, String clickUrl, VideoPlayerListener listener
  ) {
    AndroidVideoPlayer.setListener(listener);
    Activity activity = UnityPlayer.currentActivity;
    Intent intent = new Intent(activity, AndroidVideoPlayer.class);
    intent.setFlags(Intent.FLAG_ACTIVITY_NO_HISTORY);
    intent.putExtra(AndroidVideoPlayer.FILE_NAME, fileFromStreamingAssets);
    intent.putExtra(AndroidVideoPlayer.URL_TO_OPEN, clickUrl);
    activity.startActivity(intent);
  }
}

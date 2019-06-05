package com.tlp.tlplib.video_player;

public interface VideoPlayerListener {
  void onCancel();
  void onVideoComplete();
  void onVideoClick();
}

package com.tinylabproductions.tlplib.video_player;

public interface VideoPlayerListener {
  void onCancel();
  void onVideoComplete();
  void onVideoClick();
}

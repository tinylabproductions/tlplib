package com.tinylabproductions.tlplib.video_player;

import android.app.Activity;
import android.content.Intent;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.VideoView;
import com.tinylabproductions.tlplib.logging.Log;
import com.tlplib.R;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.URL;

public class AndroidVideoPlayer extends Activity {
  static final String TAG = "TLP-AndroidVideoPlayer";
  static final String FILE_NAME = "file-name";
  static final String URL_TO_OPEN = "open-url";
  static VideoPlayerListener listener;

  @Override
  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    setContentView(R.layout.media_player_layout);
    VideoView videoView = (VideoView) findViewById(R.id.video_display);
    Button closeButton = (Button) findViewById(R.id.close_button);
    // This is needed so the listener wouldn't change if for some reason new Activity was created before closing the first one
    final VideoPlayerListener listenerInstance = listener;
    Intent intent = getIntent();
    final String fileName = intent.getStringExtra(FILE_NAME);
    final String urlToOpen = intent.getStringExtra(URL_TO_OPEN);

    videoView.setOnCompletionListener(new MediaPlayer.OnCompletionListener() {
      @Override
      public void onCompletion(MediaPlayer mp) {
        if (listenerInstance != null) listenerInstance.onVideoComplete();
        openUrl(urlToOpen);
      }
    });
    closeButton.setOnClickListener(new View.OnClickListener() {
      @Override
      public void onClick(View v) {
        if (listenerInstance != null) listenerInstance.onCancel();
        closeActivity();
      }
    });
    videoView.setOnTouchListener(new View.OnTouchListener() {
      @Override
      public boolean onTouch(View v, MotionEvent event) {
        if (listenerInstance != null) listenerInstance.onVideoClick();
        openUrl(urlToOpen);
        return true;
      }
    });

    File file = extractFromResource("/assets/" + fileName);
    videoView.setVideoPath(file.getAbsolutePath());
    videoView.requestFocus();
    videoView.start();
  }

  static void setListener(VideoPlayerListener videoListener){
    listener = videoListener;
  }

  private void openUrl(String url){
    Intent i = new Intent(Intent.ACTION_VIEW);
    try {
      i.setData(Uri.parse(url));
      startActivity(i);
    } catch (Exception ex) {
      Log.log(Log.ERROR, TAG, ex.toString());
    }
  }

  private File extractFromResource(String resource) {
    File file = null;
    URL res = getClass().getResource(resource);
    if (res.toString().startsWith("jar:")) {
      try {
        InputStream input = getClass().getResourceAsStream(resource);
        file = File.createTempFile("tempfile", ".tmp");
        OutputStream out = new FileOutputStream(file);
        int read;
        byte[] bytes = new byte[1024];

        while ((read = input.read(bytes)) != -1) {
          out.write(bytes, 0, read);
        }
        file.deleteOnExit();
      } catch (IOException ex) {
        Log.log(Log.ERROR, "TLP", ex.toString());
      }
    } else {
      //this will probably work in your IDE, but not from a JAR
      file = new File(res.getFile());
    }

    if (file != null && !file.exists()) {
      throw new RuntimeException("Error: File " + file + " not found!");
    }
    return file;
  }

  void closeActivity(){
    finish();
  }

}

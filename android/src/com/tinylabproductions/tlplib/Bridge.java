package com.tinylabproductions.tlplib;

import android.app.Activity;
import android.content.Intent;
import android.content.res.Configuration;
import android.net.Uri;
import com.unity3d.player.UnityPlayer;

import java.io.File;

@SuppressWarnings("UnusedDeclaration")
public class Bridge {
  public static void sharePNG(String path, String title, String sharerText) {
    Intent shareIntent = new Intent();
    shareIntent.setAction(Intent.ACTION_SEND);
    shareIntent.putExtra(Intent.EXTRA_TEXT, sharerText);
    shareIntent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(new File(path)));
    shareIntent.setType("image/png");
    shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
    UnityPlayer.currentActivity.startActivity(
      Intent.createChooser(shareIntent, title)
    );
  }

  public static boolean isTablet() {
    Activity current = UnityPlayer.currentActivity;
    Configuration cfg = current.getResources().getConfiguration();
    int sizeFlag = cfg.screenLayout & Configuration.SCREENLAYOUT_SIZE_MASK;

    return sizeFlag == Configuration.SCREENLAYOUT_SIZE_XLARGE
        || sizeFlag == Configuration.SCREENLAYOUT_SIZE_LARGE;
  }
}

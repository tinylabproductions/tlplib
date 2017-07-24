package com.tinylabproductions.tlplib.utils;

import android.util.Log;

import com.tinylabproductions.tlplib.Tag;
import com.unity3d.player.UnityPlayer;

public class Utils {
    public static void runOnUiSafe(final String logLabel, final Runnable f) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    f.run();
                } catch (Throwable e) {
                    // TODO: Crashalytics
                    Log.e(Tag.TAG, "Error running [" + logLabel + "] on UI thread", e);
                }
            }
        });
    }
}

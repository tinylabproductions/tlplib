package com.tinylabproductions.tlplib.util;

import com.tinylabproductions.tlplib.Tag;
import com.tinylabproductions.tlplib.logging.Log;
import com.unity3d.player.UnityPlayer;

@SuppressWarnings("unused")
public class Utils {
    public static void runOnUiSafe(final String logLabel, final Runnable f) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    f.run();
                } catch (Throwable e) {
                    Log.log(Log.ERROR, Tag.TAG, "Error running [" + logLabel + "] on UI thread", e);
                }
            }
        });
    }
}

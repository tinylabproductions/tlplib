package com.tinylabproductions.tlplib;

import android.content.Intent;
import android.util.Log;

import java.util.HashSet;
import java.util.Set;

public class ActivityResultTracker {
    public final int REQUEST_CODE_BASE = 1000000;
    private int requestCode = REQUEST_CODE_BASE;

    final Set<UnityActivity.IActivityResult> activityResultListeners = new HashSet<>();

    public void subscribeOnActivityResult(UnityActivity.IActivityResult f) {
        activityResultListeners.add(f);
    }

    public void unsubscribeOnActivityResult(UnityActivity.IActivityResult f) {
        if (activityResultListeners.contains(f))
            activityResultListeners.remove(f);
    }

    public int generateRequestCode() { return requestCode++; }

    public interface IActivityResult {
        void onActivityResult(int requestCode, int resultCode, Intent data);
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        for (UnityActivity.IActivityResult f : activityResultListeners) {
            try {
                f.onActivityResult(requestCode, resultCode, data);
            } catch (Exception e) {
                Log.e(Tag.TAG, "Error executing onActivityResult subscriber " + f, e);
            }
        }
    }
}

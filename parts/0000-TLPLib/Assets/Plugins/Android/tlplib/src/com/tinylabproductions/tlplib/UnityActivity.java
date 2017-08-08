package com.tinylabproductions.tlplib;

import android.content.Intent;
import android.util.Log;
import com.unity3d.player.UnityPlayerActivity;
import java.util.HashSet;
import java.util.Set;

public class UnityActivity extends UnityPlayerActivity {
    private int requestCodeBase = 1000000;
    public interface IActivityResult {
        void onActivityResult(int requestCode, int resultCode, Intent data);
    }

    Set<IActivityResult> activityResultListeners = new HashSet<>();

    public void subscribeOnActivityResult(IActivityResult f) {
        activityResultListeners.add(f);
    }

    public void unsubscribeOnActivityResult(IActivityResult f) {
        if (activityResultListeners.contains(f)) activityResultListeners.remove(f);
    }

    public int generateRequestCode() {
        return requestCodeBase++;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        for (IActivityResult f : activityResultListeners) {
            try {
                f.onActivityResult(requestCode, resultCode, data);
            } catch (Exception e) {
                Log.e(Tag.TAG, "Error executing onActivityResult subscriber");
            }
        }
    }
}

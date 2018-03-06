package com.tinylabproductions.tlplib;

import android.content.Intent;
import android.util.Log;
import com.unity3d.player.UnityPlayerActivity;
import java.util.HashSet;
import java.util.Set;

public class UnityActivity extends UnityPlayerActivity {
    public final int REQUEST_CODE_BASE = 1000000;
    private int requestCode = REQUEST_CODE_BASE;

    public interface IActivityResult {
        void onActivityResult(int requestCode, int resultCode, Intent data);
    }

    final Set<IActivityResult> activityResultListeners = new HashSet<>();

    public void subscribeOnActivityResult(IActivityResult f) {
        activityResultListeners.add(f);
    }

    public void unsubscribeOnActivityResult(IActivityResult f) {
        if (activityResultListeners.contains(f))
            activityResultListeners.remove(f);
    }

    public int generateRequestCode() {
        return requestCode++;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        for (IActivityResult f : activityResultListeners) {
            try {
                f.onActivityResult(requestCode, resultCode, data);
            } catch (Exception e) {
                Log.e(Tag.TAG, "Error executing onActivityResult subscriber " + f, e);
            }
        }
    }
}

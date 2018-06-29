package com.tinylabproductions.tlplib;

import android.content.Intent;
import com.unity3d.player.UnityPlayerActivity;

public class UnityActivity extends UnityPlayerActivity implements IActivityWithResultTracker {
    private final ActivityResultTracker tracker = new ActivityResultTracker();

    @Override
    public ActivityResultTracker getTracker() { return tracker; }

    public interface IActivityResult {
        void onActivityResult(int requestCode, int resultCode, Intent data);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        tracker.onActivityResult(requestCode, resultCode, data);
        super.onActivityResult(requestCode, resultCode, data);
    }
}

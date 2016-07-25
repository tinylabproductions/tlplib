package com.tinylabproductions.tlplib.referrer;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.util.Log;
import com.tinylabproductions.tlplib.Tag;

@SuppressWarnings("WeakerAccess")
public class InstallReferrerReceiver extends BroadcastReceiver {
    public static final String PREF_REFERRER = "referrer";

    public static SharedPreferences getPrefs(Context context) {
        return context.getSharedPreferences("tlplib_InstallReferrerReceiver", Context.MODE_PRIVATE);
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        String referrer = intent.getStringExtra("referrer");
        Log.d(Tag.TAG, "InstallReferrerReceiver=" + referrer);

        SharedPreferences prefs = getPrefs(context);
        SharedPreferences.Editor editor = prefs.edit();
        editor.putString(PREF_REFERRER, referrer);
        editor.apply();
    }
}

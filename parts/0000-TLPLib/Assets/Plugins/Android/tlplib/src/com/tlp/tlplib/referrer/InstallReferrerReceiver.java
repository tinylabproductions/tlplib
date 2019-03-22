package com.tlp.tlplib.referrer;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;
import com.tlp.tlplib.Tag;
import java.util.ArrayList;
import java.util.List;

@SuppressWarnings("WeakerAccess")
public class InstallReferrerReceiver extends BroadcastReceiver {
    public static final String PREF_REFERRER = "referrer";
    static final String TAG = "Unity-TLPLib";

    public static SharedPreferences getPrefs(Context context) {
        return context.getSharedPreferences("tlplib_InstallReferrerReceiver", Context.MODE_PRIVATE);
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        broadcastEvent(context, intent);

        String referrer = intent.getStringExtra("referrer");
        Log.d(Tag.TAG, "InstallReferrerReceiver=" + referrer);

        SharedPreferences prefs = getPrefs(context);
        SharedPreferences.Editor editor = prefs.edit();
        editor.putString(PREF_REFERRER, referrer);
        editor.apply();
    }

    // Android supports only one referrer receiver
    // Add this to your manifest to call other receivers
    // <meta-data android:name="com.yourpackage.yourclass" android:value="TLPLibInstallReferrerReceiver"/>

    private void broadcastEvent(Context context, Intent intent) {
        List<BroadcastReceiver> proxyClasses = getBroadcastReceivers(context, "TLPLibInstallReferrerReceiver");

        for (BroadcastReceiver r : proxyClasses) {
            try {
                r.onReceive(context, intent);
                Log.i(TAG, "Called onReceive on: " + r.getClass().getName());
            }
            catch (Exception e) {
                Log.e(TAG, "Exception calling onReceive: " + e.getMessage());
            }
        }
    }

    private List<BroadcastReceiver> getBroadcastReceivers(Context context, String name) {
        List<BroadcastReceiver> receivers = new ArrayList<BroadcastReceiver>();
        try {
            ApplicationInfo ai =
                    context.getPackageManager().getApplicationInfo(
                            context.getPackageName(), PackageManager.GET_META_DATA
                    );
            Bundle bundle = ai.metaData;
            for (String key : bundle.keySet()) {
                try {
                    Object bundleValue = bundle.get(key);
                    if ((bundleValue instanceof String)) {
                        String value = (String)bundleValue;
                        if (value.equals(name)) {
                            try {
                                Class<?> classObj = Class.forName(key);
                                BroadcastReceiver r = (BroadcastReceiver) classObj.newInstance();
                                receivers.add(r);
                                Log.i(TAG, "Found referrer receiver class: " + classObj);
                            }
                            catch (ClassCastException e) {
                                Log.e(TAG, "Class is not a BroadcastReceiver: " + value);
                            }
                            catch (ClassNotFoundException e) {
                                Log.e(TAG, "No referrer receiver class found: " + value);
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Log.e(TAG, "Exception: " + e.getMessage());
                }
            }
        }
        catch (PackageManager.NameNotFoundException e) {
            Log.e(TAG, "Failed to load meta-data, NameNotFound: " + e.getMessage());
        }
        catch (NullPointerException e) {
            Log.e(TAG, "Failed to load meta-data, NullPointer: " + e.getMessage());
        }
        return receivers;
    }
}

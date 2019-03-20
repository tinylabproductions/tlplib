package com.tlp.tlplib;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.location.Address;
import android.location.Geocoder;
import android.location.Location;
import android.location.LocationManager;
import android.net.Uri;
import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.IOException;
import java.util.List;
import java.util.Locale;

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

  public static String countryCodeFromLastKnownLocation() throws IOException {
    Activity current = UnityPlayer.currentActivity;
    LocationManager locationManager =
            (LocationManager) current.getSystemService(Context.LOCATION_SERVICE);
    Location location =
            locationManager
            .getLastKnownLocation(LocationManager.NETWORK_PROVIDER);
    if (location != null && Geocoder.isPresent()) {
      Geocoder gcd = new Geocoder(current, Locale.getDefault());
      List<Address> addresses;
      addresses = gcd.getFromLocation(location.getLatitude(), location.getLongitude(), 1);

      if (addresses != null && !addresses.isEmpty()) {
        Address address = addresses.get(0);
        return address.getCountryCode();
      }
    }
    return null;
  }
}

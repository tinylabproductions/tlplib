package com.tlp.tlplib.logging;

import android.util.Log;

/**
 * Created by Karolis Jucius on 2017-09-08.
 */

public class AndroidLogger implements ILogger {
  @Override
  public void log(int priority, String tag, String message) {
    Log.println(priority, tag, message);
  }

  @Override
  public void log(int priority, String tag, String message, Throwable throwable) {
    Log.println(priority, tag, message + "\n" + Log.getStackTraceString(throwable));
  }
}

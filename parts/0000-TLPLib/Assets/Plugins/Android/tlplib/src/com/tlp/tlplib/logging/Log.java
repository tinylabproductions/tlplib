package com.tlp.tlplib.logging;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by Karolis Jucius on 2017-09-08.
 */

public class Log {
  public static final int
    VERBOSE = android.util.Log.VERBOSE,
    DEBUG = android.util.Log.DEBUG,
    INFO = android.util.Log.INFO,
    WARN = android.util.Log.WARN,
    ERROR = android.util.Log.ERROR,
    ASSERT = android.util.Log.ASSERT;

  @SuppressWarnings("WeakerAccess")
  public final static List<ILogger> loggers = new ArrayList<>();

  static {
    loggers.add(new AndroidLogger());
  }

  public static void log(int priority, String tag, String message) {
    for (ILogger logger: loggers){
      logger.log(priority, tag, message);
    }
  }

  public static void log(int priority, String tag, String message, Throwable throwable) {
    for (ILogger logger: loggers){
      logger.log(priority, tag, message, throwable);
    }
  }

}

package com.tinylabproductions.tlplib.logging;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by Karolis Jucius on 2017-09-08.
 */

public class Log {
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

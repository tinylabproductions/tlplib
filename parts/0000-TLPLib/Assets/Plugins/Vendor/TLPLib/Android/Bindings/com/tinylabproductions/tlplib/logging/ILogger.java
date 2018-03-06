package com.tinylabproductions.tlplib.logging;

/**
 * Created by Karolis Jucius on 2017-09-08.
 */

public interface ILogger {
  void log(int priority, String tag, String message);
  void log(int priority, String tag, String message, Throwable throwable);
}

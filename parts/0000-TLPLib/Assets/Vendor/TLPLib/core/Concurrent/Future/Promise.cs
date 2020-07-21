﻿using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /** Asynchronous promise that can be used to complete an asynchronous future. **/
  public interface Promise<A> {
    /** Complete with value, exception if already completed. **/
    void complete(A v);
    /** Complete with value, return false if already completed. **/
    bool tryComplete(A v);
  }

  public static class PromiseExts {
    public static void completeSuccess<Err, Val>(this Promise<Either<Err, Val>> p, Val value) {
      p.complete(Either<Err, Val>.Right(value));
    }

    public static void completeSuccess<Val>(this Promise<Try<Val>> p, Val value) {
      p.complete(F.scs(value));
    }

    public static void tryCompleteSuccess<Err, Val>(this Promise<Either<Err, Val>> p, Val value) {
      p.tryComplete(Either<Err, Val>.Right(value));
    }

    public static void tryCompleteSuccess<Val>(this Promise<Try<Val>> p, Val value) {
      p.tryComplete(F.scs(value));
    }

    public static void completeError<Err, Val>(this Promise<Either<Err, Val>> p, Err error) {
      p.complete(Either<Err, Val>.Left(error));
    }

    public static void completeError<Val>(this Promise<Try<Val>> p, Exception error) {
      p.complete(F.err<Val>(error));
    }

    public static void tryCompleteError<Err, Val>(this Promise<Either<Err, Val>> p, Err error) {
      p.tryComplete(Either<Err, Val>.Left(error));
    }

    public static void tryCompleteError<Val>(this Promise<Try<Val>> p, Exception error) {
      p.tryComplete(F.err<Val>(error));
    }

    public static void completeOrLog<A>(this Promise<A> p, A v) {
      try {
        p.complete(v);
      }
      catch (Exception e) {
        // we will get exception if future is already completed, or listener action throws an error
        Log.d.error(e);
      }
    }
  }
}

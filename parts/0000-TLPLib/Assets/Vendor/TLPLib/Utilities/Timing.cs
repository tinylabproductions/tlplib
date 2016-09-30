using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Logger;
using Smooth.Pools;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public interface ITiming {
    void openScope(string name);
    void scopeIteration();
    void closeScope();
  }

  public struct TimingData {
    public readonly string scope;
    public readonly float startTime, endTime;
    public readonly uint iterations, innerScopes;

    public TimingData(string scope, float startTime, float endTime, uint iterations, uint innerScopes) {
      this.scope = scope;
      this.startTime = startTime;
      this.endTime = endTime;
      this.iterations = iterations;
      this.innerScopes = innerScopes;
    }

    public Duration duration => Duration.fromSeconds(endTime - startTime);

    public string durationStr { get {
      var durationS = $"{duration.millis}ms";
      if (iterations != 0) {
        var avg = Duration.fromSeconds(duration.seconds / iterations);
        durationS += $", iters={iterations}, avg iter={avg.millis}ms";
      }
      if (innerScopes != 0) {
        var avg = Duration.fromSeconds(duration.seconds / innerScopes);
        durationS += $", iscopes={innerScopes}, avg iscope={avg.millis}ms";
      }
      return durationS;
    } }

    public override string ToString() {
      return $"{nameof(TimingData)}[{scope}, {durationStr}]";
    } 
  }

  public static class ITimingExts {
    public static void scoped(this ITiming timing, string name, Action f) {
      timing.openScope(name);
      f();
      timing.closeScope();
    }

    public static A scoped<A>(this ITiming timing, string name, Fn<A> f) {
      timing.openScope(name);
      var ret = f();
      timing.closeScope();
      return ret;
    }

    public static ITiming ifLogLevel(this ITiming backing, Log.Level level, ILog log=null) =>
      new TimingConditional(backing, (log ?? Log.defaultLogger).willLog(level));
  }

  public class TimingNoOp : ITiming {
    public static ITiming instance = new TimingNoOp();
    TimingNoOp() {}

    public void openScope(string name) {}
    public void scopeIteration() {}
    public void closeScope() {}
  }

  public class TimingConditional : ITiming {
    readonly ITiming backing;
    readonly bool shouldRun;

    public TimingConditional(ITiming backing, bool shouldRun) {
      this.backing = backing;
      this.shouldRun = shouldRun;
    }

    public void openScope(string name) { if (shouldRun) backing.openScope(name); }
    public void scopeIteration() { if (shouldRun) backing.scopeIteration(); }
    public void closeScope() { if (shouldRun) backing.closeScope(); }
  }

  public class Timing : ITiming {
    class Data {
      public string scope;
      public float startTime;
      public uint iterations, innerScopes;
    }

    static readonly Pool<Data> dataPool = new Pool<Data>(() => new Data(), _ => { });
    readonly Stack<Data> scopes = new Stack<Data>();
    readonly Act<TimingData> onData;

    public Timing(Act<TimingData> onData)
    { this.onData = onData; }

    public void openScope(string name) {
      var hasParentScope = scopes.Count != 0;
      if (hasParentScope) scopes.Peek().innerScopes += 1;
      var data = dataPool.Borrow();
      data.scope = hasParentScope ? $"{scopes.Peek().scope}.{name}" : name;
      data.startTime = Time.realtimeSinceStartup;
      data.iterations = data.innerScopes = 0;
      scopes.Push(data);
    }

    public void scopeIteration() {
      checkForScope();
      scopes.Peek().iterations += 1;
    }

    public void closeScope() {
      checkForScope();
      var data = scopes.Pop();
      onData(new TimingData(
        data.scope, data.startTime, Time.realtimeSinceStartup, data.iterations, data.innerScopes
      ));
      dataPool.Release(data);
    }

    void checkForScope() {
      if (scopes.Count == 0) throw new IllegalStateException(
        "Timing does not have any scopes in it!"
      );
    }
  }
}
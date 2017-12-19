using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using Smooth.Pools;
using UnityEngine;
using UnityEngine.Profiling;

namespace com.tinylabproductions.TLPLib.Utilities {
  /// <summary> 
  ///   <para>
  ///    Tracks how much time has passed from the moment scope is opened <see cref="openScope"/> until it is closed <see cref="closeScope"/>,
  ///    counts how many iterations occured inside scope. Tracking also supports nesting (scope inside a scope).
  ///   </para>
  /// Can be used to analyze how long it takes for specific parts of code to be executed.
  /// </summary>
  /// <example>
  /// <code>
  /// <![CDATA[
  ///   ITiming timing = new Timing(data => Log.d("Elapsed time: ", data.durationStr));
  ///   timing.openScope("add one");
  ///   var numbers = Enumerable.Range(0, 100);
  ///   var result = numbers.Select(number => {
  ///     timing.scopeIteration();
  ///     return number++;
  ///   });
  ///   timing.closeScope();
  /// ]]>
  /// </code>
  /// </example>
  public interface ITiming {
    FrameTimingScope frameScope(string name);
    void openScope(string name);
    void scopeIteration();
    void closeScope();
  }

  public struct FrameTimingScope : IDisposable {
    readonly ITiming timing;
//    readonly ProfiledScope profiledScope;

    public FrameTimingScope(string name, ITiming timing) {
      this.timing = timing;
//      profiledScope = new ProfiledScope(name);
      timing.openScope(name);
    }

    public void Dispose() {
      timing.closeScope();
//      profiledScope.Dispose();
    }
  } 

  public struct TimingData {
    public readonly string scope;
    public readonly DateTime startTime, endTime;
    public readonly uint iterations;
    public readonly Duration duration;
    public readonly ImmutableArray<KeyValuePair<string, Duration>> childDurations;

    public TimingData(
      string scope, DateTime startTime, DateTime endTime, uint iterations, 
      ImmutableArray<KeyValuePair<string, Duration>> childDurations
    ) {
      this.scope = scope;
      this.startTime = startTime;
      this.endTime = endTime;
      this.iterations = iterations;
      this.childDurations = childDurations;
      duration = (endTime - startTime).toDuration();
    }

    public string durationStr { get {
      var durationS = new StringBuilder($"{duration.millis}ms");
      if (iterations != 0) {
        var avg = Duration.fromSeconds(duration.seconds / iterations);
        durationS.Append($", iters={iterations}, avg iter={avg.millis}ms");
      }
      if (childDurations.Length != 0) {
        durationS.Append($", iscopes=[\n");
        foreach (var kv in childDurations) {
          var percentage = (float) kv.Value.millis / duration.millis * 100;
          durationS.AppendLine($"{kv.Key} = {kv.Value.millis}ms ({percentage:F}%)");
        }
        durationS.Append("]");
      }
      return durationS.ToString();
    } }

    public override string ToString() => $"{nameof(TimingData)}[{scope}, {durationStr}]";
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
      new TimingConditional(backing, (log ?? Log.@default).willLog(level));
  }

  public class TimingNoOp : ITiming {
    public static readonly ITiming instance = new TimingNoOp();
    public static readonly FrameTimingScope noOpScope;

    static TimingNoOp() {
      noOpScope = new FrameTimingScope(null, instance);
    }
    
    TimingNoOp() {}

    public FrameTimingScope frameScope(string name) => noOpScope;
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

    public FrameTimingScope frameScope(string name) => shouldRun ? backing.frameScope(name) : TimingNoOp.noOpScope;
    public void openScope(string name) { if (shouldRun) backing.openScope(name); }
    public void scopeIteration() { if (shouldRun) backing.scopeIteration(); }
    public void closeScope() { if (shouldRun) backing.closeScope(); }
  }

  public class Timing : ITiming {
    class Data {
      public string name, fullScopeName;
      public DateTime startTime;
      public uint iterations;
      public readonly Dictionary<string, Duration> innerScopeDurations = 
        new Dictionary<string, Duration>();
      public Option<Data> parent = Option<Data>.None;
    }

    static readonly Pool<Data> dataPool = new Pool<Data>(() => new Data(), _ => { });
    readonly Stack<Data> scopes = new Stack<Data>();
    readonly Act<TimingData> onData;

    public Timing(Act<TimingData> onData) { this.onData = onData; }

    public FrameTimingScope frameScope(string name) => new FrameTimingScope(name, this);

    public void openScope(string name) {
      var hasParentScope = scopes.Count != 0;
      var parent = hasParentScope ? scopes.Peek().some() : Option<Data>.None;
      var data = dataPool.Borrow();
      data.name = name;
      data.fullScopeName = hasParentScope ? $"{scopes.Peek().fullScopeName}.{name}" : name;
      data.startTime = DateTime.Now;
      data.iterations = 0;
      data.innerScopeDurations.Clear();
      data.parent = parent;
      scopes.Push(data);
    }

    public void scopeIteration() {
      checkForScope();
      scopes.Peek().iterations += 1;
    }

    public void closeScope() {
      checkForScope();
      var data = scopes.Pop();
      var timingData = new TimingData(
        data.fullScopeName, data.startTime, DateTime.Now, data.iterations, 
        data.innerScopeDurations.ToImmutableArray()
      );
      foreach (var parent in data.parent) {
        parent.innerScopeDurations[data.name] = timingData.duration;
      }
      onData(timingData);
      dataPool.Release(data);
    }

    void checkForScope() {
      if (scopes.Count == 0) throw new IllegalStateException(
        "Timing does not have any scopes in it!"
      );
    }
  }
}
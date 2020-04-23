using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Logger {
  /// <summary>
  /// Upon creation starts buffering all log events that match <see cref="bufferingLevel"/>. It also forwards all
  /// log events to <see cref="backing"/> logger.
  ///
  /// Once you have a sink (where to put the buffered log entries) you can connect that using <see cref="set_sink"/>.
  ///
  /// Each log entry will be passed to the <see cref="Sink"/>. The order is not guaranteed.
  /// </summary>
  [PublicAPI] public partial class BufferingAndForwardingLog : ILog {
    readonly ILog backing, debugLog;
    readonly List<BufferEntry> buffer = new List<BufferEntry>();
    readonly IDisposableTracker tracker = new DisposableTracker();

    public Log.Level bufferingLevel;
    public bool disabled { get; private set; }
    
    Option<SinkRuntimeData> _sinkData = None._;

    public BufferingAndForwardingLog(ILog backing, Log.Level bufferingLevel) {
      this.backing = backing;
      debugLog = backing.withScope(nameof(BufferingAndForwardingLog));
      this.bufferingLevel = bufferingLevel;
      // Gather logs from Debug.Log and friends as well.
      UnityLog.fromUnityLogMessages.strict.subscribe(tracker, logEvent => bufferLog(logEvent.level, logEvent.entry));
    }

    public void disable() {
      disabled = true;
      buffer.Clear();
      setSink(None._);
      tracker.Dispose();
    }

    public Log.Level level { get => backing.level; set => backing.level = value; }
    public bool willLog(Log.Level l) => l >= bufferingLevel || backing.willLog(l);

    public void log(Log.Level l, LogEntry o) {
      bufferLog(l, o);
      if (backing.willLog(l)) backing.log(l, o);
    }

    /// <summary>
    /// Removes all messages from buffer which do not satisfy at least given level. 
    /// </summary>
    public void removeEntriesFromBuffer(Log.Level requiredLevel) {
      buffer.removeWhere(tpl => tpl.level < requiredLevel, replaceRemovedElementWithLast: true);
    }

    public void setSinkAndStartDispatching(SinkData sinkData) => setSink(Some.a(sinkData));
    
    public IRxObservable<LogEvent> messageLogged => backing.messageLogged;
    
    void setSink(Option<SinkData> maybeSink) {
      {
        if (_sinkData.valueOut(out var previousSinkData)) 
          previousSinkData.cleanup();
      }
      _sinkData = maybeSink.map(sinkData => {
        SinkRuntimeData runtimeData = null;
        var coroutine = ASync.EveryXSeconds(sinkData.deliveryInterval.seconds, () => {
          // ReSharper disable once AccessToModifiedClosure
          tryToDispatch(runtimeData);
          return true;
        });
        runtimeData = new SinkRuntimeData(sinkData, coroutine);
        return runtimeData;
      });
    }

    void tryToDispatch(SinkRuntimeData sinkRuntimeData) {
      {
        if (sinkRuntimeData.currentRequest.valueOut(out var currentRequest) && !currentRequest.isCompleted) {
          debugLog.mDebug(nameof(tryToDispatch) + " returning because current request is not yet completed");
          return;
        }
      }
      
      // We have the sink and current request is completed.
      debugLog.mDebug(nameof(tryToDispatch));
      
      // Copy the entries. 
      var entries = buffer.ToArray();
      // Clear current buffer
      buffer.Clear();
      // Dispatch the request
      var dispatchFuture = sinkRuntimeData.sinkData.sink(entries);
      sinkRuntimeData.currentRequest = Some.a(dispatchFuture);
      
      dispatchFuture.onComplete(success => {
        debugLog.mDebug($"{nameof(tryToDispatch)}: dispatch future has finished, {success.echo()}");
        // If data dispatch failed add it back to the buffer.
        if (!success) {
          buffer.AddRange(entries);
        }
      });
    }
    
    void bufferLog(Log.Level l, LogEntry o) {
      if (l < bufferingLevel) return;
      buffer.Add(new BufferEntry(DateTime.UtcNow, l, o));
    }
    
    /// <returns>
    /// Future that completes with whether consumption of an log entry was successful.
    ///
    /// If it was the entries will be forgotten. If it was not the entries will be rescheduled (in
    /// for later dispatch.
    /// </returns>
    public delegate Future<bool> Sink(IReadOnlyCollection<BufferEntry> bufferEntries);
    
    [Record] public sealed partial class SinkData {
      public readonly Sink sink;
      public readonly Duration deliveryInterval;
    }

    [Record] sealed partial class SinkRuntimeData {
      public readonly SinkData sinkData;
      public readonly Coroutine coroutine;

      public Option<Future<bool>> currentRequest = None._;

      public void cleanup() => coroutine.stop();
    }

    [Record] public readonly partial struct BufferEntry {
      public readonly DateTime timestamp;
      public readonly Log.Level level;
      public readonly LogEntry entry;
    }
  }
}
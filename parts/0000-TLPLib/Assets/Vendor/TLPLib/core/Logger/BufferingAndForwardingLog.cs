using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Data;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.dispose;
using pzd.lib.dispose;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Logger {
  /// <summary>
  /// Upon creation starts buffering all log events that match <see cref="bufferingLevel"/>. It also forwards all
  /// log events to <see cref="backing"/> logger.
  ///
  /// Once you have a sink (where to put the buffered log entries) you can connect that using
  /// <see cref="setSinkAndStartDispatching"/>.
  ///
  /// Each log entry will be passed to the <see cref="Sink"/>. The order is not guaranteed.
  /// </summary>
  [PublicAPI] public partial class BufferingAndForwardingLog : ILog {
    readonly ILog backing, debugLog;
    readonly List<BufferEntry> buffer = new List<BufferEntry>();
    readonly IDisposableTracker tracker = new DisposableTracker();
    readonly Option<LogLevel> generateBacktraceIfMissingFor;

    public LogLevel bufferingLevel;
    public bool disabled { get; private set; }
    
    Option<SinkRuntimeData> _sinkData = None._;
    uint sequenceNo;

    public BufferingAndForwardingLog(
      ILog backing, LogLevel bufferingLevel, Option<LogLevel> generateBacktraceIfMissingFor
    ) {
      this.backing = backing;
      debugLog = backing.withScope(nameof(BufferingAndForwardingLog));
      this.bufferingLevel = bufferingLevel;
      this.generateBacktraceIfMissingFor = generateBacktraceIfMissingFor;
      // Gather logs from Debug.Log and friends as well.
      UnityLog.fromUnityLogMessages.strict.subscribe(tracker, logEvent => bufferLog(logEvent.level, logEvent.entry));
    }

    public void disable() {
      disabled = true;
      lock (buffer) {
        buffer.Clear();
      }
      setSink(None._);
      tracker.Dispose();
    }

    public LogLevel level { get => backing.level; set => backing.level = value; }
    public bool willLog(LogLevel l) => l >= bufferingLevel || backing.willLog(l);

    public void log(LogLevel l, LogEntry o) {
      bufferLog(l, o);
      if (backing.willLog(l)) backing.log(l, o);
    }

    /// <summary>
    /// Removes all messages from buffer which do not satisfy at least given level. 
    /// </summary>
    public void removeEntriesFromBuffer(LogLevel requiredLevel) {
      lock (buffer) {
        buffer.removeWhere(tpl => tpl.level < requiredLevel, replaceRemovedElementWithLast: true);
      }
    }

    public void setSinkAndStartDispatching(SinkData sinkData) => setSink(Some.a(sinkData));
    
    public IRxObservable<LogEvent> messageLogged => backing.messageLogged;
    
    void setSink(Option<SinkData> maybeSink) {
      {
        if (_sinkData.valueOut(out var previousSinkData)) {
          debugLog.mDebug("Cleaning up previous sink");
          previousSinkData.cleanup();
        }
      }
      _sinkData = maybeSink.map(sinkData => {
        debugLog.mDebug($"Launching new sink: {sinkData}");
        SinkRuntimeData runtimeData = null;
        var coroutine = ASync.EveryXSeconds(sinkData.deliveryInterval.seconds, () => {
          // ReSharper disable once AccessToModifiedClosure
          var data = runtimeData;
          // First invocation has this null.
          if (data != null) tryToDispatch(data);
          return true;
        });
        runtimeData = new SinkRuntimeData(sinkData, coroutine);
        return runtimeData;
      });
    }

    void tryToDispatch(SinkRuntimeData sinkRuntimeData) {
      try {
        debugLog.mVerbose($"{nameof(tryToDispatch)}: entering");

        {
          if (sinkRuntimeData.currentRequest.valueOut(out var currentRequest) && !currentRequest.isCompleted) {
            debugLog.mVerbose(nameof(tryToDispatch) + " returning because current request is not yet completed");
            return;
          }
        }
        BufferEntry[] entriesToSend;
        lock (buffer) {
          if (buffer.isEmpty()) {
            debugLog.mVerbose(nameof(tryToDispatch) + " returning because we have no entries");
            return;
          }

          // Ensure at least 1 element gets removed.
          var batchSize = Math.Min(Math.Max(1, sinkRuntimeData.sinkData.maxBatchSize), buffer.Count);
          entriesToSend = buffer.dropFromHead(batchSize.toIntClamped());
        }

        // Copy the entries. 
        var entries = entriesToSend.asReadOnlyCollection().toNonEmpty().getOrThrow("developer error!");

        // We have the sink and current request is completed.
        debugLog.mDebug($"{nameof(tryToDispatch)} dispatching {entries.a.Count} entries");

        // Dispatch the request
        var dispatchFuture = sinkRuntimeData.sinkData.sink(entries);
        sinkRuntimeData.currentRequest = Some.a(dispatchFuture);

        dispatchFuture.onComplete(success => {
          debugLog.mDebug($"{nameof(tryToDispatch)}: dispatch future has finished, {success.echo()}");
          // If data dispatch failed add it back to the buffer.
          if (!success) {
            lock (buffer) {
              buffer.AddRange(entries.a);
            }
          }
        });
      }
      catch (Exception e) {
        debugLog.error($"Error in {nameof(tryToDispatch)}", e);
      }
    }
    
    void bufferLog(LogLevel l, LogEntry o) {
      if (l < bufferingLevel) return;

      {
        if (
          o.backtrace == null
          && generateBacktraceIfMissingFor.valueOut(out var genBacktraceLevel)
          && l >= genBacktraceLevel
          && Backtrace.generateFromHere().valueOut(out var backtrace)
        ) {
          o = o.withBacktrace(backtrace);
        }
      }

      lock (buffer) {
        buffer.Add(new BufferEntry(DateTime.UtcNow, l, o, sequenceNo));
        sequenceNo++;
      }
    }
    
    /// <returns>
    /// Future that completes with whether consumption of an log entry was successful.
    ///
    /// If it was the entries will be forgotten. If it was not the entries will be rescheduled (in
    /// for later dispatch.
    /// </returns>
    public delegate Future<bool> Sink(NonEmpty<IReadOnlyCollection<BufferEntry>> bufferEntries);
    
    [Record] public sealed partial class SinkData {
      public readonly Sink sink;
      public readonly Duration deliveryInterval;
      public readonly uint maxBatchSize;
    }

    [Record] sealed partial class SinkRuntimeData {
      public readonly SinkData sinkData;
      public readonly ICoroutine coroutine;

      public Option<Future<bool>> currentRequest = None._;

      public void cleanup() => coroutine.stop();
    }

    [Record] public readonly partial struct BufferEntry {
      public readonly DateTime timestamp;
      public readonly LogLevel level;
      public readonly LogEntry entry;
      public readonly uint sequenceNo;
    }
  }
}
#if UNITY_TEST
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using NUnit.Framework;
using UnityEngine;


namespace Assets.Vendor.TLPLib.Concurrent {
  [TestFixture]
  public class FutureTest {
    struct LogEntry {
      public readonly string message, stacktrace;
      public readonly LogType type;

      public LogEntry(string message, string stacktrace, LogType type) {
        this.message = message;
        this.stacktrace = stacktrace;
        this.type = type;
      }

      public override string ToString() {
        return string.Format("message: {0}, type: {1}", message, type);
      }
    }

    static ReadOnlyCollection<LogEntry> withLogs(Act f) {
      var entries = new List<LogEntry>();
      Application.LogCallback logger = (message, stacktrace, type) => {
        entries.Add(new LogEntry(message, stacktrace, type));
      };
      Application.logMessageReceived += logger;
      try { f(); }
      finally { Application.logMessageReceived -= logger; }
      return entries.AsReadOnly();
    }

    static void assertErrors(ReadOnlyCollection<LogEntry> entries, int count) {
      Assert.AreEqual(count, entries.Count);
      Assert.True(entries.All(e => e.type == LogType.Exception), "all entries must be exceptions: " + entries.asString());
    }

    static void failedFuture(Act<Future<int>> registerCallbacks) {
      Act failFuture = null;
      var f = Future.a<int>(p => failFuture = () => p.completeError(new Exception()));
      registerCallbacks(f);
      failFuture();
    }

    static void failedMappedFuture(Act<Future<int>> registerCallbacks) {
      failedFuture(f => registerCallbacks(f.map(_ => _ * 2)));
    }

    static void failedMapped2Future(Act<Future<int>, Future<int>> registerCallbacks) {
      failedFuture(f => registerCallbacks(f.map(_ => _ * 2), f.map(_ => _ * 3)));
    }

    [Test]
    public void testFutureWithNoHandlesLogsErrors() {
      var entries = withLogs(() => failedFuture(f => {}));
      assertErrors(entries, 1);
    }

    [Test]
    public void testFutureWithOnSuccessLogsErrors() {
      var entries = withLogs(() => failedFuture(f => f.onSuccess(_ => {})));
      assertErrors(entries, 1);
    }

    [Test]
    public void testFutureWithOnFailureDoesNotLogErrors() {
      var entries = withLogs(() => failedFuture(f => f.onFailure(_ => {})));
      assertErrors(entries, 0);
    }

    [Test]
    public void testMappedFutureWithNoHandlesLogsErrors() {
      var entries = withLogs(() => failedMappedFuture(f => {}));
      assertErrors(entries, 1);
    }

    [Test]
    public void testMappedFutureWithOnSuccessLogsErrors() {
      var entries = withLogs(() => failedMappedFuture(f => f.onSuccess(_ => {})));
      assertErrors(entries, 1);
    }

    [Test]
    public void testMappedFutureWithOnFailureDoesNotLogErrors() {
      var entries = withLogs(() => failedMappedFuture(f => f.onFailure(_ => {})));
      assertErrors(entries, 0);
    }

    [Test]
    public void testMappedFutureWithOnCompleteDoesNotLogErrors() {
      var entries = withLogs(() => failedMappedFuture(f => f.onComplete(_ => {})));
      assertErrors(entries, 0);
    }

    [Test]
    public void testMapped2FutureWithNoHandlesLogsErrors() {
      var entries = withLogs(() => failedMapped2Future((f1, f2) => {}));
      assertErrors(entries, 2);
    }

    [Test]
    public void testMapped2FutureWithOnSuccessLogsErrors() {
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => {
          f1.onSuccess(_ => {});
          f2.onSuccess(_ => {});
        })),
        2
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f1.onSuccess(_ => {}))),
        2
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f2.onSuccess(_ => {}))),
        2
      );
    }

    [Test]
    public void testMapped2FutureWithOnFailureDoesNotLogErrors() {
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => {
          f1.onFailure(_ => {});
          f2.onFailure(_ => {});
        })),
        0
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f1.onFailure(_ => {}))),
        1
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f2.onFailure(_ => {}))),
        1
      );
    }

    [Test]
    public void testMapped2FutureWithOnCompleteDoesNotLogErrors() {
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => {
          f1.onComplete(_ => {});
          f2.onComplete(_ => {});
        })),
        0
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f1.onComplete(_ => {}))),
        1
      );
      assertErrors(
        withLogs(() => failedMapped2Future((f1, f2) => f2.onComplete(_ => {}))),
        1
      );
    }
  }
}

#endif

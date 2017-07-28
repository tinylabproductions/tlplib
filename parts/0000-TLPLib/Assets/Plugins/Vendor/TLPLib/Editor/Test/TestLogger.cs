using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Test {
  public class TestLogger : LogBase {
    public struct Entry : IEquatable<Entry> {
      public readonly string message;
      public readonly Option<Object> context;

      #region Equality

      public bool Equals(Entry other) {
        return string.Equals(message, other.message) && context.Equals(other.context);
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Entry && Equals((Entry) obj);
      }

      public override int GetHashCode() {
        unchecked { return ((message != null ? message.GetHashCode() : 0) * 397) ^ context.GetHashCode(); }
      }

      public static bool operator ==(Entry left, Entry right) { return left.Equals(right); }
      public static bool operator !=(Entry left, Entry right) { return !left.Equals(right); }

      #endregion

      public Entry(string message, Option<Object> context) {
        this.message = message;
        this.context = context;
      }

      public override string ToString() => 
        $"{nameof(Entry)}[" +
        $"{nameof(message)}: {message}, " +
        $"{nameof(context)}: {context}" +
        $"]";
    }

    public readonly List<Entry> 
      verboseMsgs = new List<Entry>(), 
      debugMsgs = new List<Entry>(), 
      infoMsgs = new List<Entry>(),
      warnMsgs = new List<Entry>(),
      errorMsgs = new List<Entry>();

    public int count => 
      verboseMsgs.Count + debugMsgs.Count + infoMsgs.Count + warnMsgs.Count + errorMsgs.Count;

    public bool isEmpty => count == 0;
    public bool nonEmpty => !isEmpty;

    public bool errorsAsExceptions;

    public TestLogger(bool errorsAsExceptions = false) {
      this.errorsAsExceptions = errorsAsExceptions;
      level = Log.Level.VERBOSE;
    }

    public void clear() {
      verboseMsgs.Clear();
      debugMsgs.Clear();
      infoMsgs.Clear();
      warnMsgs.Clear();
      errorMsgs.Clear();
    }

    protected override void logInner(Log.Level l, string s, Option<Object> context) {
      var entry = new Entry(s, context);
      switch (l) {
        case Log.Level.NONE:
          break;
        case Log.Level.ERROR:
          if (errorsAsExceptions) throw new Exception(entry.ToString());
          errorMsgs.Add(entry);
          break;
        case Log.Level.WARN:
          warnMsgs.Add(entry);
          break;
        case Log.Level.INFO:
          infoMsgs.Add(entry);
          break;
        case Log.Level.DEBUG:
          debugMsgs.Add(entry);
          break;
        case Log.Level.VERBOSE:
          verboseMsgs.Add(entry);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }
  }
}

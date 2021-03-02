using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.concurrent;
using pzd.lib.functional;
using pzd.lib.log;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class ObservableU {
    static IRxObservable<Unit> everyFrameInstance;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void reset() {
      everyFrameInstance = null;
      Observable.onError = err => Log.d.error(err);
      Observable.onException = (err, exception) => Log.d.error(err, exception);
    }

    public static IRxObservable<Unit> everyFrame =>
      everyFrameInstance ??= new Observable<Unit>(observer => {
        var cr = ASync.StartCoroutine(everyFrameCR(observer));
        return new Subscription(cr.stop);
      });

    #region touches

    public readonly struct Touch {
      public readonly int fingerId;
      public readonly Vector2 position, previousPosition;
      public readonly int tapCount;
      public readonly TouchPhase phase;

      public Touch(int fingerId, Vector2 position, Vector2 previousPosition, int tapCount, TouchPhase phase) {
        this.fingerId = fingerId;
        this.position = position;
        this.previousPosition=previousPosition;
        this.tapCount = tapCount;
        this.phase = phase;
      }
    }

    static IRxObservable<List<Touch>> touchesInstance;

    public static IRxObservable<List<Touch>> touches =>
      touchesInstance ??= createTouchesInstance();

    static IRxObservable<List<Touch>> createTouchesInstance() {
      var touchList = new List<Touch>();
      var previousMousePos = new Vector2();
      var previousMousePhase = TouchPhase.Ended;
      var prevPositions = new Dictionary<int, Vector2>();
      return everyFrame.map(_ => {
        touchList.Clear();
        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) {
          var curPos = (Vector2) Input.mousePosition;
          var curPhase = Input.GetMouseButtonDown(0)
            ? TouchPhase.Began
            : Input.GetMouseButtonUp(0)
              ? TouchPhase.Ended
              : curPos == previousMousePos ? TouchPhase.Moved : TouchPhase.Stationary;
          if (previousMousePhase == TouchPhase.Ended) previousMousePos = curPos;
          touchList.Add(new Touch(-100, curPos, previousMousePos, 0, curPhase));
          previousMousePos = curPos;
          previousMousePhase = curPhase;
        }
        for (var i = 0; i < Input.touchCount; i++) {
          var t = Input.GetTouch(i);
          var id = t.fingerId;
          var previousPos = t.position;
          if (t.phase != TouchPhase.Began) {
            if (!prevPositions.TryGetValue(id, out previousPos)) {
              previousPos = t.position;
            }
            prevPositions[id] = t.position;
          }
          if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended) {
            prevPositions.Remove(id);
          }
          touchList.Add(new Touch(t.fingerId, t.position, previousPos, t.tapCount, t.phase));
        }
        return touchList;
      });
    }

    #endregion

    public static IRxObservable<DateTime> interval(Duration interval, Duration delay) =>
      ObservableU.interval(interval, Some.a(delay));

    public static IRxObservable<DateTime> interval(
      Duration interval, Option<Duration> delay=default
    ) {
      Option.ensureValue(ref delay);
      return new Observable<DateTime>(observer => {
        var cr = ASync.StartCoroutine(intervalEnum(observer, interval, delay));
        return new Subscription(cr.stop);
      });
    }

    static IEnumerator everyFrameCR(Action<Unit> onEvent) {
      while (true) {
        onEvent(Unit._);
        yield return null;
      }
      // ReSharper disable once IteratorNeverReturns
    }

    static IEnumerator intervalEnum(
      Action<DateTime> pushEvent, Duration interval, Option<Duration> delay
    ) {
      foreach (var d in delay) yield return new WaitForSeconds(d.seconds);
      var wait = new WaitForSeconds(interval.seconds);
      while (true) {
        pushEvent(DateTime.Now);
        yield return wait;
      }
      // ReSharper disable once IteratorNeverReturns
    }
  }
}
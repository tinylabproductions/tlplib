using System;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenTestBed : MonoBehaviour, IMB_Start, IMB_Update {
    public Transform obj1, obj2, obj3, obj4;
    public float duration = 5;

    TweenSequence tr;

    public void Start() {
//      var obj3T = TweenLerp.vector3.tween(
//        obj3.position, obj3.position + Vector3.right * 10, Eases.linear, duration
//      );
//      var obj23T = obj2T
//      var obj4T = TweenLerp.vector3.tween(
//        obj4.position, obj4.position + Vector3.right * 10, Eases.quadratic, duration
//      );
//      var obj4T2 = TweenLerp.vector3.tween(
//        obj4T.end, obj4T.end + Vector3.right * 10, Eases.quadratic, duration
//      );
//
//      // 
//
      var t1 = obj1.tweenPositionRelative(Vector3.right * 2, Eases.linear, duration);
      tr = TweenSequence.sequential(
        Tween.callback(() => print("start")),
        t1,
        Tween.callback(() => print("1")),
        obj2.tweenPositionRelative(Vector3.right * 2, Eases.linear, duration),
        Tween.callback(() => print("2")),
        t1.tweenPositionRelative(Vector3.right * 2, Eases.linear, duration),
        Tween.callback(() => print("end"))
      );

//      var tr1 =
//        TweenSequence.Builder.create()
//          .append(obj1.tweenPositionRelative(Vector3.right * 10, Eases.cubic, duration))
//          .append(obj2.tweenPositionRelative(Vector3.right * 10, Eases.sin, duration))
//          .build();
//
//      tr =
//        TweenSequence.Builder.create()
//          .append(tr1)
//          .insert(1, obj3.tweenPositionRelative(Vector3.right * 10, Eases.linear, duration))
//          .append(obj4.tweenPositionRelative(Vector3.right * 10, Eases.cubic, duration))
//          .append(obj4.tweenPosition(Vector3.left * 10, Eases.quadratic, duration))
//          .build();
//
//      ASync.WithDelay(tr.totalDuration, tr.reset);
//      ASync.WithDelay(tr.totalDuration * 2, tr.reset);
    }

    public void Update() {
      tr.update(Time.deltaTime);
    }
  }
}
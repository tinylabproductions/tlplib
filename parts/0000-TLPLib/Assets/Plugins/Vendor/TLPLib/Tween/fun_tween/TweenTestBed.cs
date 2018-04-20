using System;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class TweenTestBed : MonoBehaviour, IMB_Start, IMB_Update {
    public SpriteRenderer indicator;
    public Transform obj1, obj2, obj3, obj4;
    public float duration = 5;

    TweenManager manager;

    public void Start() {
//      var obj3T = TweenOps.vector3.tween(
//        obj3.position, obj3.position + Vector3.right * 10, Eases.linear, duration
//      );
//      var obj23T = obj2T
//      var obj4T = TweenOps.vector3.tween(
//        obj4.position, obj4.position + Vector3.right * 10, Eases.quadratic, duration
//      );
//      var obj4T2 = TweenOps.vector3.tween(
//        obj4T.end, obj4T.end + Vector3.right * 10, Eases.quadratic, duration
//      );
//
//      //
//
      var t1 = obj1.tweenPositionRelative(Vector3.right * 5, Eases.linear, duration); 
      var t2 = obj1.tweenPositionRelative(Vector3.up, Eases.expoOut, duration);
      var t3 = obj1.tweenPositionRelative(Vector3.down, Eases.quadInOut, duration);
      var t4 = obj1.tweenPositionRelative(Vector3.left, Eases.elasticInOut, duration);
      var tweens = new TweenTimelineElement[] {t1/*, t2, t3, t4*/};

      var tra = TweenTimeline.parallelEnumerable(tweens/*.shuffleRepeatedly(Rng.now).Take(100)*/).build();
//        Tween.callback(_ => print($"start {_}")),
//        Tween.callback(_ => print($"1 {_}")),
//        obj2.tweenPositionRelative(Vector3.right * 2, Eases.linear, duration),
//        obj2.tweenPositionRelative(Vector3.right * 2, Eases.linear, 0f),
//        Tween.callback(_ => print($"2 {_}")),
//        t1.tweenPositionRelative(Vector3.right * 2, Eases.linear, duration),
//        Tween.callback(_ => print($"end {_}"))
      manager = tra.managed();
      var tr = TweenTimeline.sequential(
        /*tra, tra.reversed(), tra.reversed().reversed(), *//*tra.reversed()*/
      ).build();
//      manager = tr.managed()
//        .onStart(forwards => indicator.color = forwards ? Color.black : Color.green)
//        .onEnd(forwards => indicator.color = forwards ? Color.gray : Color.red);

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
      if (Input.GetKeyDown(KeyCode.P)) manager.play();
      if (Input.GetKeyDown(KeyCode.LeftBracket)) manager.play(forwards: true);
      if (Input.GetKeyDown(KeyCode.RightBracket)) manager.play(forwards: false);
      if (Input.GetKeyDown(KeyCode.R)) manager.rewind();
      if (Input.GetKeyDown(KeyCode.F)) manager.rewind(applyEffectsForRelativeTweens: true);
      if (Input.GetKeyDown(KeyCode.T)) manager.resume();
      if (Input.GetKeyDown(KeyCode.Y)) manager.resume(true);
      if (Input.GetKeyDown(KeyCode.U)) manager.resume(false);
      if (Input.GetKeyDown(KeyCode.S)) manager.stop();
      if (Input.GetKeyDown(KeyCode.G)) manager.reverse();
      if (Input.GetKeyDown(KeyCode.Alpha1)) manager.timescale = -2f;
      if (Input.GetKeyDown(KeyCode.Alpha2)) manager.timescale = -1.5f;
      if (Input.GetKeyDown(KeyCode.Alpha3)) manager.timescale = -1f;
      if (Input.GetKeyDown(KeyCode.Alpha4)) manager.timescale = -0.5f;
      if (Input.GetKeyDown(KeyCode.Alpha5)) manager.timescale = 0f;
      if (Input.GetKeyDown(KeyCode.Alpha6)) manager.timescale = 0.5f;
      if (Input.GetKeyDown(KeyCode.Alpha7)) manager.timescale = 1f;
      if (Input.GetKeyDown(KeyCode.Alpha8)) manager.timescale = 1.5f;
      if (Input.GetKeyDown(KeyCode.Alpha9)) manager.timescale = 2f;
    }
  }
}
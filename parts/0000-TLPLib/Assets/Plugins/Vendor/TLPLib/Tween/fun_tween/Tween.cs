namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public class Tween<A> {
    public readonly A start, end;
    public readonly Ease ease;
    public readonly TweenLerp<A> lerp;
    public readonly float duration;

    public Tween(A start, A end, Ease ease, TweenLerp<A> lerp, float duration) {
      this.start = start;
      this.end = end;
      this.ease = ease;
      this.lerp = lerp;
      this.duration = duration;
    }

    public A eval(float timePassed) => lerp(start, end, ease(timePassed / duration));
  }
}
using System;
using com.tinylabproductions.TLPLib.Functional;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public interface Ref<A> : Val<A> {
    new A value { get; set; }
  }

  /* Simple heap-allocated reference. */
  public sealed class SimpleRef<A> : Ref<A> {
    // For access using ref keyword.
    public A value;

    A Val<A>.value => value;

    A Ref<A>.value {
      get { return value;}
      set { this.value = value; }
    }

    public SimpleRef(A value) { this.value = value; }

    public static implicit operator A(SimpleRef<A> r) => r.value;
    public override string ToString() => $"{nameof(SimpleRef<A>)}({value})";
  }

  public class LambdaRef<A> : Ref<A> {
    readonly Fn<A> get;
    readonly Act<A> set;

    public LambdaRef(Fn<A> get, Act<A> set) {
      this.get = get;
      this.set = set;
    }

    public A value {
      get { return get(); }
      set { set(value); }
    }

    public override string ToString() => $"λRef({value})";
  }

  /// <summary>
  /// Lazy reference that wraps another <see cref="Ref{A}"/>, but does not initialize it
  /// until first access.
  ///
  /// Very useful in making <see cref="PrefVal{A}"/>s inspectable.
  ///
  /// For example:
  /// <code>
  /// [Inspect, UsedImplicitly]
  /// LazyRef&lt;string&gt; adbAdditions = Ref.lazy(() =&gt; prefVals.adbAdditions);
  /// </code>
  /// </summary>
  public class LazyRef<A> : Ref<A> {
    [HideInInspector]
    public readonly LazyVal<Ref<A>> backing;

    public LazyRef(LazyVal<Ref<A>> backing) { this.backing = backing; }

    [ShowInInspector]
    public A value {
      get => backing.strict.value;
      set => backing.strict.value = value;
    }

    public override string ToString() => backing.strict.ToString();
  }

  public static class Ref {
    public static Ref<A> a<A>(A value) => new SimpleRef<A>(value);
    public static Ref<A> a<A>(Fn<A> get, Act<A> set) => new LambdaRef<A>(get, set);
    public static LazyRef<A> lazy<A>(Fn<Ref<A>> backing) => lazy(F.lazy(backing));
    public static LazyRef<A> lazy<A>(LazyVal<Ref<A>> backing) => new LazyRef<A>(backing);

    public static Ref<B> map<A, B>(this Ref<A> r, Fn<A, B> map, Fn<B, A> contraMap) =>
      a(() => map(r.value), b => r.value = contraMap(b));

    public static Ref<B> map<A, B>(this Ref<A> r, Fn<A, B> map, Fn<B, Option<A>> contraMap) =>
      a(() => map(r.value), b => {
        foreach (var a in contraMap(b))
          r.value = a;
      });
  }

  /*[DrawerPriority(10, 0, 0)] // Giv
  public class LazyRefDrawer<L, A> : OdinValueDrawer<L>
    where L : LazyRef<A>
  {
    protected override bool CanDrawValueProperty(InspectorProperty property) {
      return base.CanDrawValueProperty(property);

    }



    /*protected override void DrawPropertyLayout(GUIContent label) {
      var entry = this.ValueEntry;
      entry.SmartValue.value;
      // Check all values for null, and if any are null, create an instance
      // Only do this in repaint; as a rule, only change reference type values in repaint
      Debug.LogError("TEST DRAWER");
//      if (Event.current.type == EventType.Repaint)
//        for (int i = 0; i < entry.ValueCount; i++)
//          if (entry.Values[i] == null)
//            entry.Values[i] = new L();

      // Call the next drawer in line, and let the lists be drawn normally
//      this.CallNextDrawer(label);
      base.DrawPropertyLayout(new GUIContent("TEST"));

    }#1#
/*    protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<LazyRef<T>> entry, GUIContent label) { base.DrawPropertyRect(position, entry, label); }

    protected override void DrawPropertyLayout(IPropertyValueEntry<LazyRef<T>> entry, GUIContent label) {
      // Check all values for null, and if any are null, create an instance
      // Only do this in repaint; as a rule, only change reference type values in repaint
      if (Event.current.type == EventType.Repaint)
        for (int i = 0; i < entry.ValueCount; i++)
          if (entry.Values[i] == null)
            entry.Values[i] = new T();

      // Call the next drawer in line, and let the lists be drawn normally
      this.CallNextDrawer(entry, label);
    }#1#
  }*/
}

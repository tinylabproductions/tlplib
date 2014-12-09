using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using com.tinylabproductions.TLPLib.Iter;
using System.Collections.Generic;
﻿using System.Linq;
﻿using System;
using System.Text.RegularExpressions;
﻿using com.tinylabproductions.TLPLib.Data;
﻿using com.tinylabproductions.TLPLib.Extensions;
﻿using com.tinylabproductions.TLPLib.Functional;
﻿using com.tinylabproductions.TLPLib.Reactive;
﻿using com.tinylabproductions.TLPLib.Tween;

namespace com.tinylabproductions.TLPLib.Binding {
  public static class UGUIBind {
    private const float TWEEN_DURATION = 0.5f;
    private const GoEaseType TWEEN_EASE = GoEaseType.SineOut;
    private static readonly Regex intFilter = new Regex(@"\D");

    public static readonly Fn<string, string> strMapper = _ => _;
    public static readonly Fn<int, string> intMapper = _ => _.ToString();
    public static readonly Fn<string, int> intComapper = text => {
      var filtered = intFilter.Replace(text, "");
      return filtered.Length == 0 ? 0 : int.Parse(filtered);
    };
    public static readonly Fn<uint, string> uintMapper = v => v.ToString();
    public static readonly Fn<string, uint> uintComapper = text => {
      var filtered = intFilter.Replace(text, "");
      return filtered.Length == 0 ? 0 : uint.Parse(filtered);
    };

    private static GoTweenConfig tCfg { get {
      return new GoTweenConfig().setEaseType(TWEEN_EASE);
    } }

    #region Misc

    public static ISubscription withTween(
      Fn<Act<GoTween>, ISubscription> body
    ) {
      var tween = F.none<GoTween>();
      return body(newT => {
        tween.each(t => t.destroy());
        tween = F.some(newT);
      }).andThen(() => tween.each(t => t.destroy()));
    }

    #endregion

    #region Observable constructors

    public static IObservable<Button> clicksObservable(this Button button) {
      return new Observable<Button>(observer => {
        UnityAction dlg = () => observer.push(button);
        button.onClick.AddListener(dlg);
        return new Subscription(() => button.onClick.RemoveListener(dlg));
      });
    }

    #endregion

    #region One-way binds

    public static ISubscription bind<A>(
      this RxList<A> list, int max, string maxName,
      Fn<int, IObservable<Option<A>>, ISubscription> bindObservable
    ) {
      var subscription = list.rxSize.subscribe(size => {
        if (size > max) throw new Exception(String.Format(
          "Max {0} {1} are supported in view, " +
          "but list size was exceeded.", max, maxName
        ));
      });
      var enumeration = Enumerable.Range(0, max).Select(i => {
        var observable = list.rxElement(i);
        return bindObservable(i, observable);
      }).ToList(); /** Eagerly evaluate to bind **/

      return new Subscription(() => {
        subscription.unsubscribe();
        foreach (var s in enumeration) s.unsubscribe();
      });
    }

    public static ISubscription bind<A, Control>(
      this RxList<A> list, int max, string maxName,
      Fn<int, Control> getControl, Act<Control, A> onChange
    ) where Control : Component {
      return list.bind(max, maxName, (i, observable) => {
        var control = getControl(i);
        return observable.subscribe(opt => {
          control.gameObject.SetActive(opt.isDefined);
          opt.each(v => onChange(control, v));
        });
      });
    }

    public static ISubscription bind(
      this IObservable<ValueWithStorage> subject, Slider control
    ) {
      return withTween(set => subject.subscribe(value => {
        control.minValue = 0;
        control.maxValue =
          // 0 out of 0 yields full progress bar which is not what we want.
          value.value == 0 && value.storage == 0 ? 1 
          : Math.Max(value.value, value.storage);
        set(Go.to(
          TF.a(() => control.value, v => control.value = v),
          TWEEN_DURATION, tCfg.floatProp(TF.Prop, value.value)
        ));
      }));
    }

    public static ISubscription bind(
      this IObservable<ValueWithStorage> subject, Text control
    ) {
      return withTween(set => subject.subscribe(value => set(Go.to(
        TF.a(
          () => ValueWithStorage.parse(control.text).AsVector2(),
          v => control.text = new ValueWithStorage(v).AsString()
        ), TWEEN_DURATION, tCfg.vector2Prop(TF.Prop, value.AsVector2())
      ))));
    }

    public static ISubscription bind(
      this IObservable<uint> subject, Text control
    ) {
      return withTween(set => subject.subscribe(value => {
        set(Go.to(
          TF.a(
            () => (int) uintComapper(control.text),
            v => control.text = v.ToString()
          ), TWEEN_DURATION, tCfg.intProp(TF.Prop, (int) value)
        ));
        control.text = value.ToString();
      }));
    }

    #endregion

    #region Two-way binds

    public static ISubscription bind<T>(
      this IRxRef<T> subject, IEnumerable<Toggle> checkboxes,
      Fn<T, string> mapper, Fn<string, T> comapper
    ) {
      var optSubject = RxRef.a(F.some(subject.value));
      var optSubjectSourceSubscription = subject.subscribe(v => 
        optSubject.value = F.some(v)
      );
      var optSubjectTargetSubscription = optSubject.subscribe(opt => 
        opt.each(v => subject.value = v)
      );

      var bindSubscription = optSubject.bind(checkboxes, mapper, comapper);
      return new Subscription(() => {
        optSubjectSourceSubscription.unsubscribe();
        optSubjectTargetSubscription.unsubscribe();
        bindSubscription.unsubscribe();
      });
    }

    public static ISubscription bind<T>(
      this IRxRef<Option<T>> subject, IEnumerable<Toggle> checkboxes,
      Fn<T, string> mapper, Fn<string, T> comapper
    ) {
      Act uncheckAll = () => {
        foreach (var cb in checkboxes) cb.isOn = false;
      };
      Act<Option<T>, string> check = (v, name) => 
        checkboxes.hIter().find(cb => cb.name == name).voidFold(
          () => {
            throw new Exception(String.Format(
              "Can't find checkbox with name {0} which was mapped from {1}",
              name, v
            ));
          },
          cb => cb.isOn = true
        );

      uncheckAll();
      subject.value.map(mapper).each(name => check(subject.value, name));

      var subscription = subject.subscribe(v => 
        v.map(mapper).voidFold(uncheckAll, name => check(v, name))
      );
      var withHandlers = checkboxes.hIter().map(cb => 
        F.t(cb, new UnityAction<bool>(selected => {
          if (selected) subject.value = F.some(comapper(cb.name));
        }))
      );

      withHandlers.each(t => t._1.onValueChanged.AddListener(t._2));

      return new Subscription(() => {
        subscription.unsubscribe();
        withHandlers.each(t => t._1.onValueChanged.RemoveListener(t._2));
      });
    }

    public static ISubscription bind(
      this IRxRef<string> subject, InputField control
    ) {
      return subject.bind(control, strMapper, strMapper);
    }

    public static ISubscription bind(
      this IRxRef<int> subject, InputField control
    ) {
      return subject.bind(control, intMapper, intComapper);
    }

    public static ISubscription bind(
      this IRxRef<uint> subject, InputField control
    ) {
      return subject.bind(control, uintMapper, uintComapper);
    }

    public static ISubscription bind<T>(
      this IRxRef<T> subject, InputField control,
      Fn<T, string> mapper, Fn<string, T> comapper
    ) {
      return subject.bind(
        mapper, comapper,
        text => control.textComponent.text = text,
        handler => control.onValueChange.AddListener(handler),
        handler => control.onValueChange.RemoveListener(handler)
      );
    }

    public static ISubscription bind<A>(
      this IRxRef<A> subject, 
      Fn<A, string> mapper, Fn<string, A> comapper,
      Act<string> changeControlText,
      Act<UnityAction<string>> subscribeToControlChanged,
      Act<UnityAction<string>> unsubscribeToControlChanged
    ) {
      var f = mapper.andThen(changeControlText);
      var subscription = subject.subscribe(f);
      UnityAction<string> handler = value => subject.value = comapper(value);
      subscribeToControlChanged(handler);
      return new Subscription(() => {
        unsubscribeToControlChanged(handler);
        subscription.unsubscribe();
      });
    }
  }

  #endregion
}

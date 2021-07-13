using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using pzd.lib.collection;
using pzd.lib.dispose;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;
using pzd.lib.reactive;
using TMPro;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.TextMeshPro.Binding {
  public static class TextMeshProBind {
    static readonly TMP_Dropdown.OptionData noPlaceholderOption =
      new TMP_Dropdown.OptionData("ERROR: None, but placeholder missing");

    public interface IDropdownSetToNoneBehaviour<in A> {
      void onNone(TMP_Dropdown dropdown, ILog log);
      void onValue(TMP_Dropdown dropdown, A value, ILog log);
    }
    
    public enum DropdownSetToNoneBehaviour {
      /// <summary>If None is selected the dropdown will be set to placeholder.</summary>
      UsePlaceholder,
      /// <summary>If None is selected the dropdown will be disabled.</summary>
      Disable
    }

    /// <summary>
    /// Binds a dropdown to a list of values.
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="tracker"></param>
    /// <param name="values">Possible values.</param>
    /// <param name="selected">Currently selected value.</param>
    /// <param name="setToNoneBehaviour">How should we act when <see cref="selected"/> becomes None?</param>
    /// <param name="log"></param>
    /// <returns>Event stream which fires an event when a value is changed by user.</returns>
    public static IRxObservable<A> bind<A>(
      this TMP_Dropdown dropdown, ITracker tracker, 
      ImmutableArrayC<(A, TMP_Dropdown.OptionData)> values, IRxVal<Option<A>> selected,
      Either<DropdownSetToNoneBehaviour, IDropdownSetToNoneBehaviour<A>> setToNoneBehaviour, [Implicit] ILog log=default
    ) {
      var subject = new Subject<A>();
      dropdown.setDropdownOptions(values._unsafeArray.map(_ => _.Item2));
      
      var items = values._unsafeArray.map(_ => _.Item1);
      // Make sure that when selected item changes we update the dropdown.
      selected.subscribe(tracker, setDropdownSelectedIndex);
      // Subscribe to dropdown value changes, which are initiated by user clicking around.
      dropdown.onValueChanged.toObservable().subscribe(tracker, index => {
        // When user clicks on a dropdown item, the dropdown automatically changes its selected item.
        //
        // We want dropdown to always show the value from the state RX, thus we need to manually set the index back
        // after the user click happens.
        setDropdownSelectedIndex(selected.value);
        
        if (items.get(index).valueOut(out var item)) {
          subject.push(item);
        }
        else {
          log.error($"This should never happen, can't find {index.echo()} in items: {items.mkStringEnum()}");
        }
      });
      tracker.track(dropdown.ClearOptions);
      return subject;

      void setDropdownSelectedIndex(Option<A> maybeA) {
        if (maybeA.valueOut(out var a)) {
          if (items.indexOfOutC(a, out var idx)) {
            {if (setToNoneBehaviour.valueOut(out var l, out var r)) r.onValue(dropdown, a, log); else onValue(l);}
            dropdown.SetValueWithoutNotify(idx);
          }
          else {
            log.error($"Can't find item={a} in items, doing nothing. Items={items.mkStringEnum()}");
          }
        }
        else {
          {if (setToNoneBehaviour.valueOut(out var l, out var r)) r.onNone(dropdown, log); else onNone(l);}
        }
      }
      
      void onNone(DropdownSetToNoneBehaviour behaviour) {
        switch (behaviour) {
          case DropdownSetToNoneBehaviour.UsePlaceholder: onPlaceholder(); break;
          case DropdownSetToNoneBehaviour.Disable: dropdown.setActiveGO(false); break;
          default: throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null);
        }

        void onPlaceholder() {
          if (dropdown.placeholder) dropdown.SetValueWithoutNotify(-1);
          else {
            log.error("Wanted to set None on dropdown, but it does not have placeholder set! Adding a placeholder!");
            if (!dropdown.options.last().exists(noPlaceholderOption)) {
              dropdown.options.Add(new TMP_Dropdown.OptionData("ERROR: None, but placeholder missing"));
            }

            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
          }
        }
      }

      void onValue(DropdownSetToNoneBehaviour behaviour) {
        switch (behaviour) {
          case DropdownSetToNoneBehaviour.UsePlaceholder: break;
          case DropdownSetToNoneBehaviour.Disable: dropdown.setActiveGO(true); break;
          default: throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null);
        }
      }
    }
    
    /// <summary>
    /// Like <see cref="bind{A}"/> but inserts <see cref="emptyItem"/> which represents the None case. 
    /// </summary>
    public static IRxObservable<Option<A>> bindWithEmpty<A>(
      this TMP_Dropdown dropdown, ITracker tracker, TMP_Dropdown.OptionData emptyItem, 
      ImmutableArrayC<(A, TMP_Dropdown.OptionData)> values, IRxVal<Option<A>> selected, [Implicit] ILog log=default
    ) {
      // Index 0 is None.
      // Index 1-N is element in values array

      var subject = new Subject<Option<A>>();
      dropdown.setDropdownOptions(emptyItem.yield().Concat(values._unsafeArray.map(_ => _.Item2)));
      
      var items = values._unsafeArray.map(_ => _.Item1);
      // Make sure that when selected item changes we update the dropdown.
      selected.subscribe(tracker, setDropdownSelectedIndex);
      // Subscribe to dropdown value changes, which are initiated by user clicking around.
      dropdown.onValueChanged.toObservable().subscribe(tracker, index => {
        // When user clicks on a dropdown item, the dropdown automatically changes its selected item.
        //
        // We want dropdown to always show the value from the state RX, thus we need to manually set the index back
        // after the user click happens.
        setDropdownSelectedIndex(selected.value);

        if (index == 0) {
          subject.push(None._);
        }
        else {
          var realIndex = index - 1;
          if (items.indexValid(realIndex)) {
            subject.push(Some.a(items[realIndex]));
          }
          else {
            log.error($"This should never happen, can't find {index.echo()} in items: {items.mkStringEnum()}");
          }
        }
      });
      tracker.track(dropdown.ClearOptions);
      return subject;

      void setDropdownSelectedIndex(Option<A> maybeState) {
        var selectedIdx = maybeState.valueOut(out var state) && items.indexOfOutC(state, out var idx) ? idx + 1 : 0;
        dropdown.SetValueWithoutNotify(selectedIdx);
      }
    }

    /// <summary>
    /// Emits an event each frame whether you are hovering with a pointer over a TextMeshPro link. Returns Some when
    /// you are hovering on it and None if you are not hovering on any link in the given <see cref="text"/>.
    ///
    /// WARNING: this method does not respect the hierarchy of rendering. For example: the link can be at the bottom
    /// of the render tree but this method will detect the pointer hovering on the link. 
    ///
    /// If <see cref="getPointerPosition"/> returns None for that frame, the detection is not performed.
    /// </summary>
    public static IRxObservable<Option<TMP_LinkInfo>> onHoverOnLink(
      this TMP_Text text, Func<Option<Vector3>> getPointerPosition, Camera camera = null
    ) {
      // Try to resolve the camera from canvas if it's not provided. If that resolves to null, it is also supported by
      // FindIntersectingLink.
      if (camera == null) camera = text.canvas.worldCamera;

      return ASync.onLateUpdate.map(_ => {
        var maybePosition = getPointerPosition();
        if (!maybePosition.valueOut(out var pointerPosition)) return None._;
        
        // Can be optimized: calculate the RectTransform bounding box and abort immediately if the pointer is outside of
        // that bounding box. This fails if your text overflows the bounding box, thus this behaviour should be
        // toggleable via method parameters.
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, pointerPosition, camera);
        return linkIndex != -1 ? Some.a(text.textInfo.linkInfo[linkIndex]) : None._;
      });
    }

    public static void setDropdownOptions(this TMP_Dropdown dropdown, IEnumerable<TMP_Dropdown.OptionData> options) {
      dropdown.ClearOptions();
      dropdown.options.AddRange(options);
      dropdown.RefreshShownValue();
    }
  }
}
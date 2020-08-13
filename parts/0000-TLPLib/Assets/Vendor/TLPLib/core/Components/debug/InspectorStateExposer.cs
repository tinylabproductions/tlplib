using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.debug {
  /// <summary>
  /// Exposes fields of non-monobehaviour objects to unity inspector.
  ///
  /// <see cref="InspectorStateExposerExts"/> and <see cref="InspectorStateExposerExts.exposeAllToInspector{A}"/>
  /// </summary>
  public partial class InspectorStateExposer : MonoBehaviour {
    [Matcher] public abstract class IValue {}
    [Record] public sealed partial class StringValue : IValue {
      public readonly string value;
    }
    [Record] public sealed partial class FloatValue : IValue {
      public readonly float value;
    }
    [Record] public sealed partial class ObjectValue : IValue {
      public readonly UnityEngine.Object value;
    }
    [Record] public sealed partial class ActionValue : IValue {
      public readonly Action value;
    }
#if UNITY_EDITOR
    [Record]
    public readonly partial struct ForRepresentation {
      public readonly object objectReference;
      public readonly string name;
      public readonly IValue value;
    }

    public interface IData {
      Option<ForRepresentation> repr { get; }
    }

    [Record]
    public partial class Data<A> : IData where A : class {
      public readonly WeakReference<A> reference;
      public readonly string name;
      public readonly Func<A, IValue> get;

      public Option<ForRepresentation> repr => 
        reference.TryGetTarget(out var _ref)
        ? F.some(new ForRepresentation(_ref, name, get(_ref)))
        : F.none_;
    }

    readonly List<IData> data = new List<IData>();

    public void add(IData data) => this.data.Add(data);

    public IEnumerable<IGrouping<object, ForRepresentation>> groupedData =>
      data.collect(_ => _.repr).GroupBy(_ => _.objectReference);
#endif
  }

  public static class InspectorStateExposerExts {
    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, InspectorStateExposer.IValue> get
    ) where A : class {
#if UNITY_EDITOR
      var exposer = go.EnsureComponent<InspectorStateExposer>();
      var wr = new WeakReference<A>(reference);
      exposer.add(new InspectorStateExposer.Data<A>(wr, name, get));
#endif
    }

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, string> get
    ) where A : class => go.exposeToInspector(reference, name, a => new InspectorStateExposer.StringValue(get(a)));

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, float> get
    ) where A : class => go.exposeToInspector(reference, name, a => new InspectorStateExposer.FloatValue(get(a)));

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, bool> get
    ) where A : class => go.exposeToInspector(reference, name, a => get(a) ? "true" : "false");

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, UnityEngine.Object> get
    ) where A : class => go.exposeToInspector(reference, name, x => new InspectorStateExposer.ObjectValue(get(x)));

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Action<A> onClick
    ) where A : class => go.exposeToInspector(
      reference, name, x => new InspectorStateExposer.ActionValue(() => onClick(x))
    );

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void exposeAllToInspector<A>(
      this GameObject go, A reference
    ) where A : class {
      foreach (var field in typeof(A).getAllFields()) {
        var fieldType = field.FieldType;
        if (fieldType.IsSubclassOf(typeof(float)))
          exposeToInspector(go, reference, field.Name, a => (float) field.GetValue(a));
        else if (fieldType.IsSubclassOf(typeof(bool)))
          exposeToInspector(go, reference, field.Name, a => (bool) field.GetValue(a));
        else if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
          exposeToInspector(go, reference, field.Name, a => (UnityEngine.Object) field.GetValue(a));
        else
          exposeToInspector(go, reference, field.Name, a => {
            var obj = field.GetValue(a);
            return obj == null ? "null" : obj.ToString();
          });
      }
    }
  }
}
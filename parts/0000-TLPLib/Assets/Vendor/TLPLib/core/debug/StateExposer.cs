using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.debug {
  /// <summary>
  /// Exposes fields of objects to Unity window.
  ///
  /// <see cref="StateExposerExts"/> and <see cref="StateExposerExts.exposeAllToInspector{A}"/>
  /// </summary>
  [Singleton, PublicAPI] public partial class StateExposer {
    public readonly Scope rootScope = new();

    /// <summary>Cleans everything before starting the game.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void reset() => instance.rootScope.clearNonStatics(); 

    public Scope withScope(ScopeKey name) => rootScope.withScope(name);
    public static Scope operator /(StateExposer e, ScopeKey name) => e.withScope(name);

    [PublicAPI, Record] public sealed partial class ScopeKey {
      public readonly string name;
      public readonly Option<UnityEngine.Object> unityObject;

      /// <summary>Is this scope still valid?</summary>
      public bool isValid => unityObject.fold(true, obj => obj);

      public static ScopeKey fromString(string name) => new ScopeKey(name, unityObject: None._);
      public static implicit operator ScopeKey(string name) => fromString(name);

      public static ScopeKey fromUnityObject(UnityEngine.Object obj) => new ScopeKey(
        Log.d.isDebug() && obj is GameObject go ? go.transform.debugPath() : obj.name, 
        unityObject: Some.a(obj)
      );
      public static implicit operator ScopeKey(UnityEngine.Object obj) => fromUnityObject(obj);
    }
    
    [PublicAPI] public sealed class Scope {
      readonly Dictionary<ScopeKey, Scope> _scopes = new();
      readonly List<IData> data = new();

      public void add(IData data) => this.data.Add(data);

      /// <summary>Clear all non statically accessible registered values.</summary>
      public void clearNonStatics() {
        lock (this) {
          var invalidKeys = new HashSet<ScopeKey>();
          foreach (var (key, scope) in _scopes) {
            if (key.isValid) scope.clearNonStatics();
            else invalidKeys.Add(key);
          }
          foreach (var invalidKey in invalidKeys) {
            _scopes.Remove(invalidKey);
          }
          data.removeWhere(_ => !_.isStatic);
        }
      }

      public KeyValuePair<ScopeKey, Scope>[] scopes {
        get {
          // Copy the scopes on access to ensure the locking.
          lock (this) return _scopes.ToArray();
        }
      }
      
      public IEnumerable<IGrouping<Option<object>, ForRepresentation>> groupedData =>
        data.collect(_ => _.representation).GroupBy(_ => _.objectReference);
      
      public Scope withScope(ScopeKey name) {
        // Lock to support accessing from all threads.
        lock (this) { return _scopes.getOrUpdate(name, () => new()); }
      }

      public static Scope operator /(Scope e, ScopeKey name) => e.withScope(name);

      /// <summary>Exposes a named value that is available statically (not via an object instance).</summary>
      public void exposeStatic(string name, Func<IRenderableValue> get) => add(new StaticData(name, get));
      public void exposeStatic(string name, Func<string> get) => exposeStatic(name, () => new StringValue(get()));
      public void exposeStatic(string name, Func<float> get) => exposeStatic(name, () => new FloatValue(get()));
      public void exposeStatic(string name, Func<bool> get) => exposeStatic(name, () => new BoolValue(get()));
      public void exposeStatic(string name, Func<UnityEngine.Object> get) => exposeStatic(name, () => new ObjectValue(get()));
      public void exposeStatic(string name, Func<Action> onClick) => exposeStatic(name, () => new ActionValue(onClick()));
      /// <summary>Helper for exposing <see cref="Future{A}"/>.</summary>
      public void exposeStatic<A>(string name, Func<Future<A>> get) => exposeStatic(name, () => get().ToString());
      
      /// <summary>Exposes a named value that is available via an object instance.</summary>
      public void expose<A>(A reference, string name, Func<A, IRenderableValue> get) where A : class => 
        add(new InstanceData<A>(reference.weakRef(), name, get));
      
      /// <summary>
      /// Helper for exposing <see cref="Future{A}"/>. Does not do anything if the future is not async (because it's a
      /// struct then).
      /// </summary>
      public void expose<A>(Future<A> future, string name) {
        if (future.asHeapFuture.valueOut(out var heapFuture)) expose(heapFuture, name, _ => _.ToString());
      }
    }

    public interface IData {
      /// <summary>Does this data represent statically accessible values?</summary>
      bool isStatic { get; }
      Option<ForRepresentation> representation { get; }
    }

    /// <summary>Value that is available an object instance.</summary>
    [Record] partial class InstanceData<A> : IData where A : class {
      readonly WeakReference<A> reference;
      readonly string name;
      readonly Func<A, IRenderableValue> get;

      public bool isStatic => false;

      public Option<ForRepresentation> representation => 
        reference.TryGetTarget(out var _ref)
          ? Some.a(new ForRepresentation(Some.a<object>(_ref), name, get(_ref)))
          : None._;
    }

    /// <summary>Value that is available statically.</summary>
    [Record] partial class StaticData : IData {
      readonly string name;
      readonly Func<IRenderableValue> get;

      public bool isStatic => true;
      public Option<ForRepresentation> representation => Some.a(new ForRepresentation(None._, name, get()));
    }
    
    /// <summary>Allows you to represent a runtime value.</summary>
    [Record] public readonly partial struct ForRepresentation {
      /// <summary>None if this value is available statically.</summary>
      public readonly Option<object> objectReference;
      public readonly string name;
      public readonly IRenderableValue value;
    }

    /// <summary>Represents a value that we can render.</summary>
    [Matcher] public abstract class IRenderableValue {
      public static implicit operator IRenderableValue(string v) => new StringValue(v);
      public static implicit operator IRenderableValue(float v) => new FloatValue(v);
      public static implicit operator IRenderableValue(double v) => new FloatValue((float) v);
      public static implicit operator IRenderableValue(bool v) => new BoolValue(v);
      public static implicit operator IRenderableValue(UnityEngine.Object v) => new ObjectValue(v);
      public static implicit operator IRenderableValue(Action v) => new ActionValue(v);
      public static implicit operator IRenderableValue(IRenderableValue[] v) => new EnumerableValue(v);
    }
    [Record] public sealed partial class StringValue : IRenderableValue { public readonly string value; }
    [Record] public sealed partial class FloatValue : IRenderableValue { public readonly float value; }
    [Record] public sealed partial class BoolValue : IRenderableValue { public readonly bool value; }
    [Record] public sealed partial class ObjectValue : IRenderableValue { public readonly UnityEngine.Object value; }
    [Record] public sealed partial class ActionValue : IRenderableValue {
      public readonly string label;
      public readonly Action value;

      public ActionValue(Action value) : this("Run", value) {}
    }
    /// <summary>Renders the key in column 1 and value in column 2.</summary>
    [Record] public sealed partial class KVValue : IRenderableValue { public readonly IRenderableValue key, value; }
    /// <summary>Renders a header and then the value, but indents the value by the specified indent.</summary>
    [Record] public sealed partial class HeaderValue : IRenderableValue {
      public readonly IRenderableValue header, value;
      public readonly byte indentBy;

      public HeaderValue(IRenderableValue header, IRenderableValue value) : this(header, value, 1) {}
    }
    /// <summary>Renders given values with header stating the count and separator between values.</summary>
    [Record] public sealed partial class EnumerableValue : IRenderableValue {
      /// <summary>Should we render the element count?</summary>
      public readonly bool showCount;
      public readonly IReadOnlyCollection<IRenderableValue> values;

      public EnumerableValue(IReadOnlyCollection<IRenderableValue> values) : this(showCount: true, values) {}
    }
  }

  [PublicAPI] public static class StateExposerExts {
    public static void exposeToInspector<A>(
      this GameObject go, A reference, string name, Func<A, StateExposer.IRenderableValue> get
    ) where A : class {
      (StateExposer.instance / go).expose(reference, name, get);
    }

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
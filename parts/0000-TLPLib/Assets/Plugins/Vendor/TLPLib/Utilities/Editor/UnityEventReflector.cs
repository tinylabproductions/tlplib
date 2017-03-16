using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.reflection;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static class UnityEventReflector {
    const string
      UnityEngineAssembly = 
        "UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
      UnityEngineEventsPersistentCallGroup =
        "UnityEngine.Events.PersistentCallGroup, " + UnityEngineAssembly,
      UnityEngineEventsPersistentCall =
        "UnityEngine.Events.PersistentCall, " + UnityEngineAssembly;
    static readonly object[] noArgs = {};

    public struct PersistentCallGroup {
      public static readonly Type type = Type2.getType(UnityEngineEventsPersistentCallGroup).rightOrThrow;
      static readonly FieldInfo f_Calls = type.GetField(
        "m_Calls",
        BindingFlags.Instance | BindingFlags.NonPublic
      );

      readonly object obj;

      public PersistentCallGroup(object obj) { this.obj = obj; }

      public Option<ListPersistentCall> calls =>
        F.opt((IList) f_Calls.GetValue(obj)).map(l => new ListPersistentCall(l));
    }

    public struct PersistentCall {
      static readonly Type type = Type2.getType(UnityEngineEventsPersistentCall).rightOrThrow;
      static readonly MethodInfo m_isValid = type.GetMethod("IsValid");

      public readonly object obj;
      public PersistentCall(object obj) { this.obj = obj; }

      public bool isValid => (bool) m_isValid.Invoke(obj, noArgs);
    }

    public struct ListPersistentCall : IEnumerable<PersistentCall> {
      readonly IList obj;
      public ListPersistentCall(IList obj) { this.obj = obj; }

      public IEnumerator<PersistentCall> GetEnumerator() {
        foreach (var o in obj) yield return new PersistentCall(o);
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static readonly Act<UnityEventBase> rebuildPersistentCallsIfNeeded;
    public static void __rebuildPersistentCallsIfNeeded(this UnityEventBase evt) =>
      rebuildPersistentCallsIfNeeded(evt);

    public static readonly Fn<UnityEventBase, PersistentCallGroup> persistentCalls;
    public static PersistentCallGroup __persistentCalls(this UnityEventBase evt) =>
      persistentCalls(evt);

    public static readonly Fn<UnityEventBase, PersistentCall, Option<MethodInfo>> findMethod;
    public static Option<MethodInfo> __findMethod(this UnityEventBase evt, PersistentCall pc) =>
      findMethod(evt, pc);

    static UnityEventReflector() {
      // ReSharper disable PossibleNullReferenceException
      var baseType = typeof(UnityEventBase);

      var m_RebuildPersistentCallsIfNeeded = baseType.GetMethod(
        "RebuildPersistentCallsIfNeeded",
        BindingFlags.Instance | BindingFlags.NonPublic
      );
      rebuildPersistentCallsIfNeeded = 
        ue => m_RebuildPersistentCallsIfNeeded.Invoke(ue, noArgs);

      var f_PersistentCalls = baseType.GetField(
        "m_PersistentCalls", 
        BindingFlags.Instance | BindingFlags.NonPublic
      );
      persistentCalls = ue => new PersistentCallGroup(f_PersistentCalls.GetValue(ue));

      var persistentCallType = Type2.getType(UnityEngineEventsPersistentCall).rightOrThrow;
      var m_methodInfo =
        typeof(UnityEventBase).GetMethod(
          "FindMethod",
          BindingFlags.Instance | BindingFlags.NonPublic,
          Type.DefaultBinder,
          new[] { persistentCallType },
          null
        );
      findMethod = (ue, pc) => F.opt((MethodInfo) m_methodInfo.Invoke(ue, new [] {pc.obj}));

      // ReSharper restore PossibleNullReferenceException
    }
  }
}
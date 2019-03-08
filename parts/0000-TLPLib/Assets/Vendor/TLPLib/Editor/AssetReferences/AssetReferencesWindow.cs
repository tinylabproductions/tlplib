using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.AssetReferences {
  class AssetReferencesAssetProcessor : AssetPostprocessor {
    [UsedImplicitly]
    static void OnPostprocessAllAssets(
      string[] importedAssets, string[] deletedAssets, string[] movedAssets,
      string[] movedFromAssetPaths
    ) {
      if (!AssetReferencesWindow.enabled.value) return;

      var data = new AssetUpdate(
        importedAssets.ToImmutableList(),
        deletedAssets.ToImmutableList(),
        movedFromAssetPaths.zip(movedAssets).Select(t => new AssetUpdate.Move(t._1, t._2)).ToImmutableList()
      );

      AssetReferencesWindow.processFiles(data);
    }
  }

  // Ugly code ahead 🚧
  public class AssetReferencesWindow : EditorWindow, IMB_OnGUI, IMB_Update, IMB_OnEnable, IMB_OnDisable {
    Vector2 scrollPos;
    // disables automatically on code refresh
    // code refresh happens on code change and when entering play mode
    public static readonly IRxRef<bool> enabled = RxRef.a(false);

    [MenuItem("TLP/Window/Asset References")]
    public static void init() {
      // Get existing open window or if none, make a new one:
      var window = GetWindow<AssetReferencesWindow>("Asset References");
      window.Show();
    }

    // ReSharper disable once NotAccessedField.Local
    static ISubscription enabledSubscription;

    [InitializeOnLoadMethod, UsedImplicitly]
    static void initTasks() {
      enabledSubscription = enabled.subscribe(NoOpDisposableTracker.instance, b => {
        if (b) {
          refsOpt = Option<AssetReferences>.None;
          processFiles(AssetUpdate.fromAllAssets(AssetDatabase.GetAllAssetPaths().ToImmutableList()));
        }
      });
    }

    static readonly Ref<float> progress = Ref.a(0f);
    static volatile bool processing, needsRepaint;
    static Option<AssetReferences> refsOpt;
    static readonly PCQueue worker = new PCQueue(1);

    public static void processFiles(AssetUpdate data) {
      if (!enabled.value) return;
      var log = Log.@default;
      worker.EnqueueItem(() => {
        try {
          process(data, log);
        }
        catch (Exception e) {
          log.error(e);
        }
      });
    }

    static void process(AssetUpdate data, ILog log) {
      try {
        processing = true;
        needsRepaint = true;

        refsOpt.voidFold(
          () => refsOpt = AssetReferences.a(data, Environment.ProcessorCount, progress, log).some(),
          refs => refs.update(data, Environment.ProcessorCount, progress, log)
        );
      }
      finally {
        processing = false;
      }
    }

    bool foldout1 = true, foldout2 = true, foldout3 = true;
    Object hoverItem, previousHoverItem, lockedObj;
    readonly IRxRef<bool> locked = RxRef.a(false);
    bool showActions;

    public void OnGUI() {
      var isMouseMoveEvent = Event.current.type == EventType.MouseMove;
      if (isMouseMoveEvent) hoverItem = null;
      if (processing) {
        EditorGUI.ProgressBar(new Rect(10, 10, position.width - 20, 20), progress.value, "Processing");
      }
      else {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        enabled.value = EditorGUILayout.Toggle("Enabled", enabled.value);
        foreach (var _ in enabled.value.opt(F.unit)) {
          locked.value = EditorGUILayout.Toggle("Lock", locked.value);
          showActions = EditorGUILayout.Toggle("Show Actions", showActions);
          var cur = locked.value ? lockedObj : Selection.activeObject;
          if (cur == null) break;
          var curPath = AssetDatabase.GetAssetPath(cur);
          if (curPath == null) break;
          var currentGUID = AssetDatabase.AssetPathToGUID(curPath);
          if (currentGUID == null) break;
          GUILayout.Label("Selected");
          objectDisplay(currentGUID);
          foreach (var refs in refsOpt) {
            displayObjects(currentGUID, "Used by objects (parents)", refs.parents, ref foldout1);
            displayObjects(currentGUID, "Contains (children)", refs.children, ref foldout2);
            displayObjects("Placed in scenes", refs.findParentScenes(currentGUID), ref foldout3);
          }
          if (!isMouseMoveEvent && hoverItem) {
            GUI.Label(new Rect(Event.current.mousePosition, new Vector2(128, 128)), AssetPreview.GetAssetPreview(previousHoverItem));
          }
        }
        EditorGUILayout.EndScrollView();
      }
      if (isMouseMoveEvent) {
        if (previousHoverItem != hoverItem || hoverItem) Repaint();
        previousHoverItem = hoverItem;
      }
    }

    void displayObjects(
      string curGuid, string name, Dictionary<string, HashSet<string>> dict, ref bool foldout
    ) {
      if (dict.ContainsKey(curGuid)) {
        displayObjects(name, dict[curGuid], ref foldout);
      }
      else {
        GUILayout.Label(name + " 0");
      }
    }

    void displayObjects(string name, ICollection<string> guids, ref bool foldout) {
      foldout = EditorGUILayout.Foldout(foldout, name + " " + guids.Count);
      if (foldout) {
        if (showActions) {
          if (GUILayout.Button("select"))
            Selection.objects = loadGuids(guids).ToArray();

          if (GUILayout.Button("set dirty")) {
            var objects = loadGuids(guids).ToArray();
            objects.recordEditorChanges("Set objects dirty");
            foreach (var o in objects) EditorUtility.SetDirty(o);
          }
        }

        foreach (var guid in guids.OrderBy(AssetDatabase.GUIDToAssetPath)) {
          var asset = AssetDatabaseUtils.loadMainAssetByGuid(guid);
          if (asset != null) {
            objectDisplay(guid);
          }
          else {
            GUILayout.Label(AssetDatabase.GUIDToAssetPath(guid));
          }
        }
      }
    }

    // ToArray in case someone modifies guids collection
    static IEnumerable<Object> loadGuids(IEnumerable<string> guids) =>
      guids.ToArray().Select(AssetDatabaseUtils.loadMainAssetByGuid);

    void objectDisplay(string guid) {
      EditorGUILayout.BeginHorizontal();
      var obj = AssetDatabaseUtils.loadMainAssetByGuid(guid);
      EditorGUILayout.ObjectField(obj, typeof(Object), false);
      var etype = Event.current.type;
      if (etype == EventType.MouseMove) {
        var info = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.NonPublic | BindingFlags.Static);
        // ReSharper disable once PossibleNullReferenceException
        var rect = (Rect)info.GetValue(null);
        var mousePos = Event.current.mousePosition;
        if (rect.Contains(mousePos)) {
          hoverItem = obj;
        }
      }

      if (GUILayout.Button("", GUILayout.MaxWidth(30))) {
        Selection.activeObject = obj;
      }
      EditorGUILayout.EndHorizontal();
    }

    public void Update() {
      if (processing) Repaint();
      else if (needsRepaint) {
        Repaint();
        needsRepaint = false;
      }
    }

    [UsedImplicitly]
    void OnSelectionChange() => Repaint();

    readonly IDisposableTracker tracker = new DisposableTracker();

    public void OnEnable() {
      wantsMouseMove = true;
      locked.subscribe(tracker, v => {
        if (v) lockedObj = Selection.activeObject;
      });
    }

    public void OnDisable() => tracker.Dispose();
  }
}
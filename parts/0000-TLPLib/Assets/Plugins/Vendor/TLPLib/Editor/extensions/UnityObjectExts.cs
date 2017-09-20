using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public struct EditorAssetInfo {
    public readonly PathStr path;
    public readonly string guid;

    public EditorAssetInfo(PathStr path, string guid) {
      this.path = path;
      this.guid = guid;
    }

    public override string ToString() => 
      $"{nameof(EditorAssetInfo)}[" +
      $"{nameof(path)}: {path}, " +
      $"{nameof(guid)}: {guid}" +
      $"]";
  }

  public struct EditorObjectInfo<A> where A : Object {
    public readonly A obj;
    /* Present if object is an asset on disk. */
    public readonly Option<EditorAssetInfo> assetInfo;

    public string name => obj.name;

    public EditorObjectInfo(A obj, Option<EditorAssetInfo> assetInfo) {
      this.obj = obj;
      this.assetInfo = assetInfo;
    }

    public override string ToString() => 
      $"{nameof(EditorObjectInfo<A>)}[" +
      $"{nameof(obj)}: {obj}, " +
      $"{nameof(assetInfo)}: {assetInfo}" +
      $"]";
  }

  public static class UnityObjectExts {
    public static EditorObjectInfo<A> debugInfo<A>(this A o) where A : Object {
      var pathOpt = AssetDatabase.GetAssetPath(o).opt().map(PathStr.a);
      var assetInfoOpt = pathOpt.map(path => {
        var guid = AssetDatabase.AssetPathToGUID(path);
        return new EditorAssetInfo(path, guid);
      });
      return new EditorObjectInfo<A>(o, assetInfoOpt);
    }

    [UsedImplicitly, MenuItem("TLP/Debug/Debug info")]
    public static void editorUtility() {
      var obj = F.opt(Selection.activeObject);
      obj.voidFold(
        () => EditorUtils.userInfo("No object selected!", "Please select an object!"),
        o => EditorUtils.userInfo($"Debug info for {o}", o.debugInfo().ToString())
      );
    }
  }
}
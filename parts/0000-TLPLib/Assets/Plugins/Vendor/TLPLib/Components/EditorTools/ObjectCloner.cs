using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  public class ObjectCloner : MonoBehaviour {
    public enum LockedAxis { X, Y, Z }

    public struct EditorData {
      public readonly GameObject prefab;
      public readonly Transform sourceTransform;

      public EditorData(GameObject prefab, Transform sourceTransform) {
        this.prefab = prefab;
        this.sourceTransform = sourceTransform;
      }
    }

// ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning disable 649

    [
      SerializeField, 
      Tooltip("Objects will be instantiated with this set as their parent.")
    ] Transform _parent;
    public Option<Transform> parent => _parent.opt();

    [
      SerializeField,
      Tooltip("Object to clone. Can be a scene object or a prefab.")
    ] GameObject _prefab;

    [
      SerializeField,
      Tooltip("If this is set, use this transform as source for coordinates/rotation/scale.")
    ] Transform _sourceTransform;

    [
      SerializeField,
      Tooltip("Locks the cloned objects into this axis.")
    ] LockedAxis _lockedAxis = LockedAxis.Z;
    public LockedAxis lockedAxis => _lockedAxis;

#pragma warning restore 649
// ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local

    public Option<EditorData> editorData => 
      _prefab.opt().map(prefab => new EditorData(
        prefab,
        _sourceTransform.opt().getOrElse(prefab.transform)
      ));
  }
}
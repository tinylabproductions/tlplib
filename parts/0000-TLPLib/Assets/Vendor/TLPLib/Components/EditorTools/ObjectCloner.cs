using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  public class ObjectCloner : MonoBehaviour {
    public enum LockedAxis { X, Y, Z }
  
#pragma warning disable 649
    [SerializeField] Transform _parent;
    public Transform parent => _parent;

    [SerializeField] GameObject _prefab;
    public GameObject prefab => _prefab;

    [SerializeField] LockedAxis _lockedAxis = LockedAxis.Z;
    public LockedAxis lockedAxis => _lockedAxis;
#pragma warning restore 649
  }
}
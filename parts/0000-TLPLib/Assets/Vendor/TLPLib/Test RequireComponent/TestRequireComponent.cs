using UnityEngine;

namespace com.tinylabproductions.TLPLib.Test.Utilities {
  [AddComponentMenu("")]
  [RequireComponent(typeof(TestComponent1), typeof(TestComponent2), typeof(TestComponent3))]
  public class TestRequireComponent : MonoBehaviour {}
}
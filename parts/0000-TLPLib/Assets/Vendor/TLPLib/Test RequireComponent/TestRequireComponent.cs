using UnityEngine;

namespace com.tinylabproductions.TLPLib.Test.Utilities {
  [RequireComponent(typeof(TestComponent1), typeof(TestComponent2), typeof(TestComponent3))]
  public class TestRequireComponent : MonoBehaviour {}
}
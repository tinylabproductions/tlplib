using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AnimatorExts {
    /// <summary>
    /// When accessing animator parameters from editor, the animator can be not loaded into memory and the parameters
    /// will just return an empty array.
    ///
    /// This behavior is explained in https://forum.unity.com/threads/animator-parameters-array-empty-when-in-prefab.335134/
    ///
    /// This tries to get the parameters out of the editor animator controller when running in Unity Editor.
    /// </summary>
    public static AnimatorControllerParameter[] parametersInEditor(this Animator animator) {
#if UNITY_EDITOR
      return animator.runtimeAnimatorController is UnityEditor.Animations.AnimatorController ctrl
        ? ctrl.parameters
        : animator.parameters;
#else
      return animator.parameters;
#endif
    }
  }
}
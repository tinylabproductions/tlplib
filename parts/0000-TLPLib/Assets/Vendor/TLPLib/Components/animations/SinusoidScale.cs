using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.animations {
  public class SinusoidScale : MonoBehaviour {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] Vector3 from = Vector3.one, to = Vector3.one;
    [SerializeField] float speed = 1;
    [SerializeField] bool useUnscaledTime;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    float timeShift;
    bool timeShiftSet;

    internal void Start() {
      if (!timeShiftSet) {
        setTimeShift(Random.value);
      }
    }

    public void OnEnable() {
      Update();
    }

    internal void Update() {
      var time = useUnscaledTime ? Time.unscaledTime : Time.time;
      transform.localScale = Vector3.Lerp(from, to, (Mathf.Sin(time * speed + timeShift) + 1) * .5f);
    }

    public void setTimeShift(float t) {
      timeShiftSet = true;
      timeShift = t * Mathf.PI * 2;
      Update();
    }
  }
}

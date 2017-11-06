using UnityEngine;

namespace Assets.Code.Utils {
  public class SinusoidScale : MonoBehaviour {
    public Vector3 from = Vector3.one;
    public Vector3 to = Vector3.one;
    public float speed = 1;
    float timeShift;
    bool timeshiftSet;

    internal void Start() {
      if (!timeshiftSet) {
        setTimeShift(Random.value);
      }
    }

    public void OnEnable() {
      Update();
    }

    internal void Update() {
      transform.localScale = Vector3.Lerp(from, to, (Mathf.Sin(Time.time * speed + timeShift) + 1) * .5f);
    }

    public void setTimeShift(float t) {
      timeshiftSet = true;
      timeShift = t * Mathf.PI * 2;
      Update();
    }
  }
}

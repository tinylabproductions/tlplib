using System;
using com.tinylabproductions.TLPLib.UI;
using UnityEngine;

public class UIDownScaleSimple : UIDownScaleBase {

  public override Vector3 targetLocalScale {
    get { return target.localScale; }
    set { target.localScale = value; }
  }

  public override Action onPointerUp { get; }
}

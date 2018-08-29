using System;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Android.Bindings.android.util;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEngine;

namespace com.tinylabproductions.TLPLib {
  [AdvancedInspector(false)]
  public class byeAI : MonoBehaviour {
    //[SerializeField] Either<float, string> either;
    [SerializeField] alio zdarova;

  }

  [Serializable]
  public class alio : UnityEither<float, string> {
    public alio(Either<float, string> ytheris) {
    }
    
  }
  
}
}
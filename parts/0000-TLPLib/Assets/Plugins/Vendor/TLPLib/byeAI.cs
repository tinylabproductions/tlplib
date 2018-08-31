using System;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Android.Bindings.android.util;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using Plugins.Vendor.TLPLib.Editor.CustomEditors;
using UnityEngine;

namespace com.tinylabproductions.TLPLib {
  [AdvancedInspector(false)]
  public class byeAI : MonoBehaviour {
    //[SerializeField] Either<float, string> either;
    public alio zdarova;
    [SerializeField] alio2 optionas;
    

  }

  [Serializable]
  public class alio : UnityEither<float, string> {
    
  }

  [Serializable]
  public class alio2 : UnityOption<int> {
    
  }
  
}
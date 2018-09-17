using System;
using com.tinylabproductions.TLPLib.Android.Bindings.android.util;
using com.tinylabproductions.TLPLib.Components.gradient;
using com.tinylabproductions.TLPLib.Components.sorting_layer;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

  public class byeAI : MonoBehaviour {
    //[SerializeField] Either<float, string> either;
//    [SerializeField] myEither EitherName;
//    [SerializeField] alio23 optionas;
//    [SerializeField] alio2 optionas2;
//    [SerializeField] Tag tag;
    [SerializeField] GraphicStyle stail;
    SortingLayerReference aaa;


    void Awake() {
      gameObject.AddComponent<gradient>();
      aaa = ScriptableObject.CreateInstance<SortingLayerReference>();
    }
    
    public void printMe(){
      Debug.Log("pressed");
    }
  }
  [Serializable]
  public class myEither : UnityEither<Transform, multi> {
    
  }



[Serializable]
public class myEither22 : UnityEither<string, alio23> {
    
}

[Serializable]
public class gradient : GradientTextureBase {
  protected override void setTexture(Texture2D texture) { throw new NotImplementedException(); }
}

[Serializable]
public class multi {
  public int a, b, c;
  public multi2 mul;
}

[Serializable]
public class multi2 {
  public int e, f, g;
}

[Serializable]
public class alio23 : UnityOption<multi2> {
}

  [Serializable]
  public class alio2 : UnityOption<string> {
  }
  
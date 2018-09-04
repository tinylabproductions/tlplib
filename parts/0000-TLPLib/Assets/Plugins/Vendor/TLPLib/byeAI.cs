using System;
using com.tinylabproductions.TLPLib.Android.Bindings.android.util;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEngine;

  public class byeAI : MonoBehaviour {
    //[SerializeField] Either<float, string> either;
    [SerializeField] myEither EitherName;
    [SerializeField] alio23 optionas;
    [SerializeField] alio2 optionas2;


  }
  [Serializable]
  public class myEither : UnityEither<Transform, multi> {
    
  }

[Serializable]
public class myEither22 : UnityEither<string, alio2> {
  public myEither22() { }
  public myEither22(Either<string, alio2> either) : base(either) { }
    
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
  
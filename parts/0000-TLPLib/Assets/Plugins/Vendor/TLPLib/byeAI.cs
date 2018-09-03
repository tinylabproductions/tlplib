using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.unity_serialization;
using UnityEngine;

  public class byeAI : MonoBehaviour {
    //[SerializeField] Either<float, string> either;
    [SerializeField] myEither EitherName;
    //[SerializeField] alio2 optionas;

  }

  [Serializable]
  public class myEither : UnityEither<Transform, string> {
    public myEither() { }
    public myEither(Either<Transform, string> either) : base(either) { }
    
  }

  [Serializable]
  public class alio2 : UnityOption<int> {
  }
  
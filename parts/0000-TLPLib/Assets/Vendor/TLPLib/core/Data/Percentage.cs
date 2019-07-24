﻿using System;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable, Record]
  public partial class Percentage {
    [SerializeField, Range(0, 1), PublicAccessor] float _value;
  }
}
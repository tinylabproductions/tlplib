using System;
using GenerationAttributes;
using pzd.lib.config;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable, Record]
  public partial struct Percentage {
    [SerializeField, Range(0, 1), PublicAccessor] float _value;

    public static readonly Config.Parser<object, Percentage> parser = Config.floatParser.map(f => new Percentage(f));
  }
}
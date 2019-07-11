using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public struct ValueWithStorage {
    public uint value;
    public uint storage;

    public ValueWithStorage(uint value, uint storage) {
      this.value = value;
      this.storage = storage;
    }

    public ValueWithStorage(Vector2 vector2) : this(
      (uint) vector2.x, (uint) vector2.y
    ) {}

    public ValueWithStorage copy(uint? value=null, uint? storage=null) {
      return new ValueWithStorage(
        value ?? this.value, storage ?? this.storage
      );
    }

    public Vector2 AsVector2() {
      return new Vector2(value, storage);
    }

    public static ValueWithStorage parse(string text) {
      var parts = text.Split('/');
      return new ValueWithStorage(
        Convert.ToUInt32(parts[0].Trim()),
        Convert.ToUInt32(parts[1].Trim())
      );
    }

    public static ValueWithStorage operator +(
      ValueWithStorage vws, uint value
    ) {
      return vws.copy(vws.missing >= value ? vws.value + value : vws.storage);
    }

    public static ValueWithStorage operator -(
      ValueWithStorage vws, uint value
    ) {
      return vws.copy(vws.value <= value ? 0 : vws.value - value);
    }

    public string AsString() {
      return string.Format("{0} / {1}", value, storage);
    }

    public bool isFull { get { return value == storage; }}

    public bool isEmpty { get { return value == 0; } }

    public uint missing { get { return storage - value; } }

    public float percentage { get { return (float) value / storage; } }

    public ValueWithStorage withStorage(uint storage) {
      return new ValueWithStorage(value, storage);
    }

    public ValueWithStorage withValue(uint value) {
      return new ValueWithStorage(value, storage);
    }

    public override string ToString() {
      return string.Format("ValueWithStorage[{0}]", AsString());
    }
  }
}

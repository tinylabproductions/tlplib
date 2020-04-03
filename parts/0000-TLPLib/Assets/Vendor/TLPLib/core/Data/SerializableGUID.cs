using System;
using GenerationAttributes;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable, InlineProperty, Record(GenerateConstructor = ConstructorFlags.None, GenerateToString = false)]
  public partial class SerializableGUID {
    [SerializeField, FormerlySerializedAs("long1"), HideInInspector, PublicAccessor] ulong _long1;
    [SerializeField, FormerlySerializedAs("long2"), HideInInspector, PublicAccessor] ulong _long2;
      
    [CustomContextMenu("Generate new GUID", nameof(generate)), ShowInInspector, HideLabel]
    string GUID {
      get => guid.ToString();
      set => guid = new Guid(value);
    }

    [PublicAPI] public void generate() => guid = Guid.NewGuid();
    
    public SerializableGUID(Guid guid) {
      this.guid = guid;
    }

    [PublicAPI] public Guid guid {
      get => new Guid(
        (uint) _long1,
        (ushort) (_long1 >> 32),
        (ushort) (_long1 >> (32 + 16)),
        (byte) _long2,
        (byte) (_long2 >> 8),
        (byte) (_long2 >> (8 * 2)),
        (byte) (_long2 >> (8 * 3)),
        (byte) (_long2 >> (8 * 4)),
        (byte) (_long2 >> (8 * 5)),
        (byte) (_long2 >> (8 * 6)),
        (byte) (_long2 >> (8 * 7))
      );
      private set {
        var bytes = value.ToByteArray();
        _long1 = BitConverter.ToUInt64(bytes, 0);
        _long2 = BitConverter.ToUInt64(bytes, 8);      
      }
    }

    [PublicAPI] public bool isZero => _long1 == 0 && _long2 == 0;

    public override string ToString() => guid.ToString();
  }
}
using System;
using GenerationAttributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Data {
  [Serializable, InlineProperty, Record(GenerateConstructor = GeneratedConstructor.None, GenerateToString = false)]
  public partial class SerializableGUID {
    [SerializeField, HideInInspector] ulong long1, long2;
      
    [CustomContextMenu("Generate new GUID", nameof(generate)), ShowInInspector, HideLabel]
    string GUID {
      get => guid.ToString();
      set => guid = new Guid(value);
    }

    public void generate() => guid = Guid.NewGuid();
    
    public SerializableGUID(Guid guid) {
      long1 = 0;
      long2 = 0;
      this.guid = guid;
    }

    Guid guid {
      get => new Guid(
        (uint) long1,
        (ushort) (long1 >> 32),
        (ushort) (long1 >> (32 + 16)),
        (byte) long2,
        (byte) (long2 >> 8),
        (byte) (long2 >> (8 * 2)),
        (byte) (long2 >> (8 * 3)),
        (byte) (long2 >> (8 * 4)),
        (byte) (long2 >> (8 * 5)),
        (byte) (long2 >> (8 * 6)),
        (byte) (long2 >> (8 * 7))
      );
      set  {
        var bytes = value.ToByteArray();
        long1 = BitConverter.ToUInt64(bytes, 0);
        long2 = BitConverter.ToUInt64(bytes, 8);      
      }
    }

    public override string ToString() => guid.ToString();
  }
}
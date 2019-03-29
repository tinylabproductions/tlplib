using System;
using System.IO;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class StreamExts {
    public static unsafe void WriteIntBigEndian(this Stream stream, int i) {
      var block = stackalloc byte[4];
      var asIntPtr = (int*) block;
      *asIntPtr = i;
      if (BitConverter.IsLittleEndian) {
        stream.WriteByte(block[3]);
        stream.WriteByte(block[2]);
        stream.WriteByte(block[1]);
        stream.WriteByte(block[0]);
      }
      else {
        stream.WriteByte(block[0]);
        stream.WriteByte(block[1]);
        stream.WriteByte(block[2]);
        stream.WriteByte(block[3]);
      }
    }
  }
}
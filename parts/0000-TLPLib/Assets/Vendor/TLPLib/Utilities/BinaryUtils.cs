using System;
using System.IO;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Utilities {
  [PublicAPI] public static class BinaryUtils {
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
    
    public static unsafe void WriteIntBigEndian(this byte[] writeBuffer, int i, int offset) {
      var block = stackalloc byte[4];
      var asIntPtr = (int*) block;
      *asIntPtr = i;
      if (BitConverter.IsLittleEndian) {
        writeBuffer[offset]     = block[3];
        writeBuffer[offset + 1] = block[2];
        writeBuffer[offset + 2] = block[1];
        writeBuffer[offset + 3] = block[0];
      }
      else {
        writeBuffer[offset]     = block[0];
        writeBuffer[offset + 1] = block[1];
        writeBuffer[offset + 2] = block[2];
        writeBuffer[offset + 3] = block[3];
      }
    }

    public static int ReadIntBigEndian(this byte[] data, int offset) => 
      data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3];
  }
}
using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Utilities {
  // https://blogs.technet.microsoft.com/cloudpfe/2018/04/18/base32-encoding-and-decoding-in-c/
  [PublicAPI] public static class Base32 {
    /// <summary>
    /// The different characters allowed in Base32 encoding.
    /// </summary>
    /// <remarks>
    /// This is a 32-character subset of the twenty-six letters A–Z and six digits 2–7.
    /// <see cref="https://en.wikipedia.org/wiki/Base32" />
    /// </remarks>
    const string Base32AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Converts a byte array into a Base32 string.
    /// </summary>
    /// <param name="input">The string to convert to Base32.</param>
    /// <param name="addPadding">Whether or not to add RFC3548 '='-padding to the string.</param>
    /// <returns>A Base32 string.</returns>
    /// <remarks>
    /// https://tools.ietf.org/html/rfc3548#section-2.2 indicates padding MUST be added unless the reference to the RFC tells us otherswise.
    /// https://github.com/google/google-authenticator/wiki/Key-Uri-Format indicates that padding SHOULD be omitted.
    /// To meet both requirements, you can omit padding when required.
    /// </remarks>
    public static string ToBase32String(this byte[] input, bool addPadding = true) {
      if (input == null || input.Length == 0) {
        return string.Empty;
      }

      var bits = input.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Aggregate((a, b) => a + b)
        .PadRight((int) (Math.Ceiling((input.Length * 8) / 5d) * 5), '0');
      var result = Enumerable.Range(0, bits.Length / 5)
        .Select(i => Base32AllowedCharacters.Substring(Convert.ToInt32(bits.Substring(i * 5, 5), 2), 1))
        .Aggregate((a, b) => a + b);
      if (addPadding) {
        result = result.PadRight((int) (Math.Ceiling(result.Length / 8d) * 8), '=');
      }

      return result;
    }

    public static string EncodeAsBase32String(this string input, bool addPadding = true) {
      if (string.IsNullOrEmpty(input)) {
        return string.Empty;
      }

      var bytes = Encoding.UTF8.GetBytes(input);
      var result = bytes.ToBase32String(addPadding);
      return result;
    }

    public static string DecodeFromBase32String(this string input) {
      if (string.IsNullOrEmpty(input)) {
        return string.Empty;
      }

      var bytes = input.ToByteArray();
      var result = Encoding.UTF8.GetString(bytes);
      return result;
    }

    /// <summary>
    /// Converts a Base32 string into the corresponding byte array, using 5 bits per character.
    /// </summary>
    /// <param name="input">The Base32 String</param>
    /// <returns>A byte array containing the properly encoded bytes.</returns>
    public static byte[] ToByteArray(this string input) {
      if (string.IsNullOrEmpty(input)) {
        return new byte[0];
      }

      var bits = input.TrimEnd('=').ToUpper().ToCharArray()
        .Select(c => Convert.ToString(Base32AllowedCharacters.IndexOf(c), 2).PadLeft(5, '0'))
        .Aggregate((a, b) => a + b);
      var result = Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2))
        .ToArray();
      return result;
    }
  }
}
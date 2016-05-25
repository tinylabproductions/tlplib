using System;
using System.Globalization;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Formats.JSON {
  /**
string
  ""
  " chars "
chars
    char
    char chars
char
    any-Unicode-character-
        except-"-or-\-or-
        control-character
    \"
    \\
    \/
    \b
    \f
    \n
    \r
    \t
    \u four-hex-digits 
  **/
  public class JString : JsonParser<string> {
    public readonly static JString instance = new JString();
    JString() {}

    public override Either<JsonParserFailure, JsonParserResult<string>> parse(
      string jsonString, JCursor cursor
    ) {
      var quotesOpen = false;
      var escapeOpen = false;
      var unicodeSymbols = -1;
      var unicodeBuf = new byte[4];
      int idx;
      StringBuilder sb = null;
      for (idx = cursor.index; idx < jsonString.Length; idx++) {
        var current = jsonString[idx];
        if (quotesOpen) {
          if (unicodeSymbols != -1) {
            if (isHex(current)) {
              unicodeBuf[unicodeSymbols] = (byte) current;
              unicodeSymbols++;

              if (unicodeSymbols == 4) {
                var hexString = Encoding.ASCII.GetString(unicodeBuf);
                sb.Append((char) int.Parse(hexString, NumberStyles.HexNumber));
                unicodeSymbols = -1;
              }
            }
            else {
              sb.Append('\\');
              sb.Append('u');
              for (var uIdx = 0; uIdx < unicodeSymbols; uIdx++) {
                sb.Append((char) unicodeBuf[uIdx]);
              }
              sb.Append(current);
              unicodeSymbols = -1;
            }
          }
          else if (escapeOpen) {
            switch (current) {
              case '"':
                sb.Append('"');
                break;
              case '\\':
                sb.Append('\\');
                break;
              case '/':
                sb.Append('/');
                break;
              case 'b':
                sb.Append('\b');
                break;
              case 'f':
                sb.Append('\f');
                break;
              case 'n':
                sb.Append('\n');
                break;
              case 'r':
                sb.Append('\r');
                break;
              case 't':
                sb.Append('\t');
                break;
              case 'u':
                unicodeSymbols = 0;
                break;
              default:
                sb.Append('\\');
                sb.Append(current);
                break;
            }
            escapeOpen = false;
          }
          else switch (current) {
            case '\\':
              escapeOpen = true;
              break;
            case '"':
              return result(idx + 1, sb.ToString());
            default:
              sb.Append(current);
              break;
          }
        }
        else {
          if (current == '"') {
            quotesOpen = true;
            sb = new StringBuilder();
          }
          else return fail(idx, $"Expected '\"', but got '{current}'");
        }
      }
      return fail(idx, "Unterminated JSON string");
    }

    static byte[] stringToByteArray(string hex) {
      int NumberChars = hex.Length;
      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }
  }
}

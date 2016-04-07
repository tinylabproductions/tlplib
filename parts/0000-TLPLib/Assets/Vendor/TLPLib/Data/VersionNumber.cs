using System;
using System.Text;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public struct VersionNumber {
    public const char DEFAULT_SEPARATOR = '.';
    public readonly uint major, minor, bugfix;
    public readonly char separator;

    public VersionNumber(uint major, uint minor, uint bugfix, char separator=DEFAULT_SEPARATOR) {
      this.major = major;
      this.minor = minor;
      this.bugfix = bugfix;
      this.separator = separator;
    }

    public static VersionNumber operator +(VersionNumber a, VersionNumber b) {
      if (a.separator != b.separator) throw new ArgumentException(
        $"separators do not match! '{a.separator}' / '{b.separator}'"
      );
      return new VersionNumber(a.major + b.major, a.minor + b.minor, a.bugfix + b.bugfix, a.separator);
    }

    public string asString { get {
      var sb = new StringBuilder();
      sb.Append(major);
      if (minor != 0 || bugfix != 0) {
        sb.Append(separator);
        sb.Append(minor);
      }
      if (bugfix != 0) {
        sb.Append(separator);
        sb.Append(bugfix);
      }
      return sb.ToString();
    } }

    public override string ToString() {
      var str = minor == 0 && bugfix == 0 ? $"{asString},sep={separator}" : asString;
      return $"{nameof(VersionNumber)}[{str}]";
    }

    public static Either<string, VersionNumber> parseString(string s, char separator=DEFAULT_SEPARATOR) {
      var errHeader = $"Can't parse '{s}' as version number with separator '{separator}'";
      var parts = s.Split(separator);
      if (parts.Length > 3)
        return $"{errHeader}: too many parts!".left().r<VersionNumber>();
      if (parts.isEmpty())
        return $"{errHeader}: empty!".left().r<VersionNumber>();
      var majorE = parts[0].parseUInt().mapLeft(e => $"{errHeader} (major): {e}");
      var minorE = getIdx(parts, 1).mapLeft(e => $"{errHeader} (minor): {e}");
      var bugfixE = getIdx(parts, 2).mapLeft(e => $"{errHeader} (bugfix): {e}");
      return majorE.flatMapRight(major => minorE.flatMapRight(minor => bugfixE.mapRight(bugfix =>
        new VersionNumber(major, minor, bugfix, separator)
      )));
    }

    static Either<string, uint> getIdx(string[] parts, int idx)
      { return parts.get(idx).fold(0u.right().l<string>(), _ => _.parseUInt()); }
  }
}

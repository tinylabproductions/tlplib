using System;
using System.Collections.Generic;
using System.Text;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Data {
  public struct VersionNumber : IEquatable<VersionNumber>, IStr, IComparable<VersionNumber> {
    public const char DEFAULT_SEPARATOR = '.';
    public readonly uint major, minor, bugfix;
    public readonly char separator;

    public VersionNumber(uint major, uint minor, uint bugfix, char separator=DEFAULT_SEPARATOR) {
      this.major = major;
      this.minor = minor;
      this.bugfix = bugfix;
      this.separator = separator;
    }

    #region Equality

    public bool Equals(VersionNumber other) {
      return major == other.major && minor == other.minor && bugfix == other.bugfix && separator == other.separator;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is VersionNumber && Equals((VersionNumber) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = (int) major;
        hashCode = (hashCode * 397) ^ (int) minor;
        hashCode = (hashCode * 397) ^ (int) bugfix;
        hashCode = (hashCode * 397) ^ separator.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(VersionNumber left, VersionNumber right) { return left.Equals(right); }
    public static bool operator !=(VersionNumber left, VersionNumber right) { return !left.Equals(right); }

    #endregion

    public int CompareTo(VersionNumber other) {
      var majorComparison = major.CompareTo(other.major);
      if (majorComparison != 0) return majorComparison;
      var minorComparison = minor.CompareTo(other.minor);
      if (minorComparison != 0) return minorComparison;
      return bugfix.CompareTo(other.bugfix);
    }

    public VersionNumber withSeparator(char separator) =>
      new VersionNumber(major, minor, bugfix, separator);

    public static VersionNumber operator +(VersionNumber a, VersionNumber b) {
      if (a.separator != b.separator) throw new ArgumentException(
        $"separators do not match! '{a.separator}' / '{b.separator}'"
      );
      return new VersionNumber(a.major + b.major, a.minor + b.minor, a.bugfix + b.bugfix, a.separator);
    }

    public string asString { get {
      var sb = new StringBuilder();
      sb.Append(major);
      sb.Append(separator);
      sb.Append(minor);
      if (bugfix != 0) {
        sb.Append(separator);
        sb.Append(bugfix);
      }
      return sb.ToString();
    } }
    string IStr.asString() => asString;

    public override string ToString() {
      var str = minor == 0 && bugfix == 0 ? $"{asString},sep={separator}" : asString;
      return $"{nameof(VersionNumber)}[{str}]";
    }

    public static Either<string, VersionNumber> parseString(string s, char separator=DEFAULT_SEPARATOR) {
      var errHeader = $"Can't parse '{s}' as version number with separator '{separator}'";
      var parts = s.Split(separator);
      if (parts.Length > 3) return $"{errHeader}: too many parts!";
      if (parts.isEmpty()) return $"{errHeader}: empty!";
      var majorE = parts[0].parseUInt().mapLeft(e => $"{errHeader} (major): {e}");
      var minorE = getIdx(parts, 1).mapLeft(e => $"{errHeader} (minor): {e}");
      var bugfixE = getIdx(parts, 2).mapLeft(e => $"{errHeader} (bugfix): {e}");
      return majorE.flatMapRight(major => minorE.flatMapRight(minor => bugfixE.mapRight(bugfix =>
        new VersionNumber(major, minor, bugfix, separator)
      )));
    }

    static Either<string, uint> getIdx(IReadOnlyList<string> parts, int idx) =>
      parts.get(idx).fold(0u, _ => _.parseUInt());
  }
}

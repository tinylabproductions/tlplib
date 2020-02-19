using System.Runtime.CompilerServices;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Conveniently packages data from <see cref="CallerMemberNameAttribute"/>, <see cref="CallerFilePathAttribute"/> and
  /// <see cref="CallerLineNumberAttribute"/>.
  /// </summary>
  [Record(GenerateToString = false), PublicAPI] public readonly partial struct CallerData : IStr {
    public readonly string memberName;
    public readonly string filePath;
    public readonly int lineNumber;

    public string asString() => $"{memberName} @ {filePath}:{lineNumber}";
    public override string ToString() => asString();
  }
}
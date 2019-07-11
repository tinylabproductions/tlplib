using com.tinylabproductions.TLPLib.Data.typeclasses;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false), PublicAPI] public readonly partial struct CallerData : IStr {
    public readonly string callerMemberName;
    public readonly string callerFilePath;
    public readonly int callerLineNumber;

    public string asString() => $"{callerMemberName} @ {callerFilePath}:{callerLineNumber}";
    public override string ToString() => asString();
  }
}
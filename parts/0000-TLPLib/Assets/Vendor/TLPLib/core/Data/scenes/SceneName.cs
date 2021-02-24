using GenerationAttributes;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Data.scenes {
  [Record] public readonly partial struct SceneName : IStr {
    public readonly string name;

    public string asString() => name;

    public static implicit operator string(SceneName s) => s.name;
  }
}
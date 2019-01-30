using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data {
  [Record] public partial struct TypedKey<Type> {
    public readonly string key;
  }
}
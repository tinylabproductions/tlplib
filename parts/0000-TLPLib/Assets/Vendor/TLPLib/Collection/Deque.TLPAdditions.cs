namespace com.tinylabproductions.TLPLib.Collection {
  public partial class Deque<T> {
    public ref T GetRef(int index) {
      checkIndexOutOfRange(index);
      return ref buffer[toBufferIndex(index)];
    }
  }
}
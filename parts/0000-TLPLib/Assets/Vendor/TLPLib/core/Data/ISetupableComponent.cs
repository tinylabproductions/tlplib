namespace com.tinylabproductions.TLPLib.Data {
  // ReSharper disable once TypeParameterCanBeVariant
  public interface ISetupableComponent<SetupData> {
    void setup(SetupData data);
  }
}
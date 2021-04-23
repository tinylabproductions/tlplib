using System;
using System.IO;
using System.Runtime.Serialization;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.serialization;


namespace com.tinylabproductions.TLPLib.Data {
  public interface IPrefValueWriter<in A> {
    void write(IPrefValueBackend backend, string key, A value);
  }

  public interface IPrefValueReader<A> {
    A read(IPrefValueBackend backend, string key, A defaultVal);
  }

  public interface IPrefValueRW<A> : IPrefValueReader<A>, IPrefValueWriter<A> {}

  public static class PrefValRW {
    public static readonly IPrefValueRW<string> str = new stringRW();
    public static readonly IPrefValueRW<Uri> uri = custom(SerializedRW.uri);
    public static readonly IPrefValueRW<int> integer = new intRW();
    public static readonly IPrefValueRW<uint> uinteger = new uintRW();
    public static readonly IPrefValueRW<float> flt = new floatRW();
    public static readonly IPrefValueRW<bool> boolean = new boolRW();
    public static readonly IPrefValueRW<Duration> duration = new DurationRW();
    public static readonly IPrefValueRW<DateTime> dateTime = new DateTimeRW();

    public static IPrefValueRW<A> custom<A>(
      Func<A, string> serialize, Func<string, Either<string, A>> deserialize,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => new CustomRW<A>(serialize, deserialize, onDeserializeFailure, log ?? Log.@default);

    public static IPrefValueRW<A> custom<A>(
      ISerializedRW<A> aRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) {
      var stream = new MemoryStream();
      return custom(
        a => Convert.ToBase64String(aRW.serializeToArray(a, stream)),
        s => {
          try {
            var bytes = Convert.FromBase64String(s);
            return aRW.deserialize(bytes).mapRight(_ => _.value);
          }
          catch (FormatException e) {
            return $"converting from base64 threw {e}";
          }
        },
        onDeserializeFailure,
        log
      );
    }

    public static IPrefValueRW<Option<A>> opt<A>(
      ISerializedRW<A> baRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => custom(
      SerializedRW.opt(baRW).mapNoFail(_ => _, _ => _),
      onDeserializeFailure, log
    );

    class stringRW : IPrefValueRW<string> {
      public string read(IPrefValueBackend backend, string key, string defaultVal) =>
        backend.getString(key, defaultVal);

      public void write(IPrefValueBackend backend, string key, string value) =>
        backend.setString(key, value);
    }

    class intRW : IPrefValueRW<int> {
      public int read(IPrefValueBackend backend, string key, int defaultVal) =>
        backend.getInt(key, defaultVal);

      public void write(IPrefValueBackend backend, string key, int value) =>
        backend.setInt(key, value);
    }

    class uintRW : IPrefValueRW<uint> {
      public uint read(IPrefValueBackend backend, string key, uint defaultVal) =>
        backend.getUInt(key, defaultVal);

      public void write(IPrefValueBackend backend, string key, uint value) =>
        backend.setUInt(key, value);
    }

    class floatRW : IPrefValueRW<float> {
      public float read(IPrefValueBackend backend, string key, float defaultVal) =>
        backend.getFloat(key, defaultVal);

      public void write(IPrefValueBackend backend, string key, float value) =>
        backend.setFloat(key, value);
    }

    class boolRW : IPrefValueRW<bool> {
      public bool read(IPrefValueBackend backend, string key, bool defaultVal) =>
        backend.getBool(key, defaultVal);

      public void write(IPrefValueBackend backend, string key, bool value) =>
        backend.setBool(key, value);
    }

    class DurationRW : IPrefValueRW<Duration> {
      public Duration read(IPrefValueBackend backend, string key, Duration defaultVal) =>
        new Duration(backend.getInt(key, defaultVal.millis));

      public void write(IPrefValueBackend backend, string key, Duration value) =>
        backend.setInt(key, value.millis);
    }

    class DateTimeRW : IPrefValueRW<DateTime> {
      public DateTime read(IPrefValueBackend backend, string key, DateTime defaultVal) =>
        deserializeDate(backend.getString(key, serializeDate(defaultVal)));

      public void write(IPrefValueBackend backend, string key, DateTime value) =>
        backend.setString(key, serializeDate(value));

      static string serializeDate(DateTime date) => date.ToBinary().ToString();
      static DateTime deserializeDate(string s) => DateTime.FromBinary(long.Parse(s));
    }

    class CustomRW<A> : IPrefValueRW<A> {
      const string DEFAULT_VALUE = "d", NON_DEFAULT_VALUE_DISCRIMINATOR = "_";

      readonly Func<A, string> serialize;
      readonly Func<string, Either<string, A>> deserialize;
      readonly PrefVal.OnDeserializeFailure onDeserializeFailure;
      readonly ILog log;

      public CustomRW(
        Func<A, string> serialize, Func<string, Either<string, A>> deserialize, 
        PrefVal.OnDeserializeFailure onDeserializeFailure, ILog log
      ) {
        this.serialize = serialize;
        this.deserialize = deserialize;
        this.onDeserializeFailure = onDeserializeFailure;
        this.log = log;
      }

      public A read(IPrefValueBackend backend, string key, A defaultVal) {
        var serialized = backend.getString(key, DEFAULT_VALUE);

        if (string.IsNullOrEmpty(serialized)) return deserializationFailed(
          key, defaultVal, "serialized data is empty", serialized
        );
        if (serialized == DEFAULT_VALUE) return defaultVal;

        var serializedWithoutDiscriminator = serialized.Substring(1);
        var either = deserialize(serializedWithoutDiscriminator);
        return 
          either.leftValueOut(out var error) 
            ? deserializationFailed(key, defaultVal, error, serialized) 
            : either.__unsafeGetRight;
      }

      A deserializationFailed(string key, A defaultVal, string error, string serialized) {
        if (onDeserializeFailure == PrefVal.OnDeserializeFailure.ReturnDefault) {
          if (log.isWarn()) log.warn(deserializeFailureMsg(key, error, serialized, ", returning default"));
          return defaultVal;
        }

        throw new SerializationException(deserializeFailureMsg(key, error, serialized));
      }

      public void write(IPrefValueBackend backend, string key, A value) =>
        backend.setString(key, $"{NON_DEFAULT_VALUE_DISCRIMINATOR}{serialize(value)}");

      static string deserializeFailureMsg(string key, string error, string serialized, string ending = "") =>
        $"Can't deserialize {typeof(A)} because of '{error}' from '{serialized}' for PrefVal '{key}'{ending}.";
    }
  }
}
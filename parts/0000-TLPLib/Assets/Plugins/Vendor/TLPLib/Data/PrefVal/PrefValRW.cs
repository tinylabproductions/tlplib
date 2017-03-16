using System;
using System.Runtime.Serialization;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

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

    public static IPrefValueRW<A> custom__OLD<A>(Fn<A, string> map, Fn<string, A> comap) => 
      new CustomOldRW<A>(map, comap);

    public static IPrefValueRW<A> custom<A>(
      Fn<A, string> serialize, Fn<string, Option<A>> deserialize,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => new CustomRW<A>(serialize, deserialize, onDeserializeFailure, log ?? Log.defaultLogger);

    public static IPrefValueRW<A> custom<A>(
      ISerializedRW<A> aRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => custom(
      a => Convert.ToBase64String(aRW.serialize(a).toArray()),
      s => {
        try {
          var bytes = Convert.FromBase64String(s);
          return aRW.deserialize(bytes, 0).map(_ => _.value);
        }
        catch (FormatException) {
          return Option<A>.None;
        }
      },
      onDeserializeFailure,
      log
    );

    public static IPrefValueRW<Option<A>> opt<A>(
      ISerializedRW<A> baRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => custom(SerializedRW.opt(baRW), onDeserializeFailure, log);

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

      readonly Fn<A, string> serialize;
      readonly Fn<string, Option<A>> deserialize;
      readonly PrefVal.OnDeserializeFailure onDeserializeFailure;
      readonly ILog log;

      public CustomRW(Fn<A, string> serialize, Fn<string, Option<A>> deserialize, PrefVal.OnDeserializeFailure onDeserializeFailure, ILog log) {
        this.serialize = serialize;
        this.deserialize = deserialize;
        this.onDeserializeFailure = onDeserializeFailure;
        this.log = log;
      }

      public A read(IPrefValueBackend backend, string key, A defaultVal) {
        var serialized = backend.getString(key, DEFAULT_VALUE);

        if (string.IsNullOrEmpty(serialized)) return deserializationFailed(key, defaultVal, serialized);
        if (serialized == DEFAULT_VALUE) return defaultVal;

        var serializedWithoutDiscriminator = serialized.Substring(1);
        var opt = deserialize(serializedWithoutDiscriminator);
        return opt.isDefined ? opt.get : deserializationFailed(key, defaultVal, serialized);
      }

      A deserializationFailed(string key, A defaultVal, string serialized) {
        if (onDeserializeFailure == PrefVal.OnDeserializeFailure.ReturnDefault) {
          if (log.isWarn()) log.warn(deserializeFailureMsg(key, serialized, ", returning default"));
          return defaultVal;
        }

        throw new SerializationException(deserializeFailureMsg(key, serialized));
      }

      public void write(IPrefValueBackend backend, string key, A value) =>
        backend.setString(key, $"{NON_DEFAULT_VALUE_DISCRIMINATOR}{serialize(value)}");

      static string deserializeFailureMsg(string key, string serialized, string ending = "") =>
        $"Can't deserialize {typeof(A)} from '{serialized}' for PrefVal '{key}'{ending}.";
    }
    
    class CustomOldRW<A> : IPrefValueRW<A> {
      /* If you store this as a value in type custom PrefValue, you'll get back a default value. */
      const string CUSTOM_V1_DEFAULT = "";

      readonly Fn<A, string> map;
      readonly Fn<string, A> comap;

      public CustomOldRW(Fn<A, string> map, Fn<string, A> comap) {
        this.map = map;
        this.comap = comap;
      }

      public A read(IPrefValueBackend backend, string key, A defaultVal) {
        var str = backend.getString(key, CUSTOM_V1_DEFAULT);
        return str == CUSTOM_V1_DEFAULT ? defaultVal : comap(str);
      }

      public void write(IPrefValueBackend backend, string key, A value) =>
        backend.setString(key, map(value));
    }
  }
}
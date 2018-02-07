using System;
using System.Reflection;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.reflection {
  public static class PrivateField {
    public static Fn<ObjectType, FieldType> getter<ObjectType, FieldType>(string fieldName) =>
      a => accessor<ObjectType, FieldType>(fieldName)(a).value;

    public static Fn<ObjectType, Ref<FieldType>> accessor<ObjectType, FieldType>(string fieldName) {
      var type = typeof(ObjectType);
      var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (fieldInfo == null) throw  new ArgumentException(
        $"Type {type} does not have non public instance field '{fieldName}'!"
      );

      return a => new LambdaRef<FieldType>(
        () => (FieldType) fieldInfo.GetValue(a),
        valueToSet => fieldInfo.SetValue(a, valueToSet)
      );
    }
  }

  public static class PrivateConstructor {
    public static Fn<object[], A> creator<A>() {
      var type = typeof(A);
      return args => (A) type.Assembly.CreateInstance(
          type.FullName, false,
          BindingFlags.Instance | BindingFlags.NonPublic,
          null, args, null, null
      );
    }
  }
}
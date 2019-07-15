
using pzd.lib.functional;
#if UNITY_EDITOR
using System.Collections;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;
namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SerializedPropertyExts {
    public static Option<object> findFieldValueInObject(this SerializedProperty sp, object obj) {
      var path = sp.propertyPath.Split('.');

      var firstFieldName = path[0];
      var currentOpt = getFieldValue(obj, firstFieldName);

      var currentSp = sp.serializedObject.FindProperty(firstFieldName);
      for (var idx = 1; idx < path.Length; idx++) {
        if (currentOpt.isNone) return None._;
        var current = currentOpt.__unsafeGet;
        var currentFieldName = path[idx];
        if (currentSp.isArray) {
          // if we have an array named myArray,
          // then its first element will be serialized as myArray.Array.data[0]
          idx++;
          var nextFieldName = path[idx];
          currentSp = currentSp
            .FindPropertyRelative(currentFieldName) // Array
            .FindPropertyRelative(nextFieldName);   // data[arrayIndex]
          var arrayIndex = parseArrayIndex(nextFieldName);
          currentOpt = F.opt((current as IList)[arrayIndex]);
        }
        else {
          // non-array-element field
          currentOpt = getFieldValue(current, currentFieldName);
          currentSp = currentSp.FindPropertyRelative(currentFieldName);
        }
      }
      return currentOpt;
    }

    static Option<object> getFieldValue(object obj, string fieldName) =>
      obj.GetType().GetFieldInHierarchy(fieldName).flatMap(fi => F.opt(fi.GetValue(obj)));

    /// <summary> Parses array idx from property field name ("data[5]" -> 5) </summary>
    static int parseArrayIndex(string propertyFieldName) {
      var startIndex = propertyFieldName.LastIndexOf('[') + 1;
      var length = propertyFieldName.LastIndexOf(']') - startIndex;
      return int.Parse(propertyFieldName.Substring(startIndex, length));
    }
  }
}
#endif

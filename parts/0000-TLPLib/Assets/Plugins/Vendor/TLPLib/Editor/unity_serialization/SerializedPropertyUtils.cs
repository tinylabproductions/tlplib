using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  public static class SerializedPropertyUtils {
    
   public static Option<SerializedProperty> getValueProp(SerializedProperty property, string propName) =>
      property.FindPropertyRelative(propName).opt();
    
  }
}
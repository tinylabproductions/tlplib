using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public static class SerializedPropertyExts {
    [PublicAPI] public static FieldInfo GetFieldInfo(this SerializedProperty property) {
      var parentType = property.serializedObject.targetObject.GetType();
      var field = parentType.GetField(
        property.propertyPath, 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
      );
      return field;
    }

    [PublicAPI] public static object GetObject(this SerializedProperty property) {
      var field = property.GetFieldInfo();
      return field.GetValue(property.serializedObject.targetObject);
    }
    
    [PublicAPI] public static Type getSerializedObjectType(this SerializedProperty property) => 
      property.GetFieldInfo().FieldType;

    [PublicAPI]
    public static void setToDefaultValue(this SerializedProperty property) {
      switch (property.propertyType) {
        case SerializedPropertyType.Character:
        case SerializedPropertyType.Integer:
        case SerializedPropertyType.LayerMask:
          property.intValue = default;
          break;
        case SerializedPropertyType.Boolean:
          property.boolValue = default;
          break;
        case SerializedPropertyType.Float:
          property.floatValue = default;
          break;
        case SerializedPropertyType.String:
          property.stringValue = default;
          break;
        case SerializedPropertyType.Color:
          property.colorValue = default;
          break;
        case SerializedPropertyType.ObjectReference:
        case SerializedPropertyType.AnimationCurve:
        case SerializedPropertyType.ExposedReference:
        case SerializedPropertyType.Gradient:
          property.objectReferenceValue = default;
          break;
        case SerializedPropertyType.Bounds:
          property.boundsValue = default;
          break;
        case SerializedPropertyType.Enum:
          property.enumValueIndex = default;
          break;
        case SerializedPropertyType.Vector2:
          property.vector2Value = default;
          break;
        case SerializedPropertyType.Vector3:
          property.vector3Value = default;
          break;
        case SerializedPropertyType.Vector4:
          property.vector4Value = default;
          break;
        case SerializedPropertyType.Rect:
          property.rectValue = default;
          break;
        case SerializedPropertyType.ArraySize:
          property.rectValue = default;
          break;
        case SerializedPropertyType.Quaternion:
          property.quaternionValue = default;
          break;
#if UNITY_2017_2_OR_NEWER
        case SerializedPropertyType.Vector2Int:
          property.vector2IntValue = default;
          break;
        case SerializedPropertyType.Vector3Int:
          property.vector3IntValue = default;
          break;
        case SerializedPropertyType.RectInt:
          property.rectIntValue = default;
          break;
        case SerializedPropertyType.BoundsInt:
          property.boundsIntValue = default;
          break;
#endif
        case SerializedPropertyType.Generic:
#if UNITY_2017_2_OR_NEWER
        case SerializedPropertyType.FixedBufferSize:
#endif
          throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, "Unknown type");
        default:
          throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, "Unknown type");
      }
    }
  }
}
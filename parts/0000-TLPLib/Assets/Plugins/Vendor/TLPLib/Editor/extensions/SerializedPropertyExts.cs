using System;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public static class SerializedPropertyExts {
    [PublicAPI] public static object GetObject(this SerializedProperty property) {
      // property.propertyPath can be something like 'foo.bar.baz'
      var path = property.propertyPath.Split('.');
      object @object = property.serializedObject.targetObject;
      for (var idx = 0; idx < path.Length; idx++) {
        var part = path[idx];
        if (@object.GetType().GetFieldInHierarchy(part).valueOut(out var field)) {
          if (idx == path.Length - 1) {
            return field.GetValue(@object);
          }
          else {
            @object = field.GetValue(@object);
          }
        }
        else {
          throw new ArgumentException($"Can't find property '{part}' in {@object.GetType()} ({@object})!");
        }
      }
      throw new ArgumentException(
        $"Can't find property with path '{property.propertyPath}' in " +
        $"{property.serializedObject.targetObject.GetType()}!"
      );
    }
    
    [PublicAPI]
    public static void setToDefaultValue(this SerializedProperty property) {
      ArgumentException exception(string extra = null) => new ArgumentOutOfRangeException(
        $"Unknown property type '{property.propertyType}' for variable {property.propertyPath} " +
        $"in {property.serializedObject.targetObject.GetType()}" + (extra ?? "")
      );

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
          throw exception();
        default:
          throw exception();
      }
    }
  }
}
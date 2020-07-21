using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  public static partial class SerializedPropertyExts {
    [Record] partial struct GetObjectData {
      public readonly Either<string, int> fieldNameOrArrayIndex;

      public Option<object> get(object source) {
        if (fieldNameOrArrayIndex.leftValueOut(out var fieldName)) {
          return source.GetType().GetFieldInHierarchy(fieldName).map(field => field.GetValue(source));
        }
        else {
          var arrayIndex = fieldNameOrArrayIndex.__unsafeGetRight;
          return F.some(source.cast().to<Array>().GetValue(arrayIndex));
        }
      }
    }
    
    static readonly Regex ARRAY_PART_RE = new Regex(@"\[(\d+)\]$");
    
    [PublicAPI] public static object GetObject(this SerializedProperty property) {
      // property.propertyPath can be something like 'foo.bar.baz'
      var path = property.propertyPath.Split('.');
      object @object = property.serializedObject.targetObject;
      for (var idx = 0; idx < path.Length; idx++) {
        var part = path[idx];
        
        GetObjectData getObjectData() {
          // unity encodes arrays like this: 'foobar.Array.data[1]' means foobar[1].
          if (part == "Array") {
            var arrayPart = path[idx + 1];
            idx++;
            var match = ARRAY_PART_RE.Match(arrayPart);
            var arrayIndex = match.Groups[1].Value.parseInt().rightOrThrow;
            return new GetObjectData(arrayIndex);
          }
          else {
            return new GetObjectData(part);
          }
        }

        var objData = getObjectData();
        if (objData.get(@object).valueOut(out var fieldValue)) {
          if (idx == path.Length - 1) {
            return fieldValue;
          }
          else {
            @object = fieldValue;
          }
        }
        else {
          throw new ArgumentException(
            $"Can't find property '{objData}' from path '{property.propertyPath}' " +
            $"in {@object.GetType()} ({@object})!"
          );
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
          property.arraySize = default;
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
          // Generic means a serializable data structure. We need to traverse it and set default
          // for all entries.
          foreach (var child in property.GetImmediateChildren()) {
            child.setToDefaultValue();
          }
          break;
#if UNITY_2017_2_OR_NEWER
        case SerializedPropertyType.FixedBufferSize:
#endif
          throw exception();
        default:
          throw exception();
      }
    }
    
    [PublicAPI]
    public static bool next(this SerializedProperty sp, bool enterChildren, bool onlyVisible = true) =>
      onlyVisible ? sp.NextVisible(enterChildren) : sp.Next(enterChildren);

    [PublicAPI]
    public static IEnumerable<SerializedProperty> GetImmediateChildren(
      this SerializedProperty property, bool onlyVisible = true
    ) {
      // https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/#post-2814895
      property = property.Copy();
      var nextElement = property.Copy();
      var hasNextElement = nextElement.NextVisible(false);
      if (!hasNextElement) {
        nextElement = null;
      }

      property.NextVisible(true);
      while (true) {
        if (SerializedProperty.EqualContents(property, nextElement)) {
          yield break;
        }

        yield return property;

        var hasNext = property.NextVisible(false);
        if (!hasNext) {
          break;
        }
      }
    }

    [PublicAPI]
    public static string debugStr(this SerializedProperty p) =>
      $"SerializedProperty[{p.propertyType} @ '{p.propertyPath}']";

    [PublicAPI]
    public static void drawInspector(this SerializedProperty property, bool onlyVisible = true) {
      foreach (var child in property.GetImmediateChildren(onlyVisible: onlyVisible)) {
        EditorGUILayout.PropertyField(child, includeChildren: true);
      }

      var obj = property.serializedObject;
      obj.ApplyModifiedProperties();
    }
  }
}
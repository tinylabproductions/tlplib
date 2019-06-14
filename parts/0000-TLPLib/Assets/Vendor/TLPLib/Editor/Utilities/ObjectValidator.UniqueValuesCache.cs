using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    public partial class UniqueValuesCache {
      [Record]
      public partial class CheckedField {
        public readonly string category;
        public readonly object fieldValue;
        public readonly Object checkedObject;
      }

      [Record]
      public partial class DuplicateField {
        public readonly string category;
        public readonly object fieldValue;
        public readonly ImmutableList<Object> objectsWithThisValue;
      }

      public static UniqueValuesCache create => new UniqueValuesCache();

      public static readonly IEqualityComparer<object> comparer = new Eql.Lambda<object>((o1, o2) => {
        var o1Null = o1 == null;
        var o2Null = o2 == null;
        if (o1Null && o2Null) return true;
        if (o1Null || o2Null) return false;

        var t1 = o1.GetType();
        var t2 = o2.GetType();
        if (t1 != t2) return false;

        bool sequenceEquals(IEnumerable _e1, IEnumerable _e2) {
          var enumerator1 = _e1.GetEnumerator();
          var enumerator2 = _e2.GetEnumerator();
          while (enumerator1.MoveNext()) {
            if (!enumerator2.MoveNext() || !comparer.Equals(enumerator1.Current, enumerator2.Current))
              return false;
          }
          return !enumerator2.MoveNext();
        }

        if (t1.IsValueType) {
          // Compare all fields of a struct to see if they are equal.
          // Can't use ==, because == checks for object reference equality.
          return o1.Equals(o2);
        }
        if (o1 is IEnumerable e1 && o2 is IEnumerable e2) {
          return sequenceEquals(e1, e2);
        }
        return o1.Equals(o2);
      }).asEqualityComparer();
      
      private readonly List<CheckedField> checkedFields = new List<CheckedField>();

      public IEnumerable<DuplicateField> getDuplicateFields() {
        Dictionary<Key, List<Value>> groupByDictionary<Key, Value>(
          IEnumerable<Value> enumerable,
          Func<Value, Key> keySelector,
          IEqualityComparer<Key> comparer
        ) {
          var dict = new Dictionary<Key, List<Value>>();
          foreach (var value in enumerable) {
            var key = keySelector(value);
            var opt = dict.Keys.find(_ => _, key, comparer);
            if (opt.valueOut(out var k)) {
              dict[k].Add(value);
            }
            else {
              dict.Add(key, new List<Value> { value });
            }
          }
      
          return dict;
        }
        
        var categories = checkedFields.GroupBy(f => f.category);
        return categories.SelectMany(category => {
          var categoryName = category.Key;
          
          // var grouped = category.GroupBy(_ => _.fieldValue, comparer);
          // Changed into dictionary because of .GroupBy bug - it is not using comparer
          var grouped = groupByDictionary(category, _ => _.fieldValue, comparer);
          var duplicateFields = grouped.Where(_ => _.Value.Count > 1);
          return duplicateFields.Select(fieldToObjects => new DuplicateField(
            categoryName, fieldToObjects.Key, fieldToObjects.Value.Select(_ => _.checkedObject).ToImmutableList()
          ));
        });
      }
      
      public void addCheckedField(string category, object identifier, Object unityObject) {
        if (checkedFields.All(_ => _.fieldValue != identifier))
          checkedFields.Add(new CheckedField(category, identifier, unityObject));
      }
    }
  }
}
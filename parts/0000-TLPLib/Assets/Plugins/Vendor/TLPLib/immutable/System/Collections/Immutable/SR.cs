namespace System.Collections.Immutable {
  public static class SR {
    public const string
      InvalidEmptyOperation = "InvalidEmptyOperation",
      CannotFindOldValue = "CannotFindOldValue",
      ArrayInitializedStateNotEqual = "ArrayInitializedStateNotEqual",
      ArrayLengthsNotEqual = "ArrayLengthsNotEqual",
      CollectionModifiedDuringEnumeration = "CollectionModifiedDuringEnumeration",
      Arg_KeyNotFoundWithKey = "Arg_KeyNotFoundWithKey {0}",
      DuplicateKey = "Duplicate Key",
      InvalidOperationOnDefaultArray = "InvalidOperationOnDefaultArray",
      CapacityMustBeGreaterThanOrEqualToCount = "CapacityMustBeGreaterThanOrEqualToCount",
      CapacityMustEqualCountOnMove = "CapacityMustEqualCountOnMove"
      ;

    public static string Format(string s, params object[] p) =>
      string.Format(s, p);
  }
}
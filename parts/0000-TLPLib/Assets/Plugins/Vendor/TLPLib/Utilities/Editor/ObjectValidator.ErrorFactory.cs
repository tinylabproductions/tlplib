using System;
using com.tinylabproductions.TLPLib.Data;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    interface IErrorFactory {
      Error nullField(FieldHierarchyStr hierarchy);
      Error emptyCollection(FieldHierarchyStr hierarchy);
      Error emptyString(FieldHierarchyStr hierarchy);
      Error badTextFieldTag(FieldHierarchyStr hierarchy);
      Error unityEventInvalid(FieldHierarchyStr hierarchy, int index);
      Error unityEventInvalidMethod(FieldHierarchyStr hierarchy, int index);
      Error exceptionInCustomValidator(FieldHierarchyStr hierarchy, Exception exception);
      Error custom(FieldHierarchyStr hierarchy, ErrorMsg customErrorMessage, bool useErrorMessageContext);
    }

    class ErrorFactory : IErrorFactory {
      readonly Object component;
      readonly CheckContext context;

      public ErrorFactory(Object component, CheckContext context) {
        this.component = component;
        this.context = context;
      }

      public Error nullField(FieldHierarchyStr hierarchy) =>
        Error.nullReference(o: component, hierarchy: hierarchy, context: context);

      public Error emptyCollection(FieldHierarchyStr hierarchy) =>
        Error.emptyCollection(o: component, hierarchy: hierarchy, context: context);

      public Error emptyString(FieldHierarchyStr hierarchy) =>
        Error.emptyString(o: component, hierarchy: hierarchy, context: context);

      public Error badTextFieldTag(FieldHierarchyStr hierarchy) =>
        Error.badTextFieldTag(o: component, hierarchy: hierarchy, context: context);

      public Error unityEventInvalid(FieldHierarchyStr hierarchy, int index) =>
        Error.unityEventInvalid(o: component, hierarchy: hierarchy, index: index, context: context);

      public Error unityEventInvalidMethod(FieldHierarchyStr hierarchy, int index) =>
        Error.unityEventInvalidMethod(o: component, hierarchy: hierarchy, index: index, context: context);

      public Error exceptionInCustomValidator(FieldHierarchyStr hierarchy, Exception exception) =>
        Error.customValidationException(o: component, hierarchy: hierarchy, exception: exception, context: context);

      public Error custom(FieldHierarchyStr hierarchy, ErrorMsg customErrorMessage, bool useErrorMessageContext) =>
        Error.customError(o: component, hierarchy: hierarchy, error: customErrorMessage, context: context, useErrorMessageContext);
    }
  }
}
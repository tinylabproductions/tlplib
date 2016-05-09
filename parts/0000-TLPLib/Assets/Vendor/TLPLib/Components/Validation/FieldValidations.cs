using System;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Components.Validation {
  public static class FieldValidations {
    public static HelpItem strHelp(Fn<string> get, int maxLength, bool isRequired) {
      var value = get() ?? "";
      if (isRequired && value.Trim().isEmpty())
        return new HelpItem(HelpType.Error, "This field is required.");
      else if (value.Length >= maxLength)
        return new HelpItem(HelpType.Info, $"Max length = {maxLength}");
      else
        return new HelpItem(HelpType.None, "");
    }
  }
}

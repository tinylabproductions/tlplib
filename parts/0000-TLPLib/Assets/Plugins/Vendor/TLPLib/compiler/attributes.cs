// If you are using a recent C# compiler, having these attributes allows the compiler
// to insert caller info at the compile time.
namespace System.Runtime.CompilerServices {
  /// <summary>Allows you to obtain the method or property name of the caller to the method.</summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public sealed class CallerMemberNameAttribute : Attribute {}

  /// <summary>Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public sealed class CallerFilePathAttribute : Attribute {}

  /// <summary>Allows you to obtain the line number in the source file at which the method is called.</summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public sealed class CallerLineNumberAttribute : Attribute {}
}
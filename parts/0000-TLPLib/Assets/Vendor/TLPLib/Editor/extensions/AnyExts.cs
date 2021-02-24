using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  [PublicAPI] public static class AnyExts {
    /// <summary>Returns an expression as a nice variable name using the Unity editor API.</summary>
    [SimpleMethodMacro("UnityEditor.ObjectNames.NicifyVariableName(nameof(${a}))")]
    public static string niceName<A>(this A a) => throw new MacroException();
  }
}
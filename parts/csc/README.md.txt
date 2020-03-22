Custom build C# compiler from https://github.com/arturaz/roslyn that allows hooking up
into the build process.

You can add your extensions by putting them in this folder with a name `CompilationExtension*.dll`.

That DLL needs to have public classes that have a parameterless constructor and implements `CompilationExtensionInterfaces.IProcessCompilation` interface.

You will also need to reference the `Microsoft.CodeAnalysis.Common` package from NuGET.

For example:
```
using System;
using System.Collections.Generic;
using System.Linq;
using CompilationExtensionInterfaces;
using Microsoft.CodeAnalysis;

namespace TestCompilationExtension {
  public class Extension : IProcessCompilation {
    public IEnumerable<object> process(ref object compilation) {
      var comp = (Compilation) compilation;
      compilation = comp;
      Console.WriteLine($"Hello from {comp}!");
      return Enumerable.Empty<Diagnostic>();
    }
  }
}
```
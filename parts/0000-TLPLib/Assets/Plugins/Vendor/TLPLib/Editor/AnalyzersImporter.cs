using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace AnalyzersImporter {
  /// <summary>
  /// Class for supporting Visual Studio analyzers
  /// Imports all dll's from `analyzers` folder
  /// </summary>
  [InitializeOnLoad]
  public class AnalyzersImporter : AssetPostprocessor {

    static AnalyzersImporter() {
      // Most code is from here:
      // https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/src/4cd7615febbe3456aa89b31c5b4f165de75618d3/CSharp%20vNext%20Support%20Solution/CSharpVNextSupport/CSharpProjectPostprocessor.cs?at=default&fileviewer=file-view-default
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("SyntaxTree.VisualStudio.Unity.Bridge") == false) {
          continue;
        }

        var projectFilesGeneratorType = assembly.GetType("SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator");
        if (projectFilesGeneratorType == null) {
          Debug.Log("Type 'SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator' not found");
          return;
        }

        var delegateType = assembly.GetType("SyntaxTree.VisualStudio.Unity.Bridge.FileGenerationHandler");
        if (delegateType == null) {
          Debug.Log("Type 'SyntaxTree.VisualStudio.Unity.Bridge.FileGenerationHandler' not found");
          return;
        }

        var projectFileGenerationField = projectFilesGeneratorType.GetField("ProjectFileGeneration", BindingFlags.Static | BindingFlags.Public);
        if (projectFileGenerationField == null) {
          Debug.Log("Field 'ProjectFileGeneration' not found");
          return;
        }

        var handlerMethodInfo = typeof(AnalyzersImporter).GetMethod(nameof(modifyProjectFile), BindingFlags.Static | BindingFlags.NonPublic);
        var handlerDelegate = Delegate.CreateDelegate(delegateType, null, handlerMethodInfo);

        var delegateValue = (Delegate)projectFileGenerationField.GetValue(null);
        delegateValue = delegateValue == null ? handlerDelegate : Delegate.Combine(delegateValue, handlerDelegate);
        projectFileGenerationField.SetValue(null, delegateValue);

        return;
      }
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    // ReSharper disable once UnusedParameter.Local
    static string modifyProjectFile(string name, string content) =>
      write(setup(content));

    // This is our actual code
    static XDocument setup(string content) {
      var xdoc = XDocument.Parse(content);
      var ns = xdoc.Root.GetDefaultNamespace();
      var project = xdoc.Descendants(ns + "Project").First();
      var dllPaths = findAllCustomDlls();
      project.Add(includeMultipleDlls(ns, dllPaths));

      return xdoc;
    }

    static XElement includeMultipleDlls(XNamespace ns, IEnumerable<string> paths) {
      var container = new XElement(ns + "ItemGroup");
      foreach (var dllPath in paths) {
        container.Add(new XElement(ns + "Analyzer", new XAttribute("Include", dllPath)));
      }
      return container;
    }

    static IEnumerable<string> findAllCustomDlls() =>
      !Directory.Exists("analyzers")
      ? Enumerable.Empty<string>()
      : Directory.GetFiles("analyzers", "*.dll", SearchOption.AllDirectories);

    static string write(XDocument document) {
      var r = new StringWriter().Encoding;
      var writer = new Utf8StringWriter();
      document.Save(writer);
      return writer.ToString();
    }

    class Utf8StringWriter : StringWriter {
      public override Encoding Encoding => Encoding.UTF8;
    }
  }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger.Reporting {
  public class AirbrakeAPI {
    public struct AirbrakeXML {
      public readonly XmlDocument doc;

      public AirbrakeXML(XmlDocument doc) { this.doc = doc; }

      public Future<WWW> send(string reportingUrl) {
        var doc = this.doc;
        return ASync.oneAtATimeWWW(() => new WWW(
          reportingUrl, Encoding.UTF8.GetBytes(doc.OuterXml), 
          new Dictionary<string, string> {
            {"Content-Type", "text/xml"}
          }
        ));
      }
    }

    public static ErrorReporter.OnError createOnError(
      string reportingUrl, string apiKey, string appVersion
    ) {
      return ((type, message, backtrace) => 
        xml(apiKey, appVersion, type, message, backtrace).send(reportingUrl)
      );
    }

    /// <param name="apiKey">Airbrake API key</param>
    /// <param name="appVersion">Version of your application</param>
    /// <param name="errorType">Error type from Unity API</param>
    /// <param name="message">Error message</param>
    /// <param name="backtrace">Error backtrace if it is available</param>
    public static AirbrakeXML xml(
      string apiKey, string appVersion,
      LogType errorType, string message, Option<string> backtrace
    ) {
      var doc = new XmlDocument();
      var dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      doc.AppendChild(dec);

      // Required. The version of the API being used. Should be set to "2.3"
      var root = doc.CreateElement("notice");
      root.SetAttribute("version", "2.3");

      // Required. The API key for the project that this error belongs to. 
      // The API key can be found by viewing the edit project form on the Airbrake site.
      root.AppendChild(doc.textElem("api-key", apiKey));

      root.AppendChild(doc.CreateElement("notifier")).tap(notifier => {
        notifier.AppendChild(doc.textElem("name", "tlplib"));
        notifier.AppendChild(doc.textElem("version", "1.0"));
        notifier.AppendChild(doc.textElem("url", "https://github.com/tinylabproductions/tlplib"));
      });

      root.AppendChild(doc.CreateElement("server-environment").tap(env => {
        env.AppendChild(doc.textElem(
          "environment-name", Debug.isDebugBuild ? "debug" : "production"
        ));
        env.AppendChild(doc.textElem("app-version", appVersion));
      }));

      root.AppendChild(doc.CreateElement("error").tap(err => {
        err.AppendChild(doc.textElem("class", errorType.ToString()));
        err.AppendChild(doc.textElem("message", message));
        err.AppendChild(doc.CreateElement("backtrace").tap(xmlBt => {
          var btElems = backtrace.map(_ => parseBacktrace(doc, _)).fold(
            () => F.list(backtraceElem(doc, "no-file", 0, F.none<string>())),
            _ => _
          );
          foreach (var btElem in btElems) xmlBt.AppendChild(btElem);
        }));
      }));

      root.AppendChild(doc.CreateElement("request").tap(req => {
        req.AppendChild(doc.textElem("url", ""));
        req.AppendChild(doc.textElem("component", ""));
      }));

      return new AirbrakeXML(doc);
    }

    /// <summary>
    /// Parses the back trace string from an exception into an XML document.
    /// </summary>
    /// <param name="doc">The document namespace context.</param>
    /// <param name="stack">The stack trace from the application.</param>
    /// <returns>A list of properly formatted XML objects that represent the stack trace for use in an XML document.</returns>
    public static List<XmlElement> parseBacktrace(XmlDocument doc, string stack) {
      var lines = Regex.Split(stack, "\r\n");

      return (
        from line in lines
        let number = regexStringConversion(
          Regex.Match(line, @":line \d+").Value.Replace(":line", string.Empty),
          "0"
        ).parseInt().rightValue.getOrElse(-1)
        let file = regexStringConversion(
          Regex.Match(line, @"in (.*):").Value.Replace("in ", string.Empty).Replace(":", string.Empty),
          "unknown-file"
        )
        let methodS = Regex.Match(line, @"at .*\)").Value.Replace("at ", string.Empty)
        let method = (!string.IsNullOrEmpty(methodS)).opt(methodS)
        select backtraceElem(doc, file, number, method)
      ).ToList();
    }

    public static string regexStringConversion(string str, string defaultString="unknown") {
      return string.IsNullOrEmpty(str) ? defaultString : str;
    }

    public static XmlElement backtraceElem(XmlDocument doc, string file, int lineNumber, Option<string> method) {
      var l = doc.CreateElement("line");
      l.SetAttribute("file", file);
      l.SetAttribute("number", lineNumber.ToString());
      method.each(m => l.SetAttribute("method", m));
      return l;
    }
  }
}

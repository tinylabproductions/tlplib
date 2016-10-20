using System.Collections.Generic;
using System.Text;
using System.Xml;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger.Reporting {
  public static class AirbrakeAPI {
    public struct AirbrakeXML {
      public readonly XmlDocument doc;

      public AirbrakeXML(XmlDocument doc) { this.doc = doc; }

      public WWW send(string reportingUrl) {
        var headers = new Dictionary<string, string> {{"Content-Type", "text/xml"}};
        var www = new WWW(reportingUrl, Encoding.UTF8.GetBytes(doc.OuterXml), headers);
        ErrorReporter.trackWWWSend("Airbrake API", www, headers);
        return www;
      }

      public override string ToString() { return string.Format("AirbrakeXML[\n{0}\n]", doc.OuterXml); }
    }

    public static ErrorReporter.OnError createOnError(
      string reportingUrl, string apiKey, ErrorReporter.AppInfo appInfo
    ) {
      return (data => xml(apiKey, appInfo, data).send(reportingUrl));
    }

    public static ErrorReporter.OnError createEditorOnError(
      string apiKey, ErrorReporter.AppInfo appInfo
    ) {
      return (data => ASync.NextFrame(() => {
        if (Log.isInfo) Log.info("Airbrake error:\n\n" + data + "\n" + xml(apiKey, appInfo, data));
      }));
    }

    public static AirbrakeXML xml(
      string apiKey, ErrorReporter.AppInfo appInfo, ErrorReporter.ErrorData data
    ) {
      var doc = new XmlDocument();
      var dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      doc.AppendChild(dec);

      // Required. The version of the API being used. Should be set to "2.3"
      var root = doc.CreateElement("notice");
      doc.AppendChild(root);
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
        env.AppendChild(doc.textElem("app-version", appInfo.bundleVersion));
      }));

      root.AppendChild(doc.CreateElement("error").tap(err => {
        err.AppendChild(doc.textElem("class", data.errorType.ToString()));
        err.AppendChild(doc.textElem("message", data.message));
        err.AppendChild(doc.CreateElement("backtrace").tap(xmlBt => {
          foreach (var btElem in data.backtrace) xmlBt.AppendChild(doc.backtraceElem(btElem));
        }));
      }));

      root.AppendChild(doc.CreateElement("request").tap(req => {
        req.AppendChild(doc.textElem("url", appInfo.bundleIdentifier));
        req.AppendChild(doc.textElem("component", ""));
      }));

      return new AirbrakeXML(doc);
    }
    
    public static XmlElement backtraceElem(this XmlDocument doc, BacktraceElem elem) {
      return backtraceElem(
        doc,
        elem.fileInfo.fold("unknown-file", fi => fi.file),
        elem.fileInfo.fold("-1", fi => fi.lineNo.ToString()),
        elem.method
      );
    }

    public static XmlElement backtraceElem(XmlDocument doc, string file, string lineNumber, string method) {
      var l = doc.CreateElement("line");
      l.SetAttribute("file", file);
      l.SetAttribute("number", lineNumber);
      l.SetAttribute("method", method);
      return l;
    }
  }
}

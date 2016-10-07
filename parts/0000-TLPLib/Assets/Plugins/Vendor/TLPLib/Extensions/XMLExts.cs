using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class XMLExts {
    public static XmlElement setInnerText(this XmlElement xml, string text) {
      xml.InnerText = text;
      return xml;
    }

    public static XmlElement textElem(this XmlDocument doc, string name, string text)
    { return doc.CreateElement(name).setInnerText(text); }
  }
}

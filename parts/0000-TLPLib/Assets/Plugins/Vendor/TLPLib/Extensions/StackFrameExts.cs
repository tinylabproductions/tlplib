using System.Diagnostics;
using System.Reflection;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class StackFrameExts {
    public static BacktraceElem toBacktraceElem(this StackFrame frame) {
      // TODO: we can optimize this to make less garbage
      // we could reuse StringBuilder in StackFrameExts.methodString
      // but I am not sure if the impact would be noticeable

      var declaringClass = frame.declaringClassString();
      var method = frame.methodString();
      return new BacktraceElem(
        $"{declaringClass}:{method}",
        frame.GetFileLineNumber() == 0
          ? F.none<BacktraceElem.FileInfo>()
          : F.some(new BacktraceElem.FileInfo(frame.GetFileName(), frame.GetFileLineNumber()))
      );
    }

    public static string fileAndLine(this StackFrame f) =>
      $"{f.GetFileName()}:{f.GetFileLineNumber()}";

    public static string declaringClassString(this StackFrame sf) {
      var mb = sf.GetMethod();
      if (mb == null) return "-";
      var t = mb.DeclaringType;
      // if there is a type (non global method) print it
      if (t == null) return "-";

      return t.FullName.Replace('+', '.');
    }

    // Copied from StackTrace.ToString decompiled source
    public static string methodString(this StackFrame sf) {
      var sb = new StringBuilder();
      var mb = sf.GetMethod();
      if (mb != null) {
        sb.Append(mb.Name);

        // deal with the generic portion of the method 
        var info = mb as MethodInfo;
        if (info != null && info.IsGenericMethod) {
          var typars = info.GetGenericArguments();
          sb.Append("[");
          var k = 0;
          var fFirstTyParam = true;
          while (k < typars.Length) {
            if (fFirstTyParam == false)
              sb.Append(",");
            else
              fFirstTyParam = false;

            sb.Append(typars[k].Name);
            k++;
          }
          sb.Append("]");
        }

        // arguments printing
        sb.Append("(");
        var pi = mb.GetParameters();
        var fFirstParam = true;
        for (var j = 0; j < pi.Length; j++) {
          if (fFirstParam == false)
            sb.Append(", ");
          else
            fFirstParam = false;

          var typeName = "<UnknownType>";
          if (pi[j].ParameterType != null)
            typeName = pi[j].ParameterType.Name;
          sb.Append(typeName);
          sb.Append(' ');
          sb.Append(pi[j].Name);
        }
        sb.Append(")");
      }
      return sb.ToString();
    }
  }
}
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public abstract class CodePreprocessorTestBase {
    public const string CODE =
      @"#if PART_UNITYADS
#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Test; 
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    // Code here
  }
}";

    public const string PRAG_STR = "#pragma warning disable\n";
    
    public const string CODE_WITH_PRAGMA =
       @"#if PART_UNITYADS
#if UNITY_ANDROID
#pragma warning disable
using com.tinylabproductions.TLPLib.Test; 
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    // Code here
  }
}";

    public const string CODE_WITH_PRAGMA_WRONG_PLACE =
      @"#if PART_UNITYADS
#pragma warning disable
#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Test; 
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    // Code here
  }
}";

  }

  public class CodePreprocessorTestWritingPragma : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableList();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void WhenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableList();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void WhenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableList();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }
  }

  public class CodePreprocessorTestRemovingPragma : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableList();
      CodePreprocessor.checkAndRemovePragma(lines).mkString("\n").shouldEqual(CODE);
    }

    [Test]
    public void WhenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableList();
      var actual = string.Join("\n", CodePreprocessor.checkAndRemovePragma(lines).ToArray());
      actual.shouldEqual(CODE);
    }

    [Test]
    public void WhenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableList();
      var actual = string.Join("\n", CodePreprocessor.checkAndRemovePragma(lines).ToArray());
      actual.shouldEqual(CODE);
    }
  }

  public class CodePreprocessorTestGetLastDirectiveIndex : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableList();
      lines.getLastDirectiveIndex().shouldBeSome(2);
    }

    [Test]
    public void WhenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableList();
      lines.getLastDirectiveIndex().shouldBeSome(1);
    }

    [Test]
    public void WhenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableList();
      lines.getLastDirectiveIndex().shouldBeSome(2);
    }
  }

  public class CodePreprocessorTestPragmaLineNumber : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableList();
      lines.pragmaLineNumber().shouldBeSome(2);
    }

    [Test]
    public void WhenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableList();
      lines.pragmaLineNumber().shouldBeNone();
    }

    [Test]
    public void WhenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableList();
      lines.pragmaLineNumber().shouldBeSome(1);
    }
  }
}
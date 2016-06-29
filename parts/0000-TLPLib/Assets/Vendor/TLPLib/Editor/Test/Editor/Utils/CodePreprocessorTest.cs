using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    const string CODE = @"
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    // Code here
  }
}
";
  const string CODE_WITH_PRAGMA = CodePreprocessor.PRAG_STR + CODE;

    [Test]
    public void WritingPragmaInFrontWhenHasPragma() {
      CodePreprocessor.CheckAndWritePragmaInFront(CODE_WITH_PRAGMA).shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void WritingPragmaInFrontWhenDoesntHavePragma() {
      CodePreprocessor.CheckAndWritePragmaInFront(CODE).shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void RemovingPragmaInFrontWhenHasPragma() {
      CodePreprocessor.RemovePragmaFromFront(CODE_WITH_PRAGMA).shouldEqual(CODE);
    }

    [Test]
    public void RemovingPragmaInFrontWhenDoesntHavePragma() {
      CodePreprocessor.RemovePragmaFromFront(CODE).shouldEqual(CODE);
    }

    [Test]
    public void HasPragmaInFrontWhenHasPragma()
    {
      CodePreprocessor.HasPragmaInFront(CODE_WITH_PRAGMA).shouldBeTrue();
    }

    [Test]
    public void HasPragmaInFrontWhenDoesntHavePragma() {
     CodePreprocessor.HasPragmaInFront(CODE).shouldBeFalse();
    }
  }

}
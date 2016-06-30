using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public abstract class CodePreprocessorTestBase {
    public const string CODE =
      @"using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorTest {
    // Code here
  }
}";

    public const string PRAG_STR = "#pragma warning disable\n";
    public const string CODE_WITH_PRAGMA = PRAG_STR + CODE;
  }

  public class CodePreprocessorTestWritingPragmaInFront : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      CodePreprocessor.checkAndWritePragmaInFront(CODE_WITH_PRAGMA).shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void WhenDoesntHavePragma() {
      CodePreprocessor.checkAndWritePragmaInFront(CODE).shouldEqual(CODE_WITH_PRAGMA);
    }
  }

  public class CodePreprocessorTestRemovingPragmaInFront : CodePreprocessorTestBase {
    [Test] public void WhenHasPragma() => 
      CodePreprocessor.removePragmaFromFront(CODE_WITH_PRAGMA).shouldEqual(CODE);

    [Test] public void WhenDoesntHavePragma() => 
      CodePreprocessor.removePragmaFromFront(CODE).shouldEqual(CODE);
  }

  public class CodePreprocessorTestHasPragmaInFront : CodePreprocessorTestBase {
    [Test]
    public void WhenHasPragma() {
      CodePreprocessor.hasPragmaInFront(CODE_WITH_PRAGMA).shouldBeTrue();
    }

    [Test]
    public void WhenDoesntHavePragma() {
     CodePreprocessor.hasPragmaInFront(CODE).shouldBeFalse();
    }
  }
  
  public class CodePreprocessorTestGetFilePaths : CodePreprocessorTestBase
  {

    [Test]
    public void WhenManySubdirectories() {
      var dir = "Assets/Vendor/TLPLib/Editor/Test/Editor/Utils/CodePreprocessorTemp/";
      var actual = CodePreprocessor.getFilePaths(dir);
      if (actual.isEmpty) Assert.Fail();
      else actual.get.Length.shouldEqual(4);
    }

    [Test]
    public void WhenEmptyDir()
    {
      var dir = "Assets/Vendor/TLPLib/Editor/Test/Editor/Utils/CodePreprocessorTemp/Test1/TestEmpty";
      var actual = CodePreprocessor.getFilePaths(dir);
      actual.shouldBeNone();
    }

    [Test]
    public void WhenDirWithNoCsFiles()
    {
      var dir = "Assets/Vendor/TLPLib/Editor/Test/Editor/Utils/CodePreprocessorTemp/Test1/TestDirWithNoCs";
      var actual = CodePreprocessor.getFilePaths(dir);
      actual.shouldBeNone();
    }

    [Test]
    public void WhenCsFile()
    {
      var dir = "Assets/Vendor/TLPLib/Editor/Test/Editor/Utils/CodePreprocessorTemp/Test1/NewBehaviourScript32434.cs";
      var actual = CodePreprocessor.getFilePaths(dir);
      if (actual.isEmpty) Assert.Fail();
      else actual.get.Length.shouldEqual(1);
    }

    [Test]
    public void WhenNoneCsFile()
    {
      var dir = "Assets/Vendor/TLPLib/Editor/Test/Editor/Utils/CodePreprocessorTemp/Test1/TestDirWithNoCs/TestJs2.js";
      var actual = CodePreprocessor.getFilePaths(dir);
      actual.shouldBeNone();
    }
  }

}
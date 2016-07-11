using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
    public void whenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void whenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }

    [Test]
    public void whenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndWritePragma(lines).ToArray());
      actual.shouldEqual(CODE_WITH_PRAGMA);
    }
  }

  public class CodePreprocessorTestRemovingPragma : CodePreprocessorTestBase {

    [Test]
    public void whenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndRemovePragma(lines).ToArray());
      actual.shouldEqual(CODE);
    }

    [Test]
    public void whenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndRemovePragma(lines).ToArray());
      actual.shouldEqual(CODE);
    }

    [Test]
    public void whenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableArray();
      var actual = string.Join("\n", CodePreprocessor.checkAndRemovePragma(lines).ToArray());
      actual.shouldEqual(CODE);
    }
  }

  public class CodePreprocessorTestGetLastDirectiveIndex : CodePreprocessorTestBase {

    [Test]
    public void whenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.getLastDirectiveIndex(lines);
      actual.get.shouldEqual(2);
    }

    [Test]
    public void whenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.getLastDirectiveIndex(lines);
      actual.get.shouldEqual(1);
    }

    [Test]
    public void whenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.getLastDirectiveIndex(lines);
      actual.get.shouldEqual(2);
    }
  }

  public class CodePreprocessorTestPragmaLineNumber : CodePreprocessorTestBase {

    [Test]
    public void whenHasPragma() {
      var lines = Regex.Split(CODE_WITH_PRAGMA, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.pragmaLineNumber(lines, 5);
      actual.get.shouldEqual(2);
    }

    [Test]
    public void whenDoesntHavePragma() {
      var lines = Regex.Split(CODE, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.pragmaLineNumber(lines, 5);
      actual.shouldBeNone();
    }

    [Test]
    public void whenHasPragmaInTheWrongPlace() {
      var lines = Regex.Split(CODE_WITH_PRAGMA_WRONG_PLACE, "\r\n|\r|\n").ToImmutableArray();
      var actual = CodePreprocessor.pragmaLineNumber(lines, 5);
      actual.get.shouldEqual(1);
    }
  }


  public class CodePreprocessorTestGetFilePaths : CodePreprocessorTestBase {
    readonly PathStr p_rootPath, p_emptyDir, p_noCsFilesDir, p_noneCsFile;
    readonly PathStr p_cs1, p_cs2, p_cs3, p_cs4;

    public CodePreprocessorTestGetFilePaths() {
      p_rootPath = new PathStr(Path.GetTempPath()) / "CodeProcessorTest";
      var dirPath1 = new PathStr(Directory.CreateDirectory(p_rootPath / "TestDir1").FullName);
      var dirPath2 = new PathStr(Directory.CreateDirectory(dirPath1 / "TestDir2").FullName);
      p_emptyDir = new PathStr(Directory.CreateDirectory(dirPath1 / "TestDirEmpty").FullName);
      p_noCsFilesDir = new PathStr(Directory.CreateDirectory(dirPath1 / "TestDirNoCs").FullName);
      p_cs1 = createFile(p_rootPath / "testCs1.cs");
      p_cs2 = createFile(dirPath2 / "testCs2.cs");
      p_cs3 = createFile(dirPath1 / "testCs3.cs");
      p_cs4 = createFile(dirPath1 / "testCs4.cs");
      p_noneCsFile = createFile(new PathStr(p_noCsFilesDir / "testTxt1.txt"));
    }

    static PathStr createFile(PathStr path) {
      File.Create(path).Close();
      return path;
    }

    [Test]
    public void whenManySubdirectories() {
      var actual = CodePreprocessor.getFilePaths(p_rootPath, "*.cs").rightValue.get.ToImmutableHashSet();
      actual.shouldEqual(ImmutableHashSet.Create(p_cs1, p_cs2, p_cs3, p_cs4));
    }

    [Test]
    public void whenEmptyDir() {
      var actual = CodePreprocessor.getFilePaths(p_emptyDir, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }

    [Test]
    public void whenDirWithNoCsFiles() {
      var actual = CodePreprocessor.getFilePaths(p_noCsFilesDir, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }

    [Test]
    public void whenCsFile() {
      var actual = CodePreprocessor.getFilePaths(p_cs1, "*.cs").rightValue.get.ToImmutableHashSet();
      actual.shouldEqual(ImmutableHashSet.Create(p_cs1));
    }

    [Test]
    public void whenNoneCsFile() {
      var actual = CodePreprocessor.getFilePaths(p_noneCsFile, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }
  }
}
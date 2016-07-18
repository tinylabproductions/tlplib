using System.Collections.Immutable;
using System.IO;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  class CodePreprocessorMenuItemsTest {
    readonly PathStr 
      p_rootPath, p_emptyDir, p_noCsFilesDir, p_noneCsFile,
      p_cs1, p_cs2, p_cs3, p_cs4;

    public CodePreprocessorMenuItemsTest() {
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
    public void WhenManySubdirectories() {
      var actual = CodePreprocessorMenuItems.getFilePaths(p_rootPath, "*.cs").rightValue.get.ToImmutableHashSet();
      actual.shouldEqual(ImmutableHashSet.Create(p_cs1, p_cs2, p_cs3, p_cs4));
    }

    [Test]
    public void WhenEmptyDir() {
      var actual = CodePreprocessorMenuItems.getFilePaths(p_emptyDir, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }

    [Test]
    public void WhenDirWithNoCsFiles() {
      var actual = CodePreprocessorMenuItems.getFilePaths(p_noCsFilesDir, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }

    [Test]
    public void WhenCsFile() {
      var actual = CodePreprocessorMenuItems.getFilePaths(p_cs1, "*.cs").rightValue.get.ToImmutableHashSet();
      actual.shouldEqual(ImmutableHashSet.Create(p_cs1));
    }

    [Test]
    public void WhenNoneCsFile() {
      var actual = CodePreprocessorMenuItems.getFilePaths(p_noneCsFile, "*.cs");
      actual.leftValue.isDefined.shouldBeTrue();
    }
  }
}

using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public class FileCachedBlobTest {
    public static readonly Encoding encoding = Encoding.UTF8;

    public static PathStr randomNonExistant() =>
      new PathStr(Path.GetTempPath()) / $"this_file_does_not_exist_{Guid.NewGuid()}";

    public static PathStr randomFile() => new PathStr(Path.GetTempFileName());

    public static PathStr randomFileWithContent(string content) {
      var path = randomFile();
      File.WriteAllText(path, content);
      return path;
    }

    public static PathStr randomUnreadableFile() {
//      var path = randomFile();
        // Well, this fails with System.NotImplementedException : The requested feature is not implemented.
//      var accessdeny = File.GetAccessControl(path);
//      accessdeny.SetAccessRule(
//        new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny)
//      );
//      File.SetAccessControl(path, accessdeny);
//      return path;

      throw new NotImplementedException();
    }
  }

  public class FileCachedBlobTestCached : FileCachedBlobTest {
    [Test]
    public void WhenFileExists() => 
      new FileCachedBlob(randomFile()).cached.shouldBeTrue();

    [Test]
    public void WhenFileDoesNotExist() =>
      new FileCachedBlob(randomNonExistant()).cached.shouldBeFalse();
  }

  public class FileCachedBlobTestRead : FileCachedBlobTest {
    [Test]
    public void WhenFileDoesNotExist() => 
      new FileCachedBlob(randomNonExistant()).read().shouldBeNone();

    [Test]
    public void WhenFileExistsAndCanBeRead() =>
      Encoding.UTF8.GetString(
        new FileCachedBlob(randomFileWithContent("foobar")).read().get.getOrThrow
      ).shouldEqual("foobar");

    [Test, Ignore]
    public void WhenFileExistsAndCanNotBeRead() { 
      new FileCachedBlob(randomUnreadableFile()).read().get.shouldBeError();
    }
  }

  public class FileCachedBlobTestStore : FileCachedBlobTest {
    [Test]
    public void WhenFileDoesNotExist() {
      var path = randomNonExistant();
      new FileCachedBlob(path)
        .store(encoding.GetBytes("foobar")).shouldBeSuccess(F.unit);
      File.ReadAllText(path, encoding).shouldEqual("foobar");
    }

    [Test]
    public void WhenFileDoesExist() {
      var path = randomFileWithContent("foobar");
      new FileCachedBlob(path)
        .store(encoding.GetBytes("unreal")).shouldBeSuccess(F.unit);
      File.ReadAllText(path, encoding).shouldEqual("unreal");
    }

    [Test, Ignore]
    public void WhenWriteFails() => 
      new FileCachedBlob(randomUnreadableFile())
        .store(encoding.GetBytes("unreal")).shouldBeError();
  }

  public class FileCachedBlobTestClear : FileCachedBlobTest {
    [Test]
    public void WhenFileDoesNotExist() => 
      new FileCachedBlob(randomNonExistant()).clear().shouldBeSuccess(F.unit);

    [Test]
    public void WhenFileExistsSuccessful() {
      var path = randomFileWithContent("foo");
      new FileCachedBlob(path).clear().shouldBeSuccess(F.unit);
      File.Exists(path).shouldBeFalse();
    }

    [Test, Ignore]
    public void WhenFileExistsUnsuccessful() {
      var path = randomUnreadableFile();
      new FileCachedBlob(path).clear().shouldBeError();
      File.Exists(path).shouldBeTrue();
    }
  }
}
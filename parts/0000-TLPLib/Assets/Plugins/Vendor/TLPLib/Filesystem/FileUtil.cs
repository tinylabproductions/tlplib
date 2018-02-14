using System.IO;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public static class FileUtil {
    /// <summary>
    /// Because Unity method sucks balls with uninformative error messages.
    /// 
    /// Source: https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="overwrite">Allow overwriting existing files.</param>
    public static void copyDirectory(PathStr source, PathStr destination, bool overwrite) {
      // Get the subdirectories for the specified directory.
      var dir = new DirectoryInfo(source);

      if (!dir.Exists)
        throw new DirectoryNotFoundException(
          $"Source directory does not exist or could not be found: {source}"
        );

      var dirs = dir.GetDirectories();
      // If the destination directory doesn't exist, create it.
      if (!Directory.Exists(destination))
        Directory.CreateDirectory(destination);

      // Get the files in the directory and copy them to the new location.
      var files = dir.GetFiles();
      foreach (var file in files) {
        var temppath = destination / file.Name;
        file.CopyTo(temppath, overwrite: overwrite);
      }

      // Copying subdirectories, copy them and their contents to new location.
      foreach (var subdir in dirs) {
        var temppath = destination / subdir.Name;
        copyDirectory(PathStr.a(subdir.FullName), temppath, overwrite);
      }
    }
  }
}
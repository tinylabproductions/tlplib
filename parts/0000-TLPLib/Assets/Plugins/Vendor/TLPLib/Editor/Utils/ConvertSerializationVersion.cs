using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Modified class from here: https://gist.github.com/Seneral/d88d2079ff17f77f2bd1775aadb8547b
namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class ConvertSerializationVersion {
    const string MENU_PATH = "Tools/Convert Serialization To 5.3";

    [MenuItem(MENU_PATH, false)]
    public static void ConvertSerializationVersion53() { ConvertSelectedAssetSerializationVersion(); }
  
    [MenuItem(MENU_PATH, true)]
    public static bool ValidateAsset() {
      if (Selection.activeObject == null || Selection.activeObject is DefaultAsset)
        return false;
      return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeInstanceID));
    }

    public static void ConvertSelectedAssetSerializationVersion() {
      string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
      if (string.IsNullOrEmpty(path))
        throw new System.InvalidOperationException("Selected object is not an asset!");

      // Fetch serialized data
      StringBuilder data = new StringBuilder(File.ReadAllText(path));
      if (data.Length == 0)
        throw new System.FormatException("Could not read text from asset '" + path + "'! Make sure you have text serialization force on!");

      // Convert data to new version
      SetSerializationVersion(ref data);

      // Save converted file
      path = Path.GetDirectoryName(path) + Path.AltDirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + "_Converted" + Path.GetExtension(path);
      if (path.StartsWith("Assets"))
        path = path.Replace("Assets", Application.dataPath);
      File.WriteAllText(path, data.ToString());
      AssetDatabase.Refresh();
    }

    public static bool SetSerializationVersion(ref StringBuilder data) {
      // Just lower serialized version for GameObjects only so Unity doesn't mess with the data (fixes empty name field pre-5.5)
      data.Replace("serializedVersion: 5", "serializedVersion: 4");
      // Fix UI images
      data.Replace("f70555f144d8491a825f0804e09c671c", "f5f67c52d1564df4a8936ccd202a3bd8");
      return true;
    }
  }
}
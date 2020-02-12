#if ODIN_INSPECTOR
using System.Linq;
using Sirenix.OdinInspector.Editor;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  public static class OdinUtils {
    public static InspectorProperty getChildSmart(this InspectorProperty property, string name) =>
      property.Children[name] 
      // When placed in any PropertyGroupAttribute, value gets placed as child element in parent #groupName
      // https://odininspector.com/documentation/sirenix.odininspector.propertygroupattribute
      ?? property.Children.First(_ => _.Info.PropertyType == PropertyType.Group).Children.First();
  }
}
#endif
using UnityEngine;
using System;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  public class HideByAttribute : PropertyAttribute {
    //The name of the bool field that will be in control
    public string ConditionalSourceField;

    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public bool HideInInspector;
    
//    public bool isInverted;

    public HideByAttribute(string conditionalSourceField) {
      ConditionalSourceField = conditionalSourceField;
      HideInInspector = false;
    }

    public HideByAttribute(string conditionalSourceField, bool hideInInspector) {
      ConditionalSourceField = conditionalSourceField;
      HideInInspector = hideInInspector;
    }
  }
  
}
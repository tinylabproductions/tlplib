using UnityEngine;
using System;


public class ConditionalHideAttribute : PropertyAttribute
{
  //The name of the bool field that will be in control
  public string ConditionalSourceField = "";
  //TRUE = Hide in inspector / FALSE = Disable in inspector 
  public bool HideInInspector = false;
 
  public ConditionalHideAttribute(string conditionalSourceField)
  {
    ConditionalSourceField = conditionalSourceField;
    HideInInspector = false;
  }
 
  public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
  {
    ConditionalSourceField = conditionalSourceField;
    HideInInspector = hideInInspector;
  }
}
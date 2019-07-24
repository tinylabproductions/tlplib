using System;

namespace Sirenix.OdinInspector {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class ChildDropdownAttribute : Attribute {
        public bool IsUniqueList;
    }
}
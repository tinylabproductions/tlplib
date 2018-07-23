using UnityEngine;
using System.Collections;
using System;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	[Serializable]
	public class MethodArgument {
		[SerializeField] private string type;

		public Type SerializedType {
			get {
				string[] split = type.Split('.');
				return Type.GetType(type + (split[0] == "UnityEngine" ? ",UnityEngine" : ""));
			}
			set {
				type = value.ToString();
				valueName = GetValueName();
			}
		}

		public string name;
		public string valueName;

		public UnityEngine.Object objParam;
		public int intParam;
		public float floatParam;
		public AnimationCurve curveParam;
		public string stringParam;
		public Vector2 vector2Param;
		public Vector3 vector3Param;
		public Transform transformParam;
		public bool boolParam;
		public Vector4 vector4Param;
		public Color colorParam;
		public Rect rectParam;
		public Space spaceParam;
		public Quaternion quaternionParam;
		public SendMessageOptions sendMessageOptionsParam;

		public MethodArgument(string name, string type) {
			this.type = type;
			this.name = name;
			this.valueName = GetValueName();
		}

		public string GetValueName() {

			switch (type) {
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
					return "intParam";
				case "System.Single":
				case "System.Double":
					return "floatParam";
				case "System.String":
					return "stringParam";
				case "System.Boolean":
					return "boolParam";
				case "UnityEngine.Vector2":
					return "vector2Param";
				case "UnityEngine.Vector3":
					return "vector3Param";
				case "UnityEngine.Vector4":
					return "vector4Param";
				case "UnityEngine.Color":
					return "colorParam";
				case "UnityEngine.Rect":
					return "rectParam";
				case "UnityEngine.AnimationCurve":
					return "curveParam";
				case "UnityEngine.Space":
					return "spaceParam";
				case "UnityEngine.Quaternion":
					return "quaternionParam";
				case "UnityEngine.SendMessageOptions":
					return "sendMessageOptionsParam";
				default:
					return "objParam";

			}
		}

		public object Get() {

			switch (type) {
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
					return intParam;
				case "System.Single":
				case "System.Double":
					return floatParam;
				case "System.String":
					return stringParam;
				case "System.Boolean":
					return boolParam;
				case "UnityEngine.Vector2":
					return vector2Param;
				case "UnityEngine.Vector3":
					return vector3Param;
				case "UnityEngine.Vector4":
					return vector4Param;
				case "UnityEngine.Color":
					return colorParam;
				case "UnityEngine.Rect":
					return rectParam;
				case "UnityEngine.AnimationCurve":
					return curveParam;
				case "UnityEngine.Space":
					return spaceParam;
				case "UnityEngine.Quaternion":
					return quaternionParam;
				case "UnityEngine.SendMessageOptions":
					return sendMessageOptionsParam;
				default:
					return objParam;

			}
		}
	}
}
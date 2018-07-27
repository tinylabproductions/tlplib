using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	[System.Serializable]
	public class EventNodeTemp {
		//public Component target;
		public string method;
		public List<MethodArgumentTemp> arguments;
		public float time = 3;
		[SerializeField] private string type;

		public Type SerializedType {
			get {
				string[] split = type.Split('.');
				return Type.GetType(type + (split[0] == "UnityEngine" ? ",UnityEngine" : ""));
			}
			set { type = value.ToString(); }
		}

		public static Type GetType(string name) {
			string[] split = name.Split('.');
			return Type.GetType(name + (split[0] == "UnityEngine" ? ",UnityEngine" : ""));
		}

		private bool cached = false;
		private MethodInfo methodInfo;
		private object[] args;
		private Component component;
		[System.NonSerialized] public bool finished;

		public void Invoke(GameObject target) {
			if (finished) {
				return;
			}

			if (!cached && target != null) {
				List<Type> paramTypes = new List<Type>();
				List<object> args1 = new List<object>();

				foreach (MethodArgumentTemp arg in arguments) {
					paramTypes.Add(arg.SerializedType);
					args1.Add(arg.Get());
				}

				args = args1.ToArray();
				methodInfo = SerializedType.GetMethod(method, paramTypes.ToArray());
				if (SerializedType == typeof(Component) || SerializedType.IsSubclassOf(typeof(Component))) {
					component = target.GetComponent(SerializedType);
				}
			}

			try {
				methodInfo.Invoke(component, args);
			}
			catch { }

			finished = true;
		}
	}
}
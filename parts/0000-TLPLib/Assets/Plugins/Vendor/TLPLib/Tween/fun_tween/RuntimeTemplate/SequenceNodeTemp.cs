using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	[System.Serializable]
	public class SequenceNodeTemp {
		[HideInInspector] public Component target;
		[HideInInspector] public string property;

		private PropertyInfo mPropertyInfo;

		private PropertyInfo propertyInfo {
			get {
				if (mPropertyInfo == null) {
					mPropertyInfo = target.GetType().GetProperty(property);
				}

				return mPropertyInfo;
			}
		}

		private FieldInfo mField;

		private FieldInfo fieldInfo {
			get {
				if (mField == null) {
					mField = target.GetType().GetField(property);
				}

				return mField;
			}
		}

		public Type PropertyType {
			get {
				Type mType = null;
				if (propertyInfo != null) {
					mType = propertyInfo.PropertyType;
				}
				else if (fieldInfo != null) {
					mType = fieldInfo.FieldType;
				}

				return mType;

			}
		}

		public float startTime;
		public float duration = 5;
		public EasingCurveTemp.EaseType ease = EasingCurveTemp.EaseType.Linear;
		public int channel;

		public float fromFloat;
		public float toFloat;
		public Vector2 fromVector2;
		public Vector2 toVector2;
		public Vector3 fromVector3;
		public Vector3 toVector3;
		public Color fromColor;
		public Color toColor;

		private bool update;

		public SequenceNodeTemp(Component target, string property) {
			this.target = target;
			this.property = property;
		}

		public void UpdateTween(float time) {
			float percentage = ((time - startTime) / duration);
			if (percentage > 0.0f && percentage <= 1.0f) {
				update = true;
			}

			if (update) {
				UpdateValue(percentage);
			}

			if ((percentage <= 0.0f || percentage > 1.0f)) {
				update = false;
			}
		}

		public void UpdateValue(float percentage) {
			if (fieldInfo != null) {
				if (PropertyType == typeof(float)) {
					fieldInfo.SetValue(target, GetValue(fromFloat, toFloat, percentage));
				}
				else if (PropertyType == typeof(Vector2)) {
					fieldInfo.SetValue(target, GetValue(fromVector2, toVector2, percentage));
				}
				else if (PropertyType == typeof(Vector3)) {
					fieldInfo.SetValue(target, GetValue(fromVector3, toVector3, percentage));
				}
				else if (PropertyType == typeof(Color)) {
					fieldInfo.SetValue(target, GetValue(fromColor, toColor, percentage));
				}
			}
			else if (propertyInfo != null) {
				if (PropertyType == typeof(float)) {
					propertyInfo.SetValue(target, GetValue(fromFloat, toFloat, percentage), null);
				}
				else if (PropertyType == typeof(Vector2)) {
					propertyInfo.SetValue(target, GetValue(fromVector2, toVector2, percentage), null);
				}
				else if (PropertyType == typeof(Vector3)) {
					propertyInfo.SetValue(target, GetValue(fromVector3, toVector3, percentage), null);
				}
				else if (PropertyType == typeof(Color)) {
					propertyInfo.SetValue(target, GetValue(fromColor, toColor, percentage), null);
				}
			}
		}

		public void SetValue(object value) {
			if (fieldInfo != null) {
				if (PropertyType == typeof(float)) {
					fieldInfo.SetValue(target, value);
				}
				else if (PropertyType == typeof(Vector2)) {
					fieldInfo.SetValue(target, value);
				}
				else if (PropertyType == typeof(Vector3)) {
					fieldInfo.SetValue(target, value);
				}
				else if (PropertyType == typeof(Color)) {
					fieldInfo.SetValue(target, value);
				}
			}
			else if (propertyInfo != null) {
				if (PropertyType == typeof(float)) {
					propertyInfo.SetValue(target, value, null);
				}
				else if (PropertyType == typeof(Vector2)) {
					propertyInfo.SetValue(target, value, null);
				}
				else if (PropertyType == typeof(Vector3)) {
					propertyInfo.SetValue(target, value, null);
				}
				else if (PropertyType == typeof(Color)) {
					propertyInfo.SetValue(target, value, null);
				}
			}
		}

		public void SetDefaultValue() {
			object value = null;
			if (fieldInfo != null) {
				value = fieldInfo.GetValue(target);
			}
			else if (propertyInfo != null) {
				value = propertyInfo.GetValue(target, null);
			}

			if (PropertyType == typeof(float)) {
				fromFloat = (float) value;
			}
			else if (PropertyType == typeof(Vector2)) {
				fromVector2 = (Vector2) value;
			}
			else if (PropertyType == typeof(Vector3)) {
				fromVector3 = (Vector3) value;
			}
			else if (PropertyType == typeof(Color)) {
				fromColor = (Color) value;
			}

		}

		public Vector3 GetValue(Vector3 from, Vector3 to, float t) {
			Vector3 vector3 = new Vector3();
			vector3.x = EasingCurveTemp.GetValue(this.ease, from.x, to.x, t);
			vector3.y = EasingCurveTemp.GetValue(this.ease, from.y, to.y, t);
			vector3.z = EasingCurveTemp.GetValue(this.ease, from.z, to.z, t);
			return vector3;
		}

		public Vector2 GetValue(Vector2 from, Vector2 to, float t) {
			Vector2 vector2 = new Vector2();
			vector2.x = EasingCurveTemp.GetValue(this.ease, from.x, to.x, t);
			vector2.y = EasingCurveTemp.GetValue(this.ease, from.y, to.y, t);
			return vector2;
		}

		public Color GetValue(Color from, Color to, float t) {
			Color color = new Color();
			color.r = EasingCurveTemp.GetValue(this.ease, from.r, to.r, t);
			color.g = EasingCurveTemp.GetValue(this.ease, from.g, to.g, t);
			color.b = EasingCurveTemp.GetValue(this.ease, from.b, to.b, t);
			color.a = EasingCurveTemp.GetValue(this.ease, from.a, to.a, t);
			return color;
		}

		public float GetValue(float from, float to, float t) {
			float value = EasingCurveTemp.GetValue(this.ease, from, to, t);
			return value;
		}
	}
}

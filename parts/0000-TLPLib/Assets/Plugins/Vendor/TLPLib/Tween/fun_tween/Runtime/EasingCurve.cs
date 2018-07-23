/*
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using UnityEngine;
using System.Collections;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
	public static class EasingCurve {

		public enum EaseType {
			EaseInQuad,
			EaseOutQuad,
			EaseInOutQuad,
			EaseInCubic,
			EaseOutCubic,
			EaseInOutCubic,
			EaseInQuart,
			EaseOutQuart,
			EaseInOutQuart,
			EaseInQuint,
			EaseOutQuint,
			EaseInOutQuint,
			EaseInSine,
			EaseOutSine,
			EaseInOutSine,
			EaseInExpo,
			EaseOutExpo,
			EaseInOutExpo,
			EaseInCirc,
			EaseOutCirc,
			EaseInOutCirc,
			Linear,
			Spring,
			EaseInBounce,
			EaseOutBounce,
			EaseInOutBounce,
			EaseInBack,
			EaseOutBack,
			EaseInOutBack,
			EaseInElastic,
			EaseOutElastic,
			EaseInOutElastic,
			Punch
		}

		//instantiates a cached ease equation refrence:
		public static float GetValue(EaseType easeType, float start, float end, float value) {
			switch (easeType) {
				case EaseType.EaseInQuad:
					return EaseInQuad(start, end, value);
				case EaseType.EaseOutQuad:
					return EaseOutQuad(start, end, value);
				case EaseType.EaseInOutQuad:
					return EaseInOutQuad(start, end, value);
				case EaseType.EaseInCubic:
					return EaseInCubic(start, end, value);
				case EaseType.EaseOutCubic:
					return EaseOutCubic(start, end, value);
				case EaseType.EaseInOutCubic:
					return EaseInOutCubic(start, end, value);
				case EaseType.EaseInQuart:
					return EaseInQuart(start, end, value);
				case EaseType.EaseOutQuart:
					return EaseOutQuart(start, end, value);
				case EaseType.EaseInOutQuart:
					return EaseInOutQuart(start, end, value);
				case EaseType.EaseInQuint:
					return EaseInQuint(start, end, value);
				case EaseType.EaseOutQuint:
					return EaseOutQuint(start, end, value);
				case EaseType.EaseInOutQuint:
					return EaseInOutQuint(start, end, value);
				case EaseType.EaseInSine:
					return EaseInSine(start, end, value);
				case EaseType.EaseOutSine:
					return EaseOutSine(start, end, value);
				case EaseType.EaseInOutSine:
					return EaseInOutSine(start, end, value);
				case EaseType.EaseInExpo:
					return EaseInExpo(start, end, value);
				case EaseType.EaseOutExpo:
					return EaseOutExpo(start, end, value);
				case EaseType.EaseInOutExpo:
					return EaseInOutExpo(start, end, value);
				case EaseType.EaseInCirc:
					return EaseInCirc(start, end, value);
				case EaseType.EaseOutCirc:
					return EaseOutCirc(start, end, value);
				case EaseType.EaseInOutCirc:
					return EaseInOutCirc(start, end, value);
				case EaseType.Spring:
					return Spring(start, end, value);
				case EaseType.EaseInBounce:
					return EaseInBounce(start, end, value);
				case EaseType.EaseOutBounce:
					return EaseOutBounce(start, end, value);
				case EaseType.EaseInOutBounce:
					return EaseInOutBounce(start, end, value);
				case EaseType.EaseInBack:
					return EaseInBack(start, end, value);
				case EaseType.EaseOutBack:
					return EaseOutBack(start, end, value);
				case EaseType.EaseInOutBack:
					return EaseInOutBack(start, end, value);
				case EaseType.EaseInElastic:
					return EaseInElastic(start, end, value);
				case EaseType.EaseOutElastic:
					return EaseOutElastic(start, end, value);
				case EaseType.EaseInOutElastic:
					return EaseInOutElastic(start, end, value);
				default:
					return Linear(start, end, value);
			}
		}

		public static float Linear(float start, float end, float value) { return Mathf.Lerp(start, end, value); }

		public static float CLerp(float start, float end, float value) {
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) * 0.5f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half) {
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}
			else if ((end - start) > half) {
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}
			else retval = start + (end - start) * value;

			return retval;
		}

		public static float Spring(float start, float end, float value) {
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) *
				(1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}

		public static float EaseInQuad(float start, float end, float value) {
			end -= start;
			return end * value * value + start;
		}

		public static float EaseOutQuad(float start, float end, float value) {
			end -= start;
			return -end * value * (value - 2) + start;
		}

		public static float EaseInOutQuad(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value + start;
			value--;
			return -end * 0.5f * (value * (value - 2) - 1) + start;
		}

		public static float EaseInCubic(float start, float end, float value) {
			end -= start;
			return end * value * value * value + start;
		}

		public static float EaseOutCubic(float start, float end, float value) {
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}

		public static float EaseInOutCubic(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value + start;
			value -= 2;
			return end * 0.5f * (value * value * value + 2) + start;
		}

		public static float EaseInQuart(float start, float end, float value) {
			end -= start;
			return end * value * value * value * value + start;
		}

		public static float EaseOutQuart(float start, float end, float value) {
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}

		public static float EaseInOutQuart(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value * value + start;
			value -= 2;
			return -end * 0.5f * (value * value * value * value - 2) + start;
		}

		public static float EaseInQuint(float start, float end, float value) {
			end -= start;
			return end * value * value * value * value * value + start;
		}

		public static float EaseOutQuint(float start, float end, float value) {
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}

		public static float EaseInOutQuint(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value * value * value + start;
			value -= 2;
			return end * 0.5f * (value * value * value * value * value + 2) + start;
		}

		public static float EaseInSine(float start, float end, float value) {
			end -= start;
			return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
		}

		public static float EaseOutSine(float start, float end, float value) {
			end -= start;
			return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
		}

		public static float EaseInOutSine(float start, float end, float value) {
			end -= start;
			return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
		}

		public static float EaseInExpo(float start, float end, float value) {
			end -= start;
			return end * Mathf.Pow(2, 10 * (value - 1)) + start;
		}

		public static float EaseOutExpo(float start, float end, float value) {
			end -= start;
			return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
		}

		public static float EaseInOutExpo(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
			value--;
			return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
		}

		public static float EaseInCirc(float start, float end, float value) {
			end -= start;
			return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
		}

		public static float EaseOutCirc(float start, float end, float value) {
			value--;
			end -= start;
			return end * Mathf.Sqrt(1 - value * value) + start;
		}

		public static float EaseInOutCirc(float start, float end, float value) {
			value /= .5f;
			end -= start;
			if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
		}

		public static float EaseInBounce(float start, float end, float value) {
			end -= start;
			float d = 1f;
			return end - EaseOutBounce(0, end, d - value) + start;
		}

		public static float EaseOutBounce(float start, float end, float value) {
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f)) {
				return end * (7.5625f * value * value) + start;
			}
			else if (value < (2 / 2.75f)) {
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}
			else if (value < (2.5 / 2.75)) {
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}
			else {
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}

		public static float EaseInOutBounce(float start, float end, float value) {
			end -= start;
			float d = 1f;
			if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
			else return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
		}

		public static float EaseInBack(float start, float end, float value) {
			end -= start;
			value /= 1;
			float s = 1.70158f;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}

		public static float EaseOutBack(float start, float end, float value) {
			float s = 1.70158f;
			end -= start;
			value = (value) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}

		public static float EaseInOutBack(float start, float end, float value) {
			float s = 1.70158f;
			end -= start;
			value /= .5f;
			if ((value) < 1) {
				s *= (1.525f);
				return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
			}

			value -= 2;
			s *= (1.525f);
			return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}

		public static float Punch(float amplitude, float value) {
			float s = 9;
			if (value == 0) {
				return 0;
			}
			else if (value == 1) {
				return 0;
			}

			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
		}

		public static float EaseInElastic(float start, float end, float value) {
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)) {
				a = end;
				s = p / 4;
			}
			else {
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
		}

		public static float EaseOutElastic(float start, float end, float value) {
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)) {
				a = end;
				s = p * 0.25f;
			}
			else {
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
		}

		public static float EaseInOutElastic(float start, float end, float value) {
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d * 0.5f) == 2) return start + end;

			if (a == 0f || a < Mathf.Abs(end)) {
				a = end;
				s = p / 4;
			}
			else {
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			if (value < 1)
				return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
			return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
		}

	}
}

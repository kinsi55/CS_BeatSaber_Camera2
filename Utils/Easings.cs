namespace Camera2.Utils {
	static class Easings {
		public static float EaseInOutCubic01(float value) {
			value /= .5f;
			if(value < 1) return 0.5f * value * value * value;
			value -= 2;
			return 0.5f * (value * value * value + 2);
		}
	}
}

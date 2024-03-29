﻿using Camera2.Configuration;
using Newtonsoft.Json;

namespace Camera2.Interfaces {
	abstract class CameraSubSettings {
		[JsonIgnore]
		public CameraSettings settings { get; private set; }
		void Init(CameraSettings settings) {
			this.settings = settings;
		}

		public static T GetFor<T>(CameraSettings settings) where T : CameraSubSettings, new() {
			var x = new T();
			x.Init(settings);
			return x;
		}
	}
}

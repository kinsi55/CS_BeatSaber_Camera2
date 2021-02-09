using System;
using UnityEngine;
using Newtonsoft.Json;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;


namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		private int _limit = 0;
		[JsonIgnore]
		public float targetFrameTime { get; private set; } = 1f / 60f;

		public int fpsLimit {
			get { return _limit; }
			set {
				_limit = value;
				targetFrameTime = value != 0f ? 1f / value : 0f;

				GlobalFPSCap.Postfix();
			}
		}
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		float renderTimeRollAccu = 0f;
		
		new public bool Pre() {
			if(!enabled || settings.FPSLimiter.fpsLimit <= 0 || Application.targetFrameRate == settings.FPSLimiter.fpsLimit) return true;

			if(cam.timeSinceLastRender + renderTimeRollAccu < settings.FPSLimiter.targetFrameTime) return false;
			renderTimeRollAccu = (cam.timeSinceLastRender + renderTimeRollAccu) % settings.FPSLimiter.targetFrameTime;
			return true;
		}
	}
}

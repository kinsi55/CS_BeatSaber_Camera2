using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		private int _limit = 60;
		[JsonIgnore]
		public float frameTime { get; private set; } = 1f / 60f;

		public int limit {
			get { return _limit; }
			set {
				_limit = value;
				frameTime = value != 0f ? 1f / value : 0f;

				GlobalFPSCap.Postfix();
			}
		}
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		float renderTimeRollAccu = 0f;
		
		new public bool Pre() {
			if(!enabled || Application.targetFrameRate == settings.FPSLimiter.limit) return true;

			if(cam.timeSinceLastRender + renderTimeRollAccu < settings.FPSLimiter.frameTime) return false;
			renderTimeRollAccu = (cam.timeSinceLastRender + renderTimeRollAccu) % settings.FPSLimiter.frameTime;
			return true;
		}
	}
}

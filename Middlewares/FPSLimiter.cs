using Camera2.Interfaces;
using Newtonsoft.Json;

namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		private int _limit = 60;
		[JsonIgnore]
		public float frameTime { get; private set; } = 1f / 60f;

		public int limit {
			get { return _limit; }
			set {
				frameTime = 1f / value;
				_limit = value;
			}
		}
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		float renderTimeRollAccu = 0f;
		
		new public bool Pre() {
			if(!enabled) return true;

			if(cam.timeSinceLastRender + renderTimeRollAccu < settings.FPSLimiter.frameTime) return false;
			renderTimeRollAccu = (cam.timeSinceLastRender + renderTimeRollAccu) % settings.FPSLimiter.frameTime;
			return true;
		}
	}
}

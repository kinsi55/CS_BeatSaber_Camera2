using Camera2.Interfaces;

namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		public int limit = 60;
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		new public bool Pre() {
			if(enabled && cam.timeSinceLastRender < 1f / settings.FPSLimiter.limit) return false;
			return true;
		}
	}
}

using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		public int limit = 60;
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		float tickSum = 0;

		private void Update() {
			tickSum += Time.deltaTime;
		}

		new public bool Pre() {
			if(this.enabled && tickSum < 1f / settings.FPSLimiter.limit) return false;
			tickSum = 0;
			return true;
		}
	}
}

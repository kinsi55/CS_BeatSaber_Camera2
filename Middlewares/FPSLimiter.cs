using System;
using UnityEngine;
using Newtonsoft.Json;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;


namespace Camera2.Configuration {
	class Settings_FPSLimiter {
		private int _limit = 60;
		[JsonIgnore]
		public float targetFrameTime { get; private set; } = 1f / 60f;

		public int limit {
			get { return _limit; }
			set {
				_limit = value;
				targetFrameTime = value != 0f ? 1f / value : 0f;

				GlobalFPSCap.Postfix();
			}
		}

		public bool improveFrametimes = true;

		/*
		 * Tries to find a "Better" frametime target that is more divisible by
		 * the application target FPS (Probably VR, or the highest FPS camera)
		 * to make recordings smoother due to a more consistent frametime output
		 */
		public void CalculateIdealFrametime() {
			targetFrameTime = limit != 0f ? 1f / limit : 0f;

			if(!improveFrametimes)
				return;

			var targetFps = UnityEngine.XR.XRDevice.refreshRate;
			if(targetFps <= 0 || !UnityEngine.XR.XRDevice.isPresent) {
				if(Application.targetFrameRate <= 0)
					targetFps = Screen.currentResolution.refreshRate;
				else
					targetFps = Application.targetFrameRate;
			}

			if(limit < targetFps / 2f && targetFrameTime > 0f) {
				var idk = float.MaxValue;
				var n = limit;

				for(; n < targetFps; n++) {
					var idk2 = (targetFps / n) % 1f;

					if(idk2 > idk)
						break;

					idk = idk2;
				}
				targetFrameTime = 1f / (n - 1);
			}

			targetFrameTime = Math.Min(targetFps / 4, targetFrameTime * 0.95f);
		}
	}
}

namespace Camera2.Middlewares {
	class FPSLimiter : CamMiddleware, IMHandler {
		float renderTimeRollAccu = 0f;
		
		new public bool Pre() {
			if(!enabled || Application.targetFrameRate == settings.FPSLimiter.limit) return true;

			if(cam.timeSinceLastRender + renderTimeRollAccu < settings.FPSLimiter.targetFrameTime) return false;
			renderTimeRollAccu = (cam.timeSinceLastRender + renderTimeRollAccu) % settings.FPSLimiter.targetFrameTime;
			return true;
		}
	}
}

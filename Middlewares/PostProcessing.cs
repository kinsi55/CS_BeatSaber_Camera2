using UnityEngine;
using Camera2.Utils;
using Camera2.Interfaces;
using Camera2.Managers;

namespace Camera2.Configuration {
	class Settings_PostProcessing : CameraSubSettings {
		public float transparencyThreshold = 0f;
	}
}

namespace Camera2.Middlewares {
	class PostProcessing : CamMiddleware, IMHandler {
		new public void Post() {
			if(enabled && Plugin.ShaderMat_LuminanceKey != null) {
				var x = RenderTexture.active;
				Plugin.ShaderMat_LuminanceKey.SetFloat("_Threshold", settings.antiAliasing > 1 ? settings.PostProcessing.transparencyThreshold : 0);
				Graphics.Blit(cam.renderTexture, cam.renderTexture, Plugin.ShaderMat_LuminanceKey);
				RenderTexture.active = x;
			}
		}
	}
}

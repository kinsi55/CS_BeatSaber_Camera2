using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Configuration {
	class Settings_PostProcessing : CameraSubSettings {
		public float transparencyThreshold = 0f;

		private bool _forceDepthTexture = false;
		public bool forceDepthTexture {
			get => _forceDepthTexture;
			set {
				_forceDepthTexture = value;
				settings.cam.UpdateDepthTextureActive();
			}
		}

		public float chromaticAberrationAmount = 0f;

	}
}

namespace Camera2.Middlewares {
	class PostProcessing : CamMiddleware, IMHandler {
		new public void Post() {
			if(enabled && Plugin.ShaderMat_LuminanceKey != null) {
				var x = RenderTexture.active;
				Plugin.ShaderMat_LuminanceKey.SetFloat("_Threshold", settings.antiAliasing > 1 ? settings.PostProcessing.transparencyThreshold : 0);
				Plugin.ShaderMat_LuminanceKey.SetFloat("_HasDepth", cam.UCamera.depthTextureMode != DepthTextureMode.None ? 1 : 0);
				Graphics.Blit(cam.renderTexture, cam.renderTexture, Plugin.ShaderMat_LuminanceKey);
				if(settings.PostProcessing.chromaticAberrationAmount > 0) {
					Plugin.ShaderMat_CA.SetFloat("_ChromaticAberration", settings.PostProcessing.chromaticAberrationAmount / 1000);
					Graphics.Blit(cam.renderTexture, cam.renderTexture, Plugin.ShaderMat_CA);
				}
				RenderTexture.active = x;
			}
		}
	}
}

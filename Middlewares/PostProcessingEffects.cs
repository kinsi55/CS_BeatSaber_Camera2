using UnityEngine;
using Camera2.Utils;
using Camera2.Interfaces;
using Camera2.Managers;

namespace Camera2.Middlewares {
	class PostProcessingEffects : CamMiddleware, IMHandler {
		new public void Post() {
			if(Plugin.PostMaterial != null) {
				var x = RenderTexture.active;
				Graphics.Blit(cam.renderTexture, cam.renderTexture, Plugin.PostMaterial);
				RenderTexture.active = x;
			}
		}
	}
}

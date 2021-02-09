using UnityEngine;
using Camera2.Utils;
using Camera2.Interfaces;
using Camera2.Managers;

namespace Camera2.Middlewares {
#if DEV
	class PostProcessor : CamMiddleware, IMHandler {
		new public void Post() {
			if(enabled && Plugin.PostMaterial != null) {
				var x = RenderTexture.active;
				Graphics.Blit(cam.renderTexture, cam.renderTexture, Plugin.PostMaterial);
				RenderTexture.active = x;
			}
		}
	}
#endif
}

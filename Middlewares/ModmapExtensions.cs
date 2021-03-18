using UnityEngine;
using Camera2.Utils;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.HarmonyPatches;

namespace Camera2.Configuration {
	class Settings_ModmapExtensions : CameraSubSettings {
		public bool moveWithMap = true;
		public bool autoOpaqueWalls = false;
		public bool autoHideHUD = false;

		public bool ShouldSerializemoveWithMap() => settings.type == CameraType.Positionable;
	}
}

namespace Camera2.Middlewares {
	class ModmapExtensions : CamMiddleware, IMHandler {
		private Transform attachedTo = null;
		public new bool Pre() {
			// We wanna parent FP cams as well so that the noodle translations are applied instantly and dont get smoothed out by SmoothFollow
			if(
				enabled &&
				HookLeveldata.isModdedMap &&
				(settings.ModmapExtensions.moveWithMap || settings.type != Configuration.CameraType.Positionable) && 
				SceneUtil.songWorldTransform != null
			) {
				// If we are not yet attached, and we dont have a parent thats active yet, try to get one!
				if(attachedTo != SceneUtil.songWorldTransform) {
#if DEBUG
					Plugin.Log.Info($"Enabling Modmap parenting for camera {cam.name}");
#endif
					attachedTo = SceneUtil.songWorldTransform;
					
					cam.SetOrigin(SceneUtil.songWorldTransform, false);
				}
			} else if(attachedTo != null) {
#if DEBUG
				Plugin.Log.Info($"Disabling Modmap parenting for camera {cam.name}");
#endif
				cam.SetOrigin(null);
				attachedTo = null;
			}
			return true;
		}
	}
}

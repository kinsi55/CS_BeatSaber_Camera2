using UnityEngine;
using Camera2.Utils;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.HarmonyPatches;
using Camera2.Behaviours;

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
		private Transformer mapMovementTransformerRemoveRoomOffset = null;
		private Transformer mapMovementTransformer = null;
		public new bool Pre() {
			// We wanna parent FP cams as well so that the noodle translations are applied instantly and dont get smoothed out by SmoothFollow
			if(
				enabled &&
				HookLeveldata.isModdedMap &&
				(settings.ModmapExtensions.moveWithMap || settings.type != Configuration.CameraType.Positionable) && 
				SceneUtil.songWorldTransform != null
			) {
				// If we are not yet attached, and we dont have a parent thats active yet, try to get one!
				if(mapMovementTransformer == null) {
#if DEBUG
					Plugin.Log.Info($"Enabling Modmap parenting for camera {cam.name}");
#endif
					mapMovementTransformer = cam.GetOrCreateTransformer("ModMapExt", TransformerOrders.ModmapParenting);

					if(settings.type == Configuration.CameraType.Positionable) {
						mapMovementTransformerRemoveRoomOffset = cam.GetOrCreateTransformer("ModMapExtDe-Offset", TransformerOrders.ModmapParenting + 1);

						mapMovementTransformerRemoveRoomOffset.transform.localPosition = -HookRoomAdjust.position;
						mapMovementTransformerRemoveRoomOffset.transform.localRotation = Quaternion.Inverse(HookRoomAdjust.rotation);
					}
				}

				mapMovementTransformer.transform.localPosition = SceneUtil.songWorldTransform.position;
				mapMovementTransformer.transform.localRotation = SceneUtil.songWorldTransform.rotation;
			} else if(mapMovementTransformer != null) {
#if DEBUG
				Plugin.Log.Info($"Disabling Modmap parenting for camera {cam.name}");
#endif
				Destroy(mapMovementTransformer);
				if(mapMovementTransformerRemoveRoomOffset != null) Destroy(mapMovementTransformerRemoveRoomOffset);
				mapMovementTransformer = null;
			}
			return true;
		}
	}
}

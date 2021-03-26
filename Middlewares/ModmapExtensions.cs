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
		private Transformer mapMovementTransformer = null;
		private Transformer mapMovementTransformerDeRoomAdjust = null;
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
					mapMovementTransformer = cam.transformchain.AddOrGet("ModMapExt", TransformerOrders.ModmapParenting);

					if(HookRoomAdjust.position != Vector3.zero || HookRoomAdjust.rotation != Quaternion.identity)
						mapMovementTransformerDeRoomAdjust = cam.transformchain.AddOrGet("ModMapExt2", TransformerOrders.ModmapParenting + 1);

					if(mapMovementTransformerDeRoomAdjust != null) {
						if(settings.type == Configuration.CameraType.Positionable) {
							mapMovementTransformerDeRoomAdjust.position = -HookRoomAdjust.position;
							mapMovementTransformerDeRoomAdjust.rotation = Quaternion.Inverse(HookRoomAdjust.rotation);
						} else {
							mapMovementTransformerDeRoomAdjust.position = Vector3.zero;
							mapMovementTransformerDeRoomAdjust.rotation = Quaternion.identity;
						}
					}
				}

				mapMovementTransformer.position = SceneUtil.songWorldTransform.position;
				mapMovementTransformer.rotation = SceneUtil.songWorldTransform.rotation;
			} else if(mapMovementTransformer != null) {
#if DEBUG
				Plugin.Log.Info($"Disabling Modmap parenting for camera {cam.name}");
#endif
				mapMovementTransformer.position = mapMovementTransformerDeRoomAdjust.position = Vector3.zero;
				mapMovementTransformer.rotation = mapMovementTransformerDeRoomAdjust.rotation = Quaternion.identity;
				mapMovementTransformer = null;
			}
			return true;
		}
	}
}

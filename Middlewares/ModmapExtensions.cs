using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

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
		static FieldInfo Noodle_PlayerTrack_Origin = AccessTools.Field(AccessTools.TypeByName("NoodleExtensions.Animation.PlayerTrack"), "_origin");
		static Transform noodleOrigin;

		private Transformer mapMovementTransformer = null;
		public new bool Pre() {
			// We wanna parent FP cams as well so that the noodle translations are applied instantly and dont get smoothed out by SmoothFollow
			if(
				enabled &&
				HookLeveldata.isModdedMap &&
				Noodle_PlayerTrack_Origin != null &&
				(settings.ModmapExtensions.moveWithMap || settings.type != Configuration.CameraType.Positionable)
			) {
				// Noodle maps do not *necessarily* have a playertrack if it not actually used
				if(noodleOrigin != null || (noodleOrigin = (Transform)Noodle_PlayerTrack_Origin.GetValue(null)) != null) {
					// If we are not yet attached, and we dont have a parent thats active yet, try to get one!
					if(mapMovementTransformer == null) {
#if DEBUG
						Plugin.Log.Info($"Enabling Modmap parenting for camera {cam.name}");
#endif
						mapMovementTransformer = cam.transformchain.AddOrGet("ModMapExt", TransformerOrders.ModmapParenting);
					}

					mapMovementTransformer.position = noodleOrigin.localPosition;
					mapMovementTransformer.rotation = noodleOrigin.localRotation;
					return true;
				}
			}
			
			if(mapMovementTransformer != null) {
#if DEBUG
				Plugin.Log.Info($"Disabling Modmap parenting for camera {cam.name}");
#endif
				mapMovementTransformer.position = Vector3.zero;
				mapMovementTransformer.rotation = Quaternion.identity;

				mapMovementTransformer = null;

				noodleOrigin = null;
			}
			return true;
		}
	}
}

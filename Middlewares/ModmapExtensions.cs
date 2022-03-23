using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using HarmonyLib;
using System;
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
		static Type Noodle_PlayerTrack;
		static FieldInfo Noodle_PlayerTrack_Instance;
		static Transform noodleOrigin;
		static MonoBehaviour playertrack_instance = null;

		public static void Reflect() {
			Noodle_PlayerTrack ??= IPA.Loader.PluginManager.GetPluginFromId("NoodleExtensions")?.Assembly.GetType("NoodleExtensions.Animation.PlayerTrack");

			if(Noodle_PlayerTrack != null)
				Noodle_PlayerTrack_Instance = AccessTools.Field(Noodle_PlayerTrack, "_instance");
		}

		private Transformer mapMovementTransformer = null;
		public new bool Pre() {
			// We wanna parent FP cams as well so that the noodle translations are applied instantly and dont get smoothed out by SmoothFollow
			if(
				enabled &&
				HookLeveldata.isModdedMap &&
				(settings.ModmapExtensions.moveWithMap || settings.type != Configuration.CameraType.Positionable)
			) {
				if(Noodle_PlayerTrack_Instance != null) {
					// Was static before, now its a singleton 😡
					// https://github.com/Aeroluna/Heck/commit/6a6030241336f5526854d71a6a6c70ccd82d7468#diff-2929f93d8ad2699fdec005f85284c1c7584562a9d2ba6cee66c765773a3d497bR23

					playertrack_instance = (MonoBehaviour)Noodle_PlayerTrack_Instance.GetValue(null);

					if(playertrack_instance == null && mapMovementTransformer == null)
						return true;
				}

				// Noodle maps do not *necessarily* have a playertrack if it not actually used
				if(playertrack_instance != null) {
					noodleOrigin = playertrack_instance.transform;

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
				playertrack_instance = null;
			}
			return true;
		}
	}
}

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
		//static Type Noodle_PlayerTrack;
		static Transform g_noodleOrigin = null;
		Transform noodleOrigin = null;

		public static void Reflect() {
			//Noodle_PlayerTrack ??= IPA.Loader.PluginManager.GetPluginFromId("NoodleExtensions")?.Assembly.GetType("NoodleExtensions.Animation.PlayerTrack");
		}

		private Transformer mapMovementTransformer = null;
		public new bool Pre() {
			// We wanna parent FP cams as well so that the noodle translations are applied instantly and dont get smoothed out by SmoothFollow
			if(
				enabled &&
				HookLeveldata.isModdedMap &&
				(settings.ModmapExtensions.moveWithMap || settings.type != Configuration.CameraType.Positionable)
			) {
				if(noodleOrigin is null) {
					// Unity momento
					if(g_noodleOrigin == null)
						g_noodleOrigin = null;

					// This stinks just as much as Mawntees fur
					noodleOrigin = g_noodleOrigin ?? (GameObject.Find("NoodlePlayerTrackHead") ?? GameObject.Find("NoodlePlayerTrackRoot"))?.transform;

					g_noodleOrigin = noodleOrigin;
				}

				// Noodle maps do not *necessarily* have a playertrack if it not actually used
				if(noodleOrigin != null) {
					// If we are not yet attached, and we dont have a parent thats active yet, try to get one!
					if(mapMovementTransformer == null) {
#if DEBUG
						Console.WriteLine("Enabling Modmap parenting for camera {0}", cam.name);
#endif
						mapMovementTransformer = cam.transformchain.AddOrGet("ModMapExt", TransformerOrders.ModmapParenting);
					}

					mapMovementTransformer.position = noodleOrigin.localPosition;
					mapMovementTransformer.rotation = noodleOrigin.localRotation;
					return true;
				}
			}

			if(!(noodleOrigin is null)) {
#if DEBUG
				Plugin.Log.Info($"Disabling Modmap parenting for camera {cam.name}");
#endif
				mapMovementTransformer.position = Vector3.zero;
				mapMovementTransformer.rotation = Quaternion.identity;

				noodleOrigin = null;
			}
			return true;
		}
	}
}

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class HookFPFC {
		public static FirstPersonFlyingController instance { get; private set; }
		public static Camera cameraInstance { get; private set; }
		public static bool isInFPFC => instance != null;

		static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
#if DEBUG
			Plugin.Log.Info("FPFC was activated, disabling its camera");
#endif
			instance = __instance;
			cameraInstance = ____camera;

			// Disable the base game camera so that it doesnt cause unnecessary load
			____camera.enabled = false;

			ScenesManager.ActiveSceneChanged();
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(FirstPersonFlyingController), "Start");
			yield return AccessTools.Method(typeof(FirstPersonFlyingController), "OnEnable");
		}

		[HarmonyPatch(typeof(FirstPersonFlyingController), "OnDisable")]
		class HookFPFCOff {
			static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
#if DEBUG
				Plugin.Log.Info("FirstPersonFlyingController.OnDisable()");
#endif
				instance = null;
				cameraInstance = null;

				// When going out of FPFC the camera gets enabled again??
				____camera.enabled = false;

				ScenesManager.ActiveSceneChanged();
			}
		}
	}
}
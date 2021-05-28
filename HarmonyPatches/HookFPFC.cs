using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	static class HookFPFC {
		public static FirstPersonFlyingController instance { get; private set; }
		public static Camera cameraInstance { get; private set; }
		public static bool isInFPFC => instance != null;

		static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
#if DEBUG
			Plugin.Log.Info("FPFC was activated, disabling its camera");
#endif
			instance = __instance;
			cameraInstance = ____camera;

			/*
			 * If I straight up disable the Camera, and theres only the FPFC camera, Camera.main
			 * becomes null (Since Cam2 cameras are also disabled) and this will cause all sorts
			 * of funny issues with some other plugins, so I'll just make it so the cam has to
			 * pretty much render nothing
			 */
			if(CamManager.baseCullingMask == 0)
				CamManager.baseCullingMask = ____camera.cullingMask;

			____camera.cullingMask = 0;

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
				// Not null-ing this as its probably still a valid handle and can be useful in places
				//cameraInstance = null;

				ScenesManager.ActiveSceneChanged();
			}
		}
	}
}
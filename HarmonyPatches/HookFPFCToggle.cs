using Camera2.Managers;
using HarmonyLib;
using IPA.Loader;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	static class HookFPFCToggle {
		public static Transform fpfcTransform { get; private set; } = null;
		public static bool isInFPFC => toggledIntoFPFC && fpfcTransform && fpfcTransform.gameObject && fpfcTransform.gameObject.activeInHierarchy;
		static bool toggledIntoFPFC = false;

		public static bool foundSiraToggle { get; private set; } = false;
		static PropertyInfo FIELD_SimpleCameraController_AllowInput = null;

		public static void SetFPFCActive(Transform transform, bool isActive = true) {
			fpfcTransform = transform;
			toggledIntoFPFC = isActive;

			ScenesManager.ActiveSceneChanged();
		}

		static void Postfix(MonoBehaviour __instance) {
			var allowInput = true;

			if(__instance.transform == fpfcTransform) {
				if(FIELD_SimpleCameraController_AllowInput != null)
					allowInput = (bool)FIELD_SimpleCameraController_AllowInput.GetValue(__instance);

				if(allowInput == toggledIntoFPFC)
					return;
			}

#if DEBUG
			//Plugin.Log.Info(string.Format("HookSiraFPFCToggle: SimpleCameraController.AllowInput => {0}", ___AllowInput));
#endif
			SetFPFCActive(__instance.transform, allowInput);
		}

		static PluginMetadata SiraUtilSimpleCameraController = PluginManager.GetPluginFromId("SiraUtil");
		//TODO: remove next version
		public static readonly bool isSiraSettingLocalPostionYes = SiraUtilSimpleCameraController != null && SiraUtilSimpleCameraController.HVersion > new Hive.Versioning.Version("3.0.5");

		static bool Prepare() => SiraUtilSimpleCameraController != null;

		static MethodBase TargetMethod() {
			var x = SiraUtilSimpleCameraController.Assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");

			var y = x?.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
			FIELD_SimpleCameraController_AllowInput = x?.GetProperty("AllowInput");

			foundSiraToggle = y != null && FIELD_SimpleCameraController_AllowInput != null;

			if(!foundSiraToggle)
				return null;

			return y;
		}

		[HarmonyPatch(typeof(FirstPersonFlyingController), nameof(FirstPersonFlyingController.OnEnable))]
		static class HookBasegameFPFC {
			static void Postfix(Transform ____camera) {
				if(!foundSiraToggle)
					SetFPFCActive(____camera.transform);
			}
		}

		static Exception Cleanup(Exception ex) => null;
	}

//	[HarmonyPatch]
//	static class HookFPFCToggle {
//		public static FirstPersonFlyingController instance { get; private set; }
//		public static Transform fpfcTransform { get; private set; }
//		public static bool isInFPFC => instance != null;

//		public const bool foundSiraToggle = true;
//		public static void SetFPFCActive(Transform t, bool x) { }

//		static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
//#if DEBUG
//			Plugin.Log.Info("FPFC was activated, disabling its camera");
//#endif
//			instance = __instance;
//			fpfcTransform = ____camera.transform;

//			/*
//			 * If I straight up disable the Camera, and theres only the FPFC camera, Camera.main
//			 * becomes null (Since Cam2 cameras are also disabled) and this will cause all sorts
//			 * of funny issues with some other plugins, so I'll just make it so the cam has to
//			 * pretty much render nothing
//			 */
//			if(CamManager.baseCullingMask == 0)
//				CamManager.baseCullingMask = ____camera.cullingMask;

//			____camera.cullingMask = 0;

//			ScenesManager.ActiveSceneChanged();
//		}

//		[HarmonyTargetMethods]
//		static IEnumerable<MethodBase> TargetMethods() {
//			yield return AccessTools.Method(typeof(FirstPersonFlyingController), "Start");
//			yield return AccessTools.Method(typeof(FirstPersonFlyingController), "OnEnable");
//		}

//		[HarmonyPatch(typeof(FirstPersonFlyingController), "OnDisable")]
//		class HookFPFCOff {
//			static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
//#if DEBUG
//				Plugin.Log.Info("FirstPersonFlyingController.OnDisable()");
//#endif
//				instance = null;
//				// Not null-ing this as its probably still a valid handle and can be useful in places
//				//cameraInstance = null;

//				ScenesManager.ActiveSceneChanged();
//			}
//		}
//	}
}
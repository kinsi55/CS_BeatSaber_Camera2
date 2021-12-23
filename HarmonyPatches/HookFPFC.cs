using Camera2.Managers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(FirstPersonFlyingController), "OnEnable")]
	static class HookFPFC {
		public static FirstPersonFlyingController instance { get; private set; }
		public static Transform fpfcTransform { get; private set; }
		public static bool isInFPFC { get; private set; }

		static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
#if DEBUG
			Plugin.Log.Info("FPFC was activated, disabling its camera");
#endif
			instance = __instance;
			fpfcTransform = ____camera.transform;

			if(CamManager.baseCullingMask == 0)
				CamManager.baseCullingMask = ____camera.cullingMask;

			isInFPFC = true;

			ScenesManager.ActiveSceneChanged();
		}

		[HarmonyPatch]
		class HookSiraFPFCToggle {
			static void Postfix(MonoBehaviour __instance, bool value) {
#if DEBUG
				Plugin.Log.Info(string.Format("HookSiraFPFCToggle: SimpleCameraController.AllowInput => {0}", value));
#endif
				isInFPFC = value;
				fpfcTransform = __instance.transform;

				ScenesManager.ActiveSceneChanged();
			}

			static MethodBase TargetMethod() {
				return IPA.Loader.PluginManager.GetPluginFromId("SiraUtil")?
					.Assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController")?
					.GetProperty("AllowInput")?.SetMethod;
			}

			static Exception Cleanup(Exception ex) => null;
		}
	}
}
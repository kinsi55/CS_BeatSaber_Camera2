using Camera2.Managers;
using HarmonyLib;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	static class InitOnMainAvailable {
		static bool isInited = false;
		public static bool useDepthTexture { get; private set; }
		static void Prefix(MainSettingsModelSO ____mainSettingsModel) {
			useDepthTexture = ____mainSettingsModel.depthTextureEnabled;
			foreach(var cam in CamManager.cams.Values) {
				cam.UpdateDepthTextureActive();
			}

			if(!isInited) {
				isInited = true;

				Plugin.Log.Notice("Game is ready, Initializing...");

				CamManager.Init();
			}
		}
	}

	[HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate")]
	static class UpdateCamScreens {
		public static void Prefix(bool firstActivation) {
			if(!firstActivation)
				return;

			foreach(var cam in CamManager.cams.Values)
				cam.UpdateRenderTextureAndView();
		}
	}
}

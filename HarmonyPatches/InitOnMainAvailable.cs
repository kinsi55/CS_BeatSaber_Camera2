using UnityEngine;
using HarmonyLib;
using Camera2.Behaviours;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	static class InitOnMainAvailable {
		static bool isInited = false;
		static void Prefix(MainSettingsModelSO ____mainSettingsModel) {
			foreach(var cam in CamManager.cams.Values)
				if(cam.UCamera != null) cam.UCamera.depthTextureMode = ____mainSettingsModel.depthTextureEnabled ? DepthTextureMode.Depth : DepthTextureMode.None;

			if(!isInited) {
				isInited = true;

				Plugin.Log.Notice("Game is ready, Initializing...");

				CamManager.Init();
			}

			UpdateCamScreens.Prefix(true);
		}
	}

	[HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate")]
	static class UpdateCamScreens {
		public static void Prefix(bool firstActivation) {
			if(!firstActivation)
				return;

			foreach(var cam in CamManager.cams.Values) {
				cam.settings.UpdateViewRect();
				cam.UpdateRenderTextureAndView();
			}
		}
	}
}

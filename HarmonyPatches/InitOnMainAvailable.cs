using UnityEngine;
using HarmonyLib;
using Camera2.Behaviours;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	class InitOnMainAvailable {
		static bool isInited = false;
		static void Prefix(MainSettingsModelSO ____mainSettingsModel) {
			foreach(var cam in CamManager.cams.Values)
				if(cam.UCamera != null) cam.UCamera.depthTextureMode = ____mainSettingsModel.depthTextureEnabled ? DepthTextureMode.Depth : DepthTextureMode.None;

			if(isInited) return;
			isInited = true;

			Plugin.Log.Notice("Game is ready, Initializing...");

			CamManager.Init();
		}
	}
}

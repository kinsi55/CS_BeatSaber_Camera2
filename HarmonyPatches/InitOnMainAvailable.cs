using Camera2.Managers;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
	static class InitOnMainAvailable {
		static bool isInited = false;
		public static bool useDepthTexture { get; private set; }
		static void Postfix(MainSettingsModelSO ____mainSettingsModel) {
			useDepthTexture = ____mainSettingsModel.smokeGraphicsSettings;

			if(!isInited) {
				if(CamManager.baseCullingMask == 0)
					CamManager.baseCullingMask = Camera.main.cullingMask;

				isInited = true;

				Plugin.Log.Notice("Game is ready, Initializing...");

				CamManager.Init();
			} else {
				foreach(var cam in CamManager.cams.Values)
					cam.UpdateDepthTextureActive();
			}
		}
	}
}

using Camera2.Managers;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using BeatSaber.GameSettings;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
	static class InitOnMainAvailable {
		static bool isInited = false;
		public static bool useDepthTexture { get; private set; }
		static void Postfix(SmoothCameraController __instance) {
			if(!isInited) {
				useDepthTexture = false;

				if(SceneUtil.GetMainCameraButReally().GetComponent<DepthTextureController>()._handler.TryGetCurrentPerformancePreset(out var pp))
					useDepthTexture = pp.smokeGraphics; // TODO: Change to .depthTexture when they fixed that its true even with smoke off

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

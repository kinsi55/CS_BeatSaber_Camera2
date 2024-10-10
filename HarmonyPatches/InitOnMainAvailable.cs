using Camera2.Managers;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
	static class InitOnMainAvailable {
		static bool isInited = false;
		static void Postfix(SmoothCameraController __instance) {
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

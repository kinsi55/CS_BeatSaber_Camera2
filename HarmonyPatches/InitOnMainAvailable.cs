using UnityEngine;
using HarmonyLib;
using Camera2.Behaviours;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	class InitOnMainAvailable {
		static bool isInited = false;
		static void Prefix() {
			if(isInited) return;
			isInited = true;

			Plugin.Log.Notice("Game is ready, Initializing...");

			CamManager.Init();

			new GameObject("Cam2_Positioner").AddComponent<CamPositioner>();
		}

	}
}

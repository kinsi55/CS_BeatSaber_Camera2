using UnityEngine;
using HarmonyLib;
using Camera2.Behaviours;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	class InitOnMainAvailable {
		static bool isInited = false;
		static void Prefix() {
			if(isInited) return;
			isInited = true;
			CamManager.Init();

			new GameObject("Cam2_Positioner").AddComponent<CamPositioner>();
		}

	}
}

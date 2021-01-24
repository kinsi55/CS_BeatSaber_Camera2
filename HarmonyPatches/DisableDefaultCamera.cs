using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "ActivateSmoothCameraIfNeeded")]
	class DisableSmoothCamera {
		static bool Prefix() {
			return false;
		}
	}

	[HarmonyPatch(typeof(SmoothCamera), "OnEnable")]
	class BlockSmoothCamera {
		static bool Prefix() {
			return false;
		}
	}
}

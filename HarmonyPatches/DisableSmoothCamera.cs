using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	static class DisableSmoothCamera {
		static bool Prefix() {
#if DEBUG
			Plugin.Log.Info("Prevented Smooth camera from activating");
#endif
			return false;
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded));
			yield return AccessTools.Method(typeof(SmoothCamera), "OnEnable");
		}
	}
}

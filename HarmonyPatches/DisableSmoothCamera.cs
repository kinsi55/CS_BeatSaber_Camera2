using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class DisableSmoothCamera {
		static bool Prefix() {
			return false;
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(SmoothCameraController), "ActivateSmoothCameraIfNeeded");
			yield return AccessTools.Method(typeof(SmoothCamera), "OnEnable");
		}
	}
}

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCamera), nameof(SmoothCamera.OnEnable))]
	static class DisableSmoothCamera {
		static bool Prefix() {
#if DEBUG
			Plugin.Log.Info("Prevented Smooth camera from activating");
#endif
			return false;
		}
	}
}

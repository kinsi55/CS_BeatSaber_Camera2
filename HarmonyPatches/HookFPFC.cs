using HarmonyLib;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(FirstPersonFlyingController), "Start")]
	class HookFPFC {
		public static FirstPersonFlyingController instance { get; private set; }
		static void Postfix(FirstPersonFlyingController __instance, Camera ____camera) {
#if DEBUG
			Plugin.Log.Info("FPFC was activated, disabling its camera");
#endif
			instance = __instance;

			// Disable the base game camera so that it doesnt cause unnecessary load
			____camera.enabled = false;
		}

		[HarmonyPatch(typeof(FirstPersonFlyingController), "OnDisable")]
		class HookFPFCOff {
			static void Postfix(FirstPersonFlyingController __instance) {
				instance = null;
			}
		}
	}

}

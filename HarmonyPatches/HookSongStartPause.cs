using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	/*
	 * Eh, this is picked at random. OnActiveSceneChanged is too early for RecorderCamera to exit and
	 * I cba to dig through obfuscated code because obscurity = security right?
	 */
	//[HarmonyPatch(typeof(PauseMenuManager))]
	//[HarmonyPatch("Awake")]
	[HarmonyPatch(typeof(AudioTimeSyncController))]
	[HarmonyPatch("StartSong")]
	class HookAudioTimeSyncController {
		static void Postfix(AudioTimeSyncController __instance) {
			SceneUtil.SongStarted(__instance);
		}
	}

	[HarmonyPatch]
	class HookAudioTimeSyncController2 {
		static void Postfix() {
			CamManager.ApplyCameraValues(worldCam: true);
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(AudioTimeSyncController), "Pause");
			yield return AccessTools.Method(typeof(AudioTimeSyncController), "Resume");
		}
	}
}

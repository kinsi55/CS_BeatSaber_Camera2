using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Camera2.HarmonyPatches {
	/*
	 * Eh, this is picked at random. OnActiveSceneChanged is too early for RecorderCamera to exit and
	 * I cba to dig through obfuscated code because obscurity = security right?
	 */
	//[HarmonyPatch(typeof(PauseMenuManager))]
	//[HarmonyPatch("Awake")]
	[HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.StartSong))]
	static class HookAudioTimeSyncController {
		static void Postfix(AudioTimeSyncController __instance) {
#if DEBUG
			Plugin.Log.Info("AudioTimeSyncController.StartSong()");
#endif
			SceneUtil.SongStarted(__instance);
		}
	}

	[HarmonyPatch]
	static class HookAudioTimeSyncController2 {
		[HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Pause))]
		[HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Resume))]
		static void Postfix() {
#if DEBUG
			Plugin.Log.Info("AudioTimeSyncController.Pause/Resume()");
#endif
			CamManager.ApplyCameraValues(worldCam: true);
		}
	}
}

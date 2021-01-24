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
	class HookSongStarted {
		static void Postfix() {
			SceneUtil.SongStarted();
		}
	}
}

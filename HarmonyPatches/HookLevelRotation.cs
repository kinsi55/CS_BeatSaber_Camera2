using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(EnvironmentSpawnRotation), "BeatmapEventAtNoteSpawnCallback")]
	class HookLevelRotation {
		public static EnvironmentSpawnRotation Instance { get; private set; }
		static void Postfix(EnvironmentSpawnRotation __instance) {
			Instance = __instance;
		}
	}
}

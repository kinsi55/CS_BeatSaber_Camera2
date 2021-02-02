using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(EnvironmentSpawnRotation), "OnEnable")]
	class HookLevelRotation {
		public static EnvironmentSpawnRotation Instance { get; private set; }
		static void Postfix(EnvironmentSpawnRotation __instance) {
#if DEBUG
			Plugin.Log.Info("Got EnvironmentSpawnRotation instance");
#endif
			Instance = __instance;
		}
	}
}

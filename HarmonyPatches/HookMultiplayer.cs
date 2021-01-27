using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
	class HookMultiplayer {
		public static MultiplayerSessionManager instance { get; private set; }
		static void Postfix(MultiplayerSessionManager __instance) {
			instance = __instance;
			ScenesManager.ActiveSceneChanged();
		}
	}
}

using HarmonyLib;
using Camera2.Managers;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
	class HookMultiplayer {
		public static MultiplayerSessionManager instance { get; private set; }
		static void Postfix(MultiplayerSessionManager __instance) {
#if DEBUG
			Plugin.Log.Info($"Multiplayer connection state changed. Connected: {__instance.isConnected}");
#endif
			instance = __instance;
			ScenesManager.ActiveSceneChanged();
		}
	}
}

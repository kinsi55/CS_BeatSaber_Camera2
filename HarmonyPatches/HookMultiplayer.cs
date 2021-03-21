using HarmonyLib;
using Camera2.Managers;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
	class HookMultiplayer {
		private static MultiplayerSessionManager _instance;
		public static MultiplayerSessionManager instance => _instance == null ? null : _instance;
		static void Postfix(MultiplayerSessionManager __instance) {
#if DEBUG
			Plugin.Log.Info($"Multiplayer connection state changed. Connected: {__instance.isConnected}");
#endif
			_instance = __instance;
			ScenesManager.ActiveSceneChanged();
		}
	}

	[HarmonyPatch(typeof(MultiplayerLocalActivePlayerGameplayManager), nameof(MultiplayerLocalActivePlayerGameplayManager.PerformPlayerFail))]
	class HookMultiplayerFail {
		public static bool hasFailed;
		static void Postfix() {
#if DEBUG
			Plugin.Log.Info("MultiplayerLocalActivePlayerGameplayManager.PerformPlayerFail()");
#endif
			hasFailed = true;
		}
	}
}

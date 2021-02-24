using HarmonyLib;
using Camera2.Managers;
using Camera2.Utils;

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

	// When you die in MP the Origin of the normal camera ends up becoming inactive, thus modmap-parented
	// cameras become inactive too and you get a black screen.
	[HarmonyPatch(typeof(MultiplayerLocalActivePlayerGameplayManager), nameof(MultiplayerLocalActivePlayerGameplayManager.PerformPlayerFail))]
	class HookMultiplayerFail {
		static void Postfix() {
#if DEBUG
			Plugin.Log.Info("MultiplayerLocalActivePlayerGameplayManager.PerformPlayerFail()");
#endif
			SceneUtil.OnSceneMaybeUnloadPre();
		}
	}
}

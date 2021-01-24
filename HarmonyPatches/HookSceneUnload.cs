using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(GameScenesManager), "PopScenes")]
	class HookSceneUnload {
		static void Prefix() {
			SceneUtil.OnSceneMaybeUnloadPre();
		}
	}

	[HarmonyPatch(typeof(GameScenesManager), "ReplaceScenes")]
	class HookSceneReplace {
		static void Prefix() {
			SceneUtil.OnSceneMaybeUnloadPre();
		}
	}
}

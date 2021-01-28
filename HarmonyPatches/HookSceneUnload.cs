using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class HookSceneUnload {
		static void Prefix() {
#if DEBUG
			Plugin.Log.Info("Scene is about to unload!");
#endif
			SceneUtil.OnSceneMaybeUnloadPre();
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(GameScenesManager), "PopScenes");
			yield return AccessTools.Method(typeof(GameScenesManager), "ReplaceScenes");
		}
	}
}

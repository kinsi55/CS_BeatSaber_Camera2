using Camera2.Middlewares;
using Camera2.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(GameScenesManager), "PopScenes")]
	class HookSceneUnload {
		static void Prefix() {
			SceneUtil.OnSceneUnloadPre();
		}
	}

	[HarmonyPatch(typeof(GameScenesManager), "ReplaceScenes")]
	class HookSceneReplace {
		static void Prefix() {
			SceneUtil.OnSceneUnloadPre();
		}
	}
}

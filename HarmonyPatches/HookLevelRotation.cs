using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(EnvironmentSpawnRotation), "BeatmapEventAtNoteSpawnCallback")]
	class HookLevelRotation {
		public static EnvironmentSpawnRotation Instance { get; private set; }
		static void Postfix(EnvironmentSpawnRotation __instance) {
			Instance = __instance;
		}
	}
}

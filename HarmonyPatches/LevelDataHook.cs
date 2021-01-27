using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class LeveldataHook {
		public static IDifficultyBeatmap difficultyBeatmap;
		static void Prefix(IDifficultyBeatmap difficultyBeatmap) {
			LeveldataHook.difficultyBeatmap = difficultyBeatmap;
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			foreach(var t in new Type[] { typeof(StandardLevelScenesTransitionSetupDataSO), typeof(MissionLevelScenesTransitionSetupDataSO), typeof(MultiplayerLevelScenesTransitionSetupDataSO)})
				yield return t.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
		}
	}
}

using Camera2.Utils;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	static class HookLeveldata {
		public static IDifficultyBeatmap difficultyBeatmap;
		public static GameplayModifiers gameplayModifiers;
		public static bool is360Level = false;
		public static bool isModdedMap = false;
		public static bool isWallMap = false;

		[HarmonyPriority(int.MinValue)]
		[HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
		[HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
		[HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
		static void Postfix(IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers) {
#if DEBUG
			Plugin.Log.Info("Got level data!");
#endif
			HookLeveldata.difficultyBeatmap = difficultyBeatmap;
			HookLeveldata.gameplayModifiers = gameplayModifiers;

			isModdedMap = ModMapUtil.IsModdedMap(difficultyBeatmap);
			is360Level = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.containsRotationEvents;
			isWallMap = ModMapUtil.IsProbablyWallmap(difficultyBeatmap);
		}

		internal static void Reset() {
			is360Level = isModdedMap = isWallMap = false;
			difficultyBeatmap = null;
		}
	}
}

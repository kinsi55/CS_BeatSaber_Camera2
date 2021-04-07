using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class HookLeveldata {
		public static IDifficultyBeatmap difficultyBeatmap;
		public static GameplayModifiers gameplayModifiers;
		public static bool is360Level = false;
		public static bool isModdedMap = false;
		public static bool isWallMap = false;

		static SpawnRotationProcessor spawnRotationProcessor = new SpawnRotationProcessor();

		static void Prefix(IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers) {
#if DEBUG
			Plugin.Log.Info("Got level data!");
#endif
			HookLeveldata.difficultyBeatmap = difficultyBeatmap;
			HookLeveldata.gameplayModifiers = gameplayModifiers;

			is360Level = difficultyBeatmap?.beatmapData?.beatmapEventsData.Any(
				x => x.type.IsRotationEvent() && spawnRotationProcessor.RotationForEventValue(x.value) != 0f
			) == true;
			isModdedMap = ModMapUtil.IsModdedMap(difficultyBeatmap);
			isWallMap = ModMapUtil.IsProbablyWallmap(difficultyBeatmap);
		}

		internal static void Reset() {
			is360Level = isModdedMap = isWallMap = false;
			difficultyBeatmap = null;
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			foreach(var t in new Type[] { typeof(StandardLevelScenesTransitionSetupDataSO), typeof(MissionLevelScenesTransitionSetupDataSO), typeof(MultiplayerLevelScenesTransitionSetupDataSO)})
				yield return t.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
		}
	}
}

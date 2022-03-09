using Camera2.Utils;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
	static class HookLeveldata {
		public static IDifficultyBeatmap difficultyBeatmap;
		public static GameplayModifiers gameplayModifiers;
		public static bool is360Level = false;
		public static bool isModdedMap = false;
		public static bool isWallMap = false;

		static SpawnRotationProcessor spawnRotationProcessor = new SpawnRotationProcessor();

		[HarmonyPriority(int.MinValue)]
		[HarmonyPatch(nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
		[HarmonyPatch(nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
		[HarmonyPatch(nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
		static void Postfix(IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers) {
#if DEBUG
			Plugin.Log.Info("Got level data!");
#endif
			HookLeveldata.difficultyBeatmap = difficultyBeatmap;
			HookLeveldata.gameplayModifiers = gameplayModifiers;

			isModdedMap = ModMapUtil.IsModdedMap(difficultyBeatmap);
			is360Level = difficultyBeatmap?.beatmapData?.beatmapEventsData.Any(
				x => x.type.IsRotationEvent() && spawnRotationProcessor.RotationForEventValue(x.value) != 0f
			) == true;
			isWallMap = ModMapUtil.IsProbablyWallmap(difficultyBeatmap);
		}


		//[HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
		//static class threesixtycheck {
		//	// TODO: remove optional thing next update
		//	static bool Prepare() => UnityGame.GameVersion > new AlmostVersion("1.19.1");
			
		//	// this is dumb this is dumb this is dumbis is dumb this is dumb this is dumb why GetBeatmapDataAsync() why hyhwhy
		//	static void Postfix(GameplayCoreSceneSetupData ____sceneSetupData) {
		//		is360Level = ____sceneSetupData.transformedBeatmapData.GetBeatmapDataItems<SpawnRotationBeatmapEventData>().Any(
		//			x => x.rotation != 0f
		//		) == true;
		//	}
		//}

		internal static void Reset() {
			is360Level = isModdedMap = isWallMap = false;
			difficultyBeatmap = null;
		}
	}
}

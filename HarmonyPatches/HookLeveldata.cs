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
		public static BeatmapLevel beatmapLevel;
		public static GameplayModifiers gameplayModifiers;
		public static bool is360Level = false;
		public static bool isModdedMap = false;
		public static bool isWallMap = false;

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			foreach(var m in AccessTools.GetDeclaredMethods(typeof(StandardLevelScenesTransitionSetupDataSO)))
				if(m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init))
					yield return m;

			foreach(var m in AccessTools.GetDeclaredMethods(typeof(MissionLevelScenesTransitionSetupDataSO)))
				if(m.Name == nameof(MissionLevelScenesTransitionSetupDataSO.Init))
					yield return m;

			yield return AccessTools.FirstMethod(
				typeof(MultiplayerLevelScenesTransitionSetupDataSO),
				x => x.Name == "Init"
			);
		}

		[HarmonyPostfix]
		static void Postfix(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, GameplayModifiers gameplayModifiers) {
#if DEBUG
			Plugin.Log.Info("Got level data!");
#endif
			HookLeveldata.beatmapLevel = beatmapLevel;
			HookLeveldata.gameplayModifiers = gameplayModifiers;

			isModdedMap = ModMapUtil.IsModdedMap(beatmapLevel, beatmapKey);
			is360Level = beatmapKey.beatmapCharacteristic.containsRotationEvents;
			isWallMap = ModMapUtil.IsProbablyWallmap(beatmapLevel, beatmapKey);
		}

		internal static void Reset() {
			is360Level = isModdedMap = isWallMap = false;
			beatmapLevel = null;
		}
	}
}

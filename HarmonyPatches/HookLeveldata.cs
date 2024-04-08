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

		[HarmonyPatch(
		typeof(StandardLevelScenesTransitionSetupDataSO),
		nameof(StandardLevelScenesTransitionSetupDataSO.Init),
			new Type[] {
				typeof(string),
				typeof(BeatmapKey),
				typeof(BeatmapLevel),
				typeof(OverrideEnvironmentSettings),
				typeof(ColorScheme),
				typeof(ColorScheme),
				typeof(GameplayModifiers),
				typeof(PlayerSpecificSettings),
				typeof(PracticeSettings),
				typeof(EnvironmentsListModel),
				typeof(AudioClipAsyncLoader),
				typeof(BeatmapDataLoader),
				typeof(string),
				typeof(BeatmapLevelsModel),
				typeof(bool),
				typeof(bool),
				typeof(RecordingToolManager.SetupData?) 
			},
			new ArgumentType[] { 
				ArgumentType.Normal,
				ArgumentType.Ref,
				ArgumentType.Normal, 
				ArgumentType.Normal, 
				ArgumentType.Normal,
				ArgumentType.Normal, 
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal, 
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal
			}
		)]
		[HarmonyPatch(
			typeof(MissionLevelScenesTransitionSetupDataSO),
			nameof(MissionLevelScenesTransitionSetupDataSO.Init),
			new Type[] {
				typeof(string),
				typeof(BeatmapKey),
				typeof(BeatmapLevel),
				typeof(MissionObjective[]),
				typeof(ColorScheme),
				typeof(GameplayModifiers),
				typeof(PlayerSpecificSettings),
				typeof(EnvironmentsListModel),
				typeof(BeatmapLevelsModel),
				typeof(AudioClipAsyncLoader),
				typeof(BeatmapDataLoader),
				typeof(string)
			},
			new ArgumentType[] {
				ArgumentType.Normal,
				ArgumentType.Ref,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal,
				ArgumentType.Normal
			}
		)]
		[HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
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

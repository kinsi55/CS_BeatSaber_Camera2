using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
	class LeveldataHook {
		public static IDifficultyBeatmap difficultyBeatmap;
		static void Prefix(IDifficultyBeatmap difficultyBeatmap) {
			LeveldataHook.difficultyBeatmap = difficultyBeatmap;
		}

		[HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Init")]
		private class LeveldatahookM {
			static void Prefix(IDifficultyBeatmap difficultyBeatmap) {
				LeveldataHook.difficultyBeatmap = difficultyBeatmap;
			}
		}

		[HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
		private class LeveldatahookMp {
			static void Prefix(IDifficultyBeatmap difficultyBeatmap) {
				LeveldataHook.difficultyBeatmap = difficultyBeatmap;
			}
		}
	}
}

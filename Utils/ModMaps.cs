using System.Linq;
using IPA.Loader;

namespace Camera2.Utils {
	static class ModMapUtil {
		static bool isModCapable = false;
		public static void Init() {
			isModCapable =
				PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions") &&
				PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically
		}

		public static bool IsProbablyWallmap(IDifficultyBeatmap map) {
			if(!isModCapable)
				return false;

			// 05:03 Kyle 1413: what if a song has 50,000 fake walls and a single real wall
			// if(map.beatmapData.obstaclesCount != 0 && map.beatmapData.obstaclesCount < 100)
			// 	return false;

			return IsModdedMap(map);
		}

		static bool IsModdedMap(IDifficultyBeatmap map) {
			try {
				return SongCore.Collections.RetrieveDifficultyData(map)?
					.additionalDifficultyData?
					._requirements?.Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
			} catch {
				return false;
			}
		}
	}
}

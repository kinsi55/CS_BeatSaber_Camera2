using IPA.Loader;
using System.Linq;

namespace Camera2.Utils {
	static class ModMapUtil {
		static bool isModCapable =
			PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions");

		static bool hasSongCore =
			PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically

		public static bool IsProbablyWallmap(IDifficultyBeatmap map) {
			if(!isModCapable)
				return false;

			// 05:03 Kyle 1413: what if a song has 50,000 fake walls and a single real wall
			// if(map.beatmapData.obstaclesCount != 0 && map.beatmapData.obstaclesCount < 100)
			// 	return false;

			return IsModdedMap(map);
		}

		public static bool IsModdedMap(IDifficultyBeatmap map) {
			if(!hasSongCore || !isModCapable)
				return false;

			return _IsModdedMap(map);
		}

		// Seperate method so we dont throw if theres no Songcore
		static bool _IsModdedMap(IDifficultyBeatmap map) {
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

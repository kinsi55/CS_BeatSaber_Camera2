using IPA.Loader;
using System.Linq;

namespace Camera2.Utils {
	static class ModMapUtil {
		static bool isModCapable =
			PluginManager.EnabledPlugins.Any(x => x.Name == "MappingExtensions" || x.Name == "NoodleExtensions");

		static bool hasSongCore =
			PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore"); // failsafe, Noodle / MapEx do require it themselves technically

		public static bool IsProbablyWallmap(BeatmapLevel map, BeatmapKey beatmapKey) {
			if(!isModCapable)
				return false;

			// 05:03 Kyle 1413: what if a song has 50,000 fake walls and a single real wall
			// if(map.beatmapData.obstaclesCount != 0 && map.beatmapData.obstaclesCount < 100)
			// 	return false;

			return IsModdedMap(map, beatmapKey);
		}

		public static bool IsModdedMap(BeatmapLevel map, BeatmapKey beatmapKey) {
			if(!hasSongCore || !isModCapable)
				return false;

			return _IsModdedMap(map, beatmapKey);
		}

		// Seperate method so we dont throw if theres no Songcore
		static bool _IsModdedMap(BeatmapLevel map, BeatmapKey beatmapKey) {
			try {
				return map != null && SongCore.Collections.RetrieveDifficultyData(map, beatmapKey)?
					.additionalDifficultyData?
					._requirements?.Any(x => x == "Mapping Extensions" || x == "Noodle Extensions") == true;
			} catch {
				return false;
			}
		}
	}
}

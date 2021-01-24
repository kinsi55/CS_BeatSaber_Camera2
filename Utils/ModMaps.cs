using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SongCore;

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

			if(map.beatmapData.obstaclesCount < 1000)
				return false;

			return IsModdedMap(map);
		}

		static bool IsModdedMap(IDifficultyBeatmap map) {
			return Collections.RetrieveDifficultyData(map)
				.additionalDifficultyData
				._requirements.Any(x => x == "Mapping Extensions" || x == "Noodle Extensions");
		}
	}
}

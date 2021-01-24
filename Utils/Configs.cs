using System;
using System.IO;

namespace Camera2.Utils {
	static class ConfigUtil {
		public static readonly string ConfigDir = Path.Combine(Environment.CurrentDirectory, "UserData/Camera2");
		public static readonly string ScenesCfg = Path.Combine(ConfigDir, "Scenes.json");
		public static readonly string CamsDir = Path.Combine(ConfigDir, "Cameras");

		public static string getCameraPath(string name) {
			return Path.Combine(CamsDir, $"{name}.json");
		}
	}
}

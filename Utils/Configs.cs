using IPA.Utilities;
using System.IO;

namespace Camera2.Utils {
	static class ConfigUtil {
		public static readonly string ConfigDir = Path.Combine(UnityGame.UserDataPath, "Camera2");
		public static readonly string ScenesCfg = Path.Combine(ConfigDir, "Scenes.json");
		public static readonly string CamsDir = Path.Combine(ConfigDir, "Cameras");
		public static readonly string MovementScriptsDir = Path.Combine(ConfigDir, "MovementScripts");

		public static string GetCameraPath(string name) {
			return Path.Combine(CamsDir, $"{name}.json");
		}
		public static string GetMovementScriptPath(string name) {
			return Path.Combine(MovementScriptsDir, $"{name}.json");
		}
	}
}

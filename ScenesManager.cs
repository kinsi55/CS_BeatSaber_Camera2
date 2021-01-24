using Camera2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2 {

	public enum SceneTypes {
		Menu,
		Playing,
		Multiplayer,
		Replay,
		Custom1,
		Custom2,
		Custom3
	}

	class ScenesSettings {
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<SceneTypes, List<string>> scenes = new Dictionary<SceneTypes, List<string>>();

		public bool enable = true;
		public bool dontAutoswitchFromCustom = false;
	}

	static class ScenesManager {
		public static ScenesSettings settings = new ScenesSettings();
		static SceneTypes loadedScene = SceneTypes.Menu;

		public static void ActiveSceneChanged(string sceneName = "MenuCore") {
			if(!settings.dontAutoswitchFromCustom && (loadedScene == SceneTypes.Custom1 || loadedScene == SceneTypes.Custom2 || loadedScene == SceneTypes.Custom3))
				return;

			if(!settings.enable)
				return;

			LoadGameScene(sceneName);
		}

		public static void LoadGameScene(string sceneName = "MenuCore") {
			SceneTypes[] toLookup = null;

			//TODO: Handle MP
			if(sceneName.StartsWith("MenuView") || sceneName == "MenuCore" || sceneName == "Credits") {
				toLookup = new SceneTypes[] { SceneTypes.Menu };
			} else if(sceneName == "GameCore") {
				toLookup = new SceneTypes[] { SceneTypes.Playing };

				if(ScoresaberUtil.IsInReplay())
					toLookup.Prepend(SceneTypes.Replay);
			}

			if(toLookup != null)
				SwitchToCamlist(GetPopulatedScene(toLookup));
		}

		public static void SwitchToScene(SceneTypes scene) {
			loadedScene = scene;

			SwitchToCamlist(settings.scenes[scene]);
		}

		private static void SwitchToCamlist(List<string> cams) {
			if(cams?.Count == 0)
				cams = null;
			/*
			 * Intentionally checking != false, this way if cams is null OR
			 * it contains it, the cam will be activated, only if its
			 * a non-empty scene we want to hide cams that are not in it
			 */
			foreach(var cam in CamManager.cams)
				cam.Value.gameObject.SetActive(cams?.Contains(cam.Key) != false);

			GL.Clear(true, true, Color.black);
		}

		private static List<string> GetPopulatedScene(SceneTypes[] types) {
			if(settings.scenes.Count == 0) return null;

			foreach(var type in types) {
				if(settings.scenes[type].Count() > 0)
					return settings.scenes[type];
			}
			return null;
		}

		public static void Load() {
			void populateMissing() {
				foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
					if(!settings.scenes.ContainsKey(foo))
						settings.scenes.Add(foo, new List<string>());
			}

			if(File.Exists(ConfigUtil.ScenesCfg)) {
				JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg, Encoding.UTF8), settings);
				populateMissing();
			} else {
				populateMissing();
				Save();
			}

			LoadGameScene();
		}

		public static void Save() {
			File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(settings, Formatting.Indented), Encoding.UTF8);
		}
	}
}

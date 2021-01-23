using Camera2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Camera2 {

	public enum SceneTypes {
		Menu,
		Playing,
		Multiplayer,
		Replay
	}

	static class ScenesManager {
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		static public Dictionary<SceneTypes, List<string>> scenes = new Dictionary<SceneTypes, List<string>>();

		public static void SceneLoaded(string sceneName = "MenuCore") {
			List<string> collection = null;
			SceneTypes[] toLookup = null;


			if(sceneName == "MenuViewCore" || sceneName == "MenuCore" || sceneName == "Credits") {
				toLookup = new SceneTypes[] { SceneTypes.Menu };
			} else if(sceneName == "GameCore") {
				toLookup = new SceneTypes[] { SceneTypes.Playing };
				
				if(ScoresaberUtil.IsInReplay())
					toLookup.Prepend(SceneTypes.Replay);
			}

			if(toLookup != null)
				collection = TryGetCollection(toLookup);

			/*
			 * Intentionally checking != false, this way if the respectice scene is empty OR
			 * it contains the cam we're looking for the cam will be activated, only if its
			 * a non-empty scene we want to hide cams that are not in it
			 */
			foreach(var cam in CamManager.cams)
				cam.Value.gameObject.SetActive(collection?.Contains(cam.Key) != false);
		}

		private static List<string> TryGetCollection(SceneTypes[] types) {
			foreach(var type in types) {
				if(scenes[type].Count() > 0)
					return scenes[type];
			}
			return null;
		}

		public static void Load() {
			void populateMissing() {
				foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
					if(!scenes.ContainsKey(foo))
						scenes.Add(foo, new List<string>());
			}

			if(File.Exists(ConfigUtil.ScenesCfg)) {
				JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg, Encoding.UTF8), scenes);
				populateMissing();
			} else {
				populateMissing();
				Save();
			}
		}

		public static void Save() {
			File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(scenes, Formatting.Indented), Encoding.UTF8);
		}
	}
}

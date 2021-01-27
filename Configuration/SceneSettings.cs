using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Camera2.Utils;
using Newtonsoft.Json.Converters;

namespace Camera2.Configuration {
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

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore), JsonConverter(typeof(DictionaryConverter<KeyCode, SceneTypes>))]
		public Dictionary<KeyCode, SceneTypes> sceneBindings = new Dictionary<KeyCode, SceneTypes> {
			{KeyCode.F2, SceneTypes.Custom1},
			{KeyCode.F3, SceneTypes.Custom2},
			{KeyCode.F4, SceneTypes.Custom3}
		};

		public bool enableAutoSwitch = true;
		public bool dontAutoswitchFromCustom = false;
		private bool wasLoaded = false;

		public void Load() {
			void populateMissing() {
				foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
					if(!scenes.ContainsKey(foo))
						scenes.Add(foo, new List<string>());

				wasLoaded = true;
			}

			if(File.Exists(ConfigUtil.ScenesCfg)) {
				JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg), this, JsonHelpers.leanDeserializeSettings);
				populateMissing();
			} else {
				populateMissing();
				Save();
			}

			ScenesManager.LoadGameScene();
		}

		public void Save() {
			if(wasLoaded)
				File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}

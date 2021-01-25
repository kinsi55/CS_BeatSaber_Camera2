using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Camera2.Utils;

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

		public bool enable = true;
		public bool dontAutoswitchFromCustom = false;

		public void Load() {
			void populateMissing()
			{
				foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
					if(!scenes.ContainsKey(foo))
						scenes.Add(foo, new List<string>());
			}

			if(File.Exists(ConfigUtil.ScenesCfg)) {
				JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg, Encoding.ASCII), this, new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore,
					Error = (se, ev) => { ev.ErrorContext.Handled = true; }
				});
				populateMissing();
			} else {
				populateMissing();
				Save();
			}

			ScenesManager.LoadGameScene();
		}

		public void Save() {
			File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.ASCII);
		}
	}
}

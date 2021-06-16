using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Camera2.Utils;
using Camera2.Managers;
using System.Runtime.Serialization;
using BeatSaberMarkupLanguage.GameplaySetup;

namespace Camera2.Configuration {
	public enum SceneTypes {
		Menu,
		MultiplayerMenu,
		Playing,
		Playing360,
		PlayingModmap,
		PlayingMulti,
		SpectatingMulti,
		Replay,
		FPFC
	}

	class ScenesSettings {
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<SceneTypes, List<string>> scenes = new Dictionary<SceneTypes, List<string>>();

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, List<string>> customScenes = new Dictionary<string, List<string>>();

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<KeyCode, string> customSceneBindings = new Dictionary<KeyCode, string>();

		public bool enableAutoSwitch = true;
		public bool autoswitchFromCustom = true;
		private bool wasLoaded = false;

		public void Load() {
			if(File.Exists(ConfigUtil.ScenesCfg)) {
				try {
					JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg), this, JsonHelpers.leanDeserializeSettings);
				} catch(Exception ex) {
					if(!wasLoaded) {
						Plugin.Log.Error($"Failed to load Scenes config, it has been reset:");
						Plugin.Log.Error(ex);

						if(File.Exists($"{ConfigUtil.ScenesCfg}.corrupted"))
							File.Delete($"{ConfigUtil.ScenesCfg}.corrupted");

						File.Move(ConfigUtil.ScenesCfg, $"{ConfigUtil.ScenesCfg}.corrupted");
					} else {
						System.Threading.Tasks.Task.Run(() => WinAPI.MessageBox(IntPtr.Zero, "It seems like the Formatting of your Scenes.json is invalid! It was not loaded.\n\nIf you cant figure out how to fix the formatting you can simply delete it which will recreate it on next load", "Camera2", 0x30));
						return;
					}
				}
			}

			// Populate missing Scenes if the Scenes cfg was outdated
			foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
				if(!scenes.ContainsKey(foo))
					scenes.Add(foo, new List<string>());

			wasLoaded = true;
#if !DEV
			Save();
#endif

			// AAaaaa I hate this being here. This needs to go to some better place IMO
			UI.SpaghettiUI.scenesSwitchUI.Update();

			ScenesManager.LoadGameScene(forceReload: true);
		}

		public void Save() {
			if(wasLoaded)
				File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}

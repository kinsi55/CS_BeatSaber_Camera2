﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Camera2.Utils;
using Camera2.Managers;
using System.Runtime.Serialization;

namespace Camera2.Configuration {
	public enum SceneTypes {
		Menu,
		MultiplayerMenu,
		Playing,
		Playing360,
		PlayingModmap,
		PlayingMulti,
		Replay,
		FPFC
	}

	class ScenesSettings {
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<SceneTypes, List<string>> scenes = new Dictionary<SceneTypes, List<string>>();

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, List<string>> customScenes = new Dictionary<string, List<string>>() {
			{"Custom1", new string[] { "ExampleCamera1", "ExampleCamera2" }.ToList() },
			{"AnotherCustomScene", new string[] { "ExampleCamera3", "ExampleCamera4" }.ToList() }
		};

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<KeyCode, string> customSceneBindings = new Dictionary<KeyCode, string> {
			{KeyCode.F2, "Custom1"},
			{KeyCode.F3, "AnotherCustomScene"}
		};

		public bool enableAutoSwitch = true;
		public bool autoswitchFromCustom = true;
		private bool wasLoaded = false;

		public void Load() {
			if(File.Exists(ConfigUtil.ScenesCfg))
				JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg), this, JsonHelpers.leanDeserializeSettings);

			// Populate missing Scenes if the Scenes cfg was outdated
			foreach(SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
				if(!scenes.ContainsKey(foo))
					scenes.Add(foo, new List<string>());

			wasLoaded = true;
#if !DEV
			Save();
#endif

			ScenesManager.LoadGameScene(forceReload: true);
		}

		public void Save() {
			if(wasLoaded)
				File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}

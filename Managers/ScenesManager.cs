using Camera2.Configuration;
using Camera2.HarmonyPatches;
using Camera2.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Camera2.Managers {

#if DEBUG
	public
#endif
	static class ScenesManager {
		internal static ScenesSettings settings { get; private set; } = new ScenesSettings();

		// Kind of a hack not having it start off Menu but else the first menu load will not apply..
		internal static SceneTypes loadedScene { get; private set; } = SceneTypes.MultiplayerMenu;
		internal static bool isOnCustomScene = false;

		public static void ActiveSceneChanged(string sceneName = null) {
			if(SceneUtil.currentScene == null)
				return;

			if(sceneName == null)
				sceneName = SceneUtil.currentScene.name;

			if(CamManager.customScreen == null)
				return;

			CamManager.customScreen.gameObject.SetActive(sceneName != "BeatmapEditor");

			if(!settings.autoswitchFromCustom && isOnCustomScene)
				return;

#if DEBUG
			Plugin.Log.Info($"ActiveSceneChanged({sceneName}) - Current loadedScene: {loadedScene}");
#endif

			LoadGameScene(sceneName);
		}

		public static void LoadGameScene(string sceneName = null, bool forceReload = false) {
			if(sceneName == null)
				sceneName = SceneUtil.currentScene.name;

			List<SceneTypes> toLookup = new List<SceneTypes> { SceneTypes.Menu };

			if(SceneUtil.menuSceneNames.Contains(sceneName)) {
				if(SceneUtil.isInMultiplayer)
					toLookup.Insert(0, SceneTypes.MultiplayerMenu);
			} else if(sceneName == "GameCore") {
				toLookup.Insert(0, SceneTypes.Playing);

				if(HookLeveldata.isModdedMap) {
					toLookup.Insert(0, SceneTypes.PlayingModmap);
				} else if(HookLeveldata.is360Level) {
					toLookup.Insert(0, SceneTypes.Playing360);
				}

				if(ScoresaberUtil.IsInReplay()) {
					toLookup.Insert(0, SceneTypes.Replay);
				} else if(SceneUtil.isInMultiplayer) {
					toLookup.Insert(0, SceneTypes.PlayingMulti);

					if(HookMultiplayerSpectatorController.instance != null)
						toLookup.Insert(0, SceneTypes.SpectatingMulti);
				}
			}

			if(HookFPFCToggle.isInFPFC)
				toLookup.Insert(0, SceneTypes.FPFC);

#if DEBUG
			Plugin.Log.Info($"LoadGameScene -> {string.Join(", ", toLookup)}");
#endif
			SwitchToScene(FindSceneToUse(toLookup.ToArray()), forceReload);
		}

		public static void SwitchToScene(SceneTypes scene, bool forceReload = false) {
			if(!settings.scenes.ContainsKey(scene))
				return;

#if DEBUG
			Plugin.Log.Info($"Switching to scene {scene}");
			Plugin.Log.Info($"Cameras: {string.Join(", ", settings.scenes[scene])}");
#endif
			if(loadedScene == scene && !forceReload && !isOnCustomScene)
				return;

			loadedScene = scene;

			var toLoad = settings.scenes[scene];

			if(scene == SceneTypes.Menu && toLoad.Count == 0)
				toLoad = CamManager.cams.Keys.ToList();

			SwitchToCamlist(toLoad);
			isOnCustomScene = false;
			UI.SpaghettiUI.scenesSwitchUI.Update(0, false);
		}

		public static void SwitchToCustomScene(string name) {
			if(!settings.customScenes.TryGetValue(name, out var s))
				return;

			if(!s.Any(CamManager.cams.ContainsKey))
				return;

			isOnCustomScene = true;

			SwitchToCamlist(s);
		}

		private static void SwitchToCamlist(List<string> cams) {
			if(cams?.Count == 0)
				cams = null;

			/*
			 * Intentionally checking != false, this way if cams is null OR
			 * it contains it, the cam will be activated, only if its
			 * a non-empty scene we want to hide cams that are not in it
			 */
			foreach(var cam in CamManager.cams) {
				if(cam.Value == null)
					continue;

				var camShouldBeActive = cams?.Contains(cam.Key) != false || UI.SettingsView.cam == cam.Value;

				cam.Value.gameObject.SetActive(camShouldBeActive);
			}

			GL.Clear(true, true, Color.black);

			GlobalFPSCap.ApplyFPSCap();
		}

		private static SceneTypes FindSceneToUse(SceneTypes[] types) {
			if(settings.scenes.Count == 0)
				return SceneTypes.Menu;

			foreach(var type in types) {
				if(settings.scenes[type].Any(CamManager.cams.ContainsKey))
					return type;
			}
			return SceneTypes.Menu;
		}
	}
}

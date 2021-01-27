using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Camera2.Configuration;
using Camera2.Utils;

namespace Camera2 {

	static class ScenesManager {
		internal static ScenesSettings settings { get; private set; } = new ScenesSettings();
		static SceneTypes loadedScene = SceneTypes.Menu;

		public static void ActiveSceneChanged(string sceneName = "MenuCore") {
			if(!settings.dontAutoswitchFromCustom && (loadedScene == SceneTypes.Custom1 || loadedScene == SceneTypes.Custom2 || loadedScene == SceneTypes.Custom3))
				return;

			if(!settings.enableAutoSwitch)
				return;

			LoadGameScene(sceneName);
		}

		public static void LoadGameScene(string sceneName = "MenuCore") {
			SceneTypes[] toLookup = null;

			//TODO: Handle MP
			if(SceneUtil.menuSceneNames.Contains(sceneName) || sceneName == "Credits") {
				toLookup = new SceneTypes[] { SceneTypes.Menu };
			} else if(sceneName == "GameCore") {
				toLookup = new SceneTypes[] { SceneTypes.Playing };

				if(ScoresaberUtil.IsInReplay())
					toLookup.Prepend(SceneTypes.Replay);
			}

			if(toLookup != null) {
				var targetList = GetPopulatedScene(toLookup);
				if(targetList != null)
					SwitchToScene((SceneTypes)targetList);
			}
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

		private static SceneTypes? GetPopulatedScene(SceneTypes[] types) {
			if(settings.scenes.Count == 0) return null;

			foreach(var type in types) {
				if(settings.scenes[type].Count() > 0)
					return type;
			}
			return null;
		}
	}
}

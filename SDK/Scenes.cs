using Camera2.Configuration;
using Camera2.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2.SDK {
	public static class Scenes {
		/// <summary>
		/// List of Scenes and what cameras belong to them
		/// </summary>
		public static IReadOnlyDictionary<SceneTypes, IReadOnlyList<string>> scenes =>
			(IReadOnlyDictionary<SceneTypes, IReadOnlyList<string>>)ScenesManager.settings.scenes
			.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

		/// <summary>
		/// List of Scenes and what cameras belong to them
		/// </summary>
		public static IReadOnlyDictionary<string, IReadOnlyList<string>> customScenes =>
			(IReadOnlyDictionary<string, IReadOnlyList<string>>)ScenesManager.settings.customScenes
			.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

		/// <summary>
		/// The currently loaded scene. Must not necessarily represent the targeted scene
		/// (E.g. Playing) incase the Playing scene was empty and Cam2 resorted to using Menu
		/// instead, etc.
		/// </summary>
		public static SceneTypes current => ScenesManager.loadedScene;

		/// <summary>
		/// Switches to the requested scene. If the scene you try to switch to does not have
		/// any cameras assigned no action will be taken.
		/// </summary>
		/// <param name="scene">Scene to switch to</param>
		public static void SwitchToScene(SceneTypes scene) {
			if(ScenesManager.settings.scenes[scene].Count > 0)
				ScenesManager.SwitchToScene(scene, true);
		}

		/// <summary>
		/// Switches to the requested custom scene. If the scene you try to switch to does not have
		/// any cameras assigned no action will be taken.
		/// </summary>
		/// <param name="scene">Scene to switch to</param>
		public static void SwitchToCustomScene(string sceneName) {
			ScenesManager.SwitchToCustomScene(sceneName);
		}

		/// <summary>
		/// Switches to whatever scene *should* be active right now, assuming it is overriden
		/// </summary>
		public static void ShowNormalScene() {
			ScenesManager.LoadGameScene(forceReload: true);
		}
	}
}

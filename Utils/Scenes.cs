using Camera2.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2.Utils {
	static class SceneUtil {
		public static Scene currentScene;
		public static bool isInMenu = true;
		public static Transform songWorldTransform;

		public static void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
			currentScene = newScene;
			isInMenu = newScene.name == "MenuViewControllers";
			ScenesManager.ActiveSceneChanged(newScene.name);
			ScoresaberUtil.UpdateIsInReplay();
		}

		public static void OnSceneUnloadPre() {
			NoodleExtensions.ForceDetachTracks();
			songWorldTransform = null;
		}

		public static void SongStarted() {
			ScoresaberUtil.UpdateIsInReplay();

			songWorldTransform = GameObject.Find("LocalPlayerGameCore/Origin")?.transform;
		}
	}
}

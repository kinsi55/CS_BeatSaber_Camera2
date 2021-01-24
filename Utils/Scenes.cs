using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Camera2.HarmonyPatches;
using Camera2.Middlewares;
using VRUIControls;
using Camera2.Behaviours;

namespace Camera2.Utils {
	static class SceneUtil {
		public static Scene currentScene;
		public static bool isInMenu = true;
		public static Transform songWorldTransform;
		public static bool isProbablyInWallMap = false;

		public static readonly string[] menuSceneNames = new string[] { "MenuViewCore", "MenuCore", "MenuViewControllers" };

		public static void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
			currentScene = newScene;
			isInMenu = menuSceneNames.Contains(newScene.name);

			if(currentScene.name != "GameCore") {
				isProbablyInWallMap = false;
			} else {
				isProbablyInWallMap = ModMapUtil.IsProbablyWallmap(LeveldataHook.difficultyBeatmap);
			}

			ScenesManager.ActiveSceneChanged(newScene.name);
			ScoresaberUtil.UpdateIsInReplay();
		}

		public static void OnSceneMaybeUnloadPre() {
			ModmapExtensions.ForceDetachTracks();
			songWorldTransform = null;
		}

		public static void SongStarted() {
			ScoresaberUtil.UpdateIsInReplay();

			songWorldTransform = GameObject.Find("LocalPlayerGameCore/Origin")?.transform;

			TransparentWalls.MakeWallsOpaqueForMainCam();
		}
	}
}

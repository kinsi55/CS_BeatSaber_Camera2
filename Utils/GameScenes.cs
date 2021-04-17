using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Camera2.HarmonyPatches;
using Camera2.Middlewares;
using Camera2.Managers;

namespace Camera2.Utils {
	static class SceneUtil {
		public static Scene currentScene { get; private set; }
		public static bool isInMenu { get; private set; } = true;
		public static bool isInSong { get; private set; } = false;
		public static Transform songWorldTransform { get; private set; }

		public static AudioTimeSyncController audioTimeSyncController { get; private set; }

		public static bool hasSongPlayer => audioTimeSyncController != null;
		public static bool isSongPlaying => hasSongPlayer && audioTimeSyncController.state == AudioTimeSyncController.State.Playing;


		public static bool isInMultiplayer => HookMultiplayer.instance?.isConnected == true;

		public static GameObject GetMainCameraButReally() => Camera.main?.gameObject ?? GameObject.FindGameObjectsWithTag("MainCamera")[0];


		public static readonly string[] menuSceneNames = new string[] { "MenuViewCore", "MenuCore", "MenuViewControllers" };

		public static void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
			if(newScene == null)
				return;

			currentScene = newScene;
			isInSong = newScene.name == "GameCore";
			isInMenu = !isInSong && menuSceneNames.Contains(newScene.name);
			HookMultiplayerFail.hasFailed = false;

			if(oldScene.name == "GameCore") {
#if DEBUG
				Plugin.Log.Info("oldScene = GameCore, resetting stuffs in SceneUtil");
#endif

				ScoresaberUtil.isInReplay = false;
				HookLeveldata.Reset();
				audioTimeSyncController = null;
				songWorldTransform = null;
			}

			ScenesManager.ActiveSceneChanged();

			// Updating the bitmask on scene change to allow for things like the auto wall toggle
			CamManager.ApplyCameraValues(bitMask: true, worldCam: true);
		}

		public static void SongStarted(AudioTimeSyncController controller) {
			audioTimeSyncController = controller;
			ScoresaberUtil.UpdateIsInReplay();
			
			// FPFCToggle compatible way of retreiving the origin
			songWorldTransform = GameObject.Find("Origin/VRGameCore")?.transform.parent;

			TransparentWalls.MakeWallsOpaqueForMainCam();
			CamManager.ApplyCameraValues(worldCam: true);
		}
	}
}

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
		public static Transform songWorldTransform { get; private set; }
		public static bool isProbablyInWallMap { get; private set; } = false;

		public static AudioTimeSyncController audioTimeSyncController { get; private set; }

		public static bool isInSong {
			get { return audioTimeSyncController != null; }
		}
		public static bool isSongPlaying {
			get {
				return isInSong && audioTimeSyncController.state == AudioTimeSyncController.State.Playing;
			}
		}
		
		public static bool isInMultiplayer {
			get {
				return HookMultiplayer.instance != null && HookMultiplayer.instance.isConnected;
			}
		}


		public static readonly string[] menuSceneNames = new string[] { "MenuViewCore", "MenuCore", "MenuViewControllers" };

		public static void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
			currentScene = newScene;
			isInMenu = menuSceneNames.Contains(newScene.name);

			if(oldScene.name == "GameCore" && ScoresaberUtil.isInReplay)
				ScoresaberUtil.isInReplay = false;

			if(newScene.name != "GameCore") {
				isProbablyInWallMap = false;
				audioTimeSyncController = null;
			} else {
				isProbablyInWallMap = ModMapUtil.IsProbablyWallmap(HookLeveldata.difficultyBeatmap);
			}

			ScenesManager.ActiveSceneChanged(newScene.name);

			// Updating the bitmask on scene change to allow for the auto wall toggle
			CamManager.ApplyCameraValues(bitMask: true, worldCam: true);
		}

		public static void OnSceneMaybeUnloadPre() {
			ModmapExtensions.ForceDetachTracks();
			songWorldTransform = null;
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

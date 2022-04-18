using Camera2.HarmonyPatches;
using Camera2.Managers;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2.Utils {
	static class SceneUtil {
		public static Scene currentScene { get; private set; }
		public static bool isInMenu { get; private set; } = true;
		public static bool isInSong { get; private set; } = false;

		public static AudioTimeSyncController audioTimeSyncController { get; private set; }

		public static bool hasSongPlayer => audioTimeSyncController != null;
		public static bool isSongPlaying => hasSongPlayer && audioTimeSyncController.state == AudioTimeSyncController.State.Playing;


		public static bool isInMultiplayer => HookMultiplayer.instance != null && HookMultiplayer.instance.isConnected == true;

		public static GameObject GetMainCameraButReally() {
			var a = Camera.main;

			if(a == null)
				return GameObject.FindGameObjectsWithTag("MainCamera")[0];

			return a.gameObject;
		}


		public static readonly string[] menuSceneNames = new string[] { "MainMenu", "MenuViewCore", "MenuCore", "MenuViewControllers" };

		public static void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
			if(newScene == null)
				return;

			currentScene = newScene;
			isInSong = newScene.name == "GameCore";
			isInMenu = !isInSong && menuSceneNames.Contains(newScene.name);

			if(oldScene.name == "GameCore") {
#if DEBUG
				Plugin.Log.Info("oldScene = GameCore, resetting stuffs in SceneUtil");
#endif

				ScoresaberUtil.isInReplay = false;
				HookLeveldata.Reset();
				audioTimeSyncController = null;
			}

			ScenesManager.ActiveSceneChanged();

			// Updating the bitmask on scene change to allow for things like the auto wall toggle
			CamManager.ApplyCameraValues(bitMask: true, worldCam: true, posRot: true);

			GlobalFPSCap.ApplyFPSCap();
		}

		public static void SongStarted(AudioTimeSyncController controller) {
			audioTimeSyncController = controller;
			ScoresaberUtil.UpdateIsInReplay();

			TransparentWalls.MakeWallsOpaqueForMainCam();
			CamManager.ApplyCameraValues(worldCam: true);

			if(CamManager.cams.Values.Any(x => !x.settings.visibleObjects.Floor)) {
				// Move the plattform stuff to the correct layer because beat games didnt
				foreach(var x in (new string[] { "Construction", "Frame", "RectangleFakeGlow" }).Select(x => GameObject.Find($"Environment/PlayersPlace/{x}")))
					if(x != null) x.layer = (int)VisibilityLayers.PlayerPlattform;
			}
		}
	}
}

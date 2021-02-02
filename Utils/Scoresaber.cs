using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	static class ScoresaberUtil {
		static Type ReplayPlayer;
		static PropertyInfo ReplayPlayer_playbackEnabled;

		//static Type Shared;
		//static PropertyInfo Shared_settings;

		//static Type Settings;
		//static PropertyInfo ReplayPlayer_replayCameraXOffset;
		//static PropertyInfo ReplayPlayer_replayCameraYOffset;
		//static PropertyInfo ReplayPlayer_replayCameraZOffset;

		//static PropertyInfo ReplayPlayer_replayCameraXRotation;
		//static PropertyInfo ReplayPlayer_replayCameraYRotation;
		//static PropertyInfo ReplayPlayer_replayCameraZRotation;

		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static void Init() {
			ReplayPlayer = AccessTools.TypeByName("ScoreSaber.ReplayPlayer");
			ReplayPlayer_playbackEnabled = ReplayPlayer?.GetProperty("playbackEnabled", BindingFlags.Public | BindingFlags.Instance);

			/*
			 * I was gonna implement logic to allow ignoring the Scoresaber Replay camera offset but quite honestly I cba
			 * If SS at least saved the offset in Vectors instead of 50 variables I might've
			 */
			//Shared = AccessTools.TypeByName("ScoreSaber.Shared");
			//Shared_settings = ReplayPlayer?.GetProperty("settings", BindingFlags.Public | BindingFlags.Static);

			//Settings = AccessTools.TypeByName("ScoreSaber.Core.Data.Settings");
		}

		public static bool IsInReplay() {
			if(ReplayPlayer_playbackEnabled == null)
				return false;

			var x = (MonoBehaviour)Resources.FindObjectsOfTypeAll(ReplayPlayer).LastOrDefault();

			return x?.isActiveAndEnabled == true && (bool)ReplayPlayer_playbackEnabled.GetValue(x);
		}

		public static void UpdateIsInReplay() {
			isInReplay = IsInReplay();
			replayCamera = null;
			if(isInReplay)
				replayCamera = GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

			if(replayCamera != null) {
				replayCamera.enabled = false;

				// Why theres a clone (Making it two cameras) that actually renders shit? Idk lol
				var x = GameObject.Find("RecorderCamera(Clone)")?.GetComponent<Camera>();
				if(x != null) x.enabled = false;
			}
#if DEBUG
			Plugin.Log.Info($"UpdateIsInReplay() -> isInReplay: {isInReplay}, replayCamera: {replayCamera}");
#endif
		}
	}
}

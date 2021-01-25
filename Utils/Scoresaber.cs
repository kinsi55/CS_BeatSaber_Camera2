using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	static class ScoresaberUtil {
		static Type ReplayPlayer;
		static PropertyInfo ReplayPlayer_playbackEnabled;
		
		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static void Init() {
			ReplayPlayer = AccessTools.TypeByName("ScoreSaber.ReplayPlayer");
			ReplayPlayer_playbackEnabled = ReplayPlayer?.GetProperty("playbackEnabled", BindingFlags.Public | BindingFlags.Instance);
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
				replayCamera = GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponentInChildren<Camera>();

			if(replayCamera != null)
				replayCamera.enabled = false;
		}
	}
}

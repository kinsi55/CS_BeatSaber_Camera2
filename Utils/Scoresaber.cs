using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Utils {
	static class ScoresaberUtil {
		static Type ReplayPlayer;
		static PropertyInfo ReplayPlayer_playbackEnabled;
		
		public static bool isInReplay { get; private set; }
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

			//GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera").gameObject.GetComponentInChildren<Camera>().gameObject.SetActive(false)
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

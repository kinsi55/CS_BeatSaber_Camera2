using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	static class ScoresaberUtil {
		static Type ReplayPlayer = AccessTools.TypeByName("ScoreSaber.ReplayPlayer");
		static PropertyInfo ReplayPlayer_playbackEnabled = ReplayPlayer?.GetProperty("playbackEnabled", BindingFlags.Public | BindingFlags.Instance);
		static FieldInfo ReplayPlayer_instance = ReplayPlayer?.GetField("instance", BindingFlags.Public | BindingFlags.Static);

		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static bool IsInReplay() {
			if(ReplayPlayer_playbackEnabled == null)
				return false;

			var x = (MonoBehaviour)ReplayPlayer_instance.GetValue(null);

			return x?.isActiveAndEnabled == true && (bool)ReplayPlayer_playbackEnabled.GetValue(x);
		}

		public static void UpdateIsInReplay() {
			isInReplay = IsInReplay();
			replayCamera = null;
			if(isInReplay)
				replayCamera = GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

			if(replayCamera != null) {
				// Cant disable this one as otherwise SS' ReplayFrameRenderer stuff "breaks"
				//replayCamera.enabled = false;

				if(!UnityEngine.XR.XRDevice.isPresent) {
					var x = GameObject.Find("RecorderCamera(Clone)")?.GetComponent<Camera>();
					if(x != null) x.enabled = false;
				}
			}
#if DEBUG
			Plugin.Log.Info($"UpdateIsInReplay() -> isInReplay: {isInReplay}, replayCamera: {replayCamera}");
#endif
		}
	}
}

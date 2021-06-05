using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	public static class ScoresaberUtil {
		static MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");

		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static bool IsInReplay() {
			return ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false;
		}

		public static void UpdateIsInReplay() {
			isInReplay = IsInReplay();
			replayCamera = !isInReplay ? null : GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

			if(replayCamera != null) {
				var x = GameObject.Find("RecorderCamera(Clone)")?.GetComponent<Camera>();

				// Cant disable this one as otherwise SS' ReplayFrameRenderer stuff "breaks"
				//replayCamera.enabled = false;
				if(x != null) {
					replayCamera.tag = "Untagged";

					if(!UnityEngine.XR.XRDevice.isPresent)
						x.enabled = false;

					x.tag = "MainCamera";
				}
			}
#if DEBUG
			Plugin.Log.Info($"UpdateIsInReplay() -> isInReplay: {isInReplay}, replayCamera: {replayCamera}");
#endif
		}
	}
}

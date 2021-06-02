using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	static class ScoresaberUtil {
		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static bool IsInReplay() {
			return GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.activeInHierarchy == true;
		}

		public static void UpdateIsInReplay() {
			isInReplay = IsInReplay();
			replayCamera = !isInReplay ? null : GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

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

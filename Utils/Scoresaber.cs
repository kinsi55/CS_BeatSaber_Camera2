using Camera2.HarmonyPatches;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Camera2.Utils {
	public static class ScoresaberUtil {
		static MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");

		public static bool isInReplay { get; internal set; }
		public static Camera replayCamera { get; private set; }

		public static Transform spectateParent { get; private set; }

		public static bool IsInReplay() {
			try {
				return ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false;
			} catch { }
			return false;
		}

		public static void UpdateIsInReplay() {
			var wasInReplay = isInReplay;

			isInReplay = IsInReplay();
			replayCamera = !isInReplay ? null : GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

			if(replayCamera != null) {
				var x = GameObject.Find("RecorderCamera(Clone)")?.GetComponent<Camera>();

				// Cant disable this one as otherwise SS' ReplayFrameRenderer stuff "breaks"
				//replayCamera.enabled = false;
				if(x != null) {
					replayCamera.tag = "Untagged";


					/*
					 * When a replay was just started, the VRCenterAdjust isnt set up "correctly" and Cam2 has no idea about
					 * the offset applied for the VR-Spectator. This is mainly relevant when having "Follow replay position"
					 * off as then the camera would be too far forward, assuming default settings, until you open the Pause
					 * menu or the Replay UI ingame
					 * This is super hacky trash but it does the job for now™
					 * I'm not exactly sure why I *need* to look it up again here, else it wont work - whatever.
					 */
					if(!wasInReplay) {
						var y = GameObject.Find("SpectatorParent/RecorderCamera(Clone)");

						spectateParent = y?.transform.parent;
					}

					if(!UnityEngine.XR.XRDevice.isPresent)
						x.enabled = false;

					// Doing this so other plugins that rely on Camera.main dont die
					x.tag = "MainCamera";
				}
			}
#if DEBUG
			Plugin.Log.Info($"UpdateIsInReplay() -> isInReplay: {isInReplay}, replayCamera: {replayCamera}");
#endif
		}
	}
}

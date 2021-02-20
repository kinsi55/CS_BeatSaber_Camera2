using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Camera2.Interfaces;
using Camera2.Utils;
using Camera2.HarmonyPatches;

namespace Camera2.Configuration {
	class Settings_Smoothfollow : CameraSubSettings {
		public float position = 10f;
		public float rotation = 5f;

		public bool forceUpright = false;
		public bool followReplayPosition = true;
		
		private bool _pivotingOffset = true;
		[JsonProperty("pivotingOffset")]
		public bool pivotingOffset {
			get { return _pivotingOffset; }
			set { _pivotingOffset = value; if(settings.isLoaded) settings.ApplyPositionAndRotation(); }
		}

		[JsonIgnore]
		internal Transform parent;
	}
}

namespace Camera2.Middlewares {
	class Smoothfollow : CamMiddleware, IMHandler {
		Scene lastScene;

		Transform parent { get { return settings.Smoothfollow.parent; } set { settings.Smoothfollow.parent = value; } }

		new public bool Pre() {
			if(settings.type == Configuration.CameraType.Positionable)
				return true;

			var parentToUse = parent;

			if(settings.Smoothfollow.followReplayPosition && ScoresaberUtil.isInReplay) {
				parentToUse = ScoresaberUtil.replayCamera.transform;
			} else if(parentToUse == null || !parentToUse.gameObject.activeInHierarchy) {
				parent = parentToUse = (Camera.main ?? HookFPFC.cameraInstance ?? null)?.transform;
			}

			//System.Console.WriteLine("FP cam is attached to {0}", parentToUse);
			
			// If we dont have a parent we should not render.
			if(parentToUse == null)
				return false;

			var targetRotation = parentToUse.rotation;
			var targetPosition = parentToUse.position;

			if(settings.Smoothfollow.forceUpright) {
				float zVal;
				if(SceneUtil.songWorldTransform != null) {
					/*
					 * Substract the world rotation so that the only thing we "correct" for being upright is the HMD
					 * E.g. Map turns you upside down - The view should still be upside down, but "upright" (No rotation other than the maps one)
					 */
					var isolatedHmdRotation = targetRotation * Quaternion.Inverse(SceneUtil.songWorldTransform.rotation);
					zVal = isolatedHmdRotation.eulerAngles.z;
				} else {
					zVal = targetRotation.eulerAngles.z;
				}

				targetRotation *= Quaternion.Euler(0, 0, -zVal);
			}

			if(!settings.Smoothfollow.pivotingOffset) {
				targetRotation *= Quaternion.Euler(settings.targetRot);
				targetPosition += settings.targetPos;
			}

			var theTransform = cam.transform;// settings.Smoothfollow.pivotingOffset ? cam.transform : cam.UCamera.transform;

			// If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
			if(lastScene != SceneUtil.currentScene || (HookFPFC.instance?.enabled == true && (!ScoresaberUtil.isInReplay || !settings.Smoothfollow.followReplayPosition))) {
				theTransform.SetPositionAndRotation(targetPosition, targetRotation);

				lastScene = SceneUtil.currentScene;
			} else {
				theTransform.position = Vector3.Lerp(theTransform.position, targetPosition, cam.timeSinceLastRender * settings.Smoothfollow.position);
				theTransform.rotation = Quaternion.Slerp(theTransform.rotation, targetRotation, cam.timeSinceLastRender * settings.Smoothfollow.rotation);
			}
			return true;
		}
	}
}

using System.Linq;
using UnityEngine;
using Camera2.Interfaces;
using Camera2.Utils;
using static Camera2.Configuration.MovementScript;
using Camera2.Configuration;
using Camera2.Managers;

namespace Camera2.Configuration {
	class Settings_MovementScript : CameraSubSettings {
		public string[] scriptList = new string[] { };
		public bool fromOrigin = true;
		public bool enableInMenu = false;
	}
}

namespace Camera2.Middlewares {
	class MovementScriptProcessor : CamMiddleware, IMHandler {
		static System.Random randomSource = new System.Random();

		Transform scriptTransform = null;

		MovementScript loadedScript = null;
		float currentAnimationTime = 0f;


		int frameIndex = 0;

		float lastFov = 0f;
		Vector3 lastPos = Vector3.zero;
		Quaternion lastRot = Quaternion.identity;

		Frame targetFrame { get { return loadedScript.frames[frameIndex]; } }
		
		private void DoParent() {
			if(scriptTransform != null)
				return;

			var newScriptTransform = new GameObject($"Cam2_MovementScriptApplier_{cam.name}");
			DontDestroyOnLoad(newScriptTransform);

			scriptTransform = newScriptTransform.transform;

#if DEBUG
			Plugin.Log.Info($"Reparenting camera {cam.name} to allow for Movement scripts");
#endif

			scriptTransform.parent = cam.UCamera.transform.parent;
			scriptTransform.gameObject.name = "MovementScriptApplier";

			var pos = cam.UCamera.transform.localPosition;
			var rot = cam.UCamera.transform.localRotation;

			scriptTransform.localPosition = pos;
			scriptTransform.localRotation = rot;

			cam.UCamera.transform.parent = scriptTransform;
		}

		private void Reset() {
			if(loadedScript == null)
				return;

			if(scriptTransform != null) {
				scriptTransform.localPosition = lastPos = Vector3.zero;
				scriptTransform.localRotation = lastRot = Quaternion.identity;
			}
			loadedScript = null;
			currentAnimationTime = 0f;
			frameIndex = 0;
			lastFov = 0f;
			cam.UCamera.fieldOfView = settings.FOV;
#if DEBUG
			Plugin.Log.Info($"Resetting MovementScriptProcessor of camera {cam.name}");
#endif
		}

		new public void CamConfigReloaded() {
			if(loadedScript == null && settings.MovementScript.fromOrigin)
				return;
			// Having a custom position on a camera thats executing a movement script is PROBABLY not what the user wants
			cam.transform.localPosition = Vector3.zero;
			cam.transform.localRotation = Quaternion.identity;
		}

		public void OnDisable() {
			Reset();
		}

		new public bool Pre() {
			if(settings.MovementScript.scriptList.Length == 0 || 
				(!SceneUtil.isInSong && !settings.MovementScript.enableInMenu) || 
				cam.settings.type != Configuration.CameraType.Positionable
			) {
				Reset();
				return true;
			}

			if(loadedScript == null) {
				var possibleScripts = settings.MovementScript.scriptList.Where(MovementScriptManager.movementScripts.ContainsKey);

				if(possibleScripts.Count() == 0)
					return true;

				var scriptToUse = possibleScripts.ElementAt(randomSource.Next(possibleScripts.Count()));

				loadedScript = MovementScriptManager.movementScripts[scriptToUse];

				if(loadedScript == null)
					return true;

				lastFov = cam.UCamera.fieldOfView;
				CamConfigReloaded();

				Plugin.Log.Info($"Applying Movementscript {scriptToUse} for camera {cam.name}");
				DoParent();
			}

			if(loadedScript.syncToSong && SceneUtil.isInSong) {
				currentAnimationTime = SceneUtil.audioTimeSyncController.songTime;
			} else {
				currentAnimationTime += cam.timeSinceLastRender;
			}

			if(currentAnimationTime > loadedScript.scriptDuration) {
				if(!loadedScript.loop)
					return true;

				currentAnimationTime %= loadedScript.scriptDuration;
				frameIndex = 0;
			}

			for(;;) {
				if(targetFrame.startTime > currentAnimationTime) {
					break;
				} else if(targetFrame.endTime <= currentAnimationTime) {
					lastPos = scriptTransform.localPosition = targetFrame.position;
					lastRot = scriptTransform.localRotation = targetFrame.rotation;
					if(targetFrame.FOV > 0)
						lastFov = cam.UCamera.fieldOfView = targetFrame.FOV;
				} else if(targetFrame.startTime <= currentAnimationTime) {
					var frameProgress = (currentAnimationTime - targetFrame.startTime) / targetFrame.duration;

					// I wish this was possible in a more DRY code fashion
					if(targetFrame.transition == MoveType.Linear) {
						scriptTransform.localPosition = Vector3.Lerp(lastPos, targetFrame.position, frameProgress);
						scriptTransform.localRotation = Quaternion.Lerp(lastRot, targetFrame.rotation, frameProgress);

						if(targetFrame.FOV > 0f)
							cam.UCamera.fieldOfView = Mathf.Lerp(lastFov, targetFrame.FOV, frameProgress);
					} else {
						scriptTransform.localPosition = Vector3.Slerp(lastPos, targetFrame.position, frameProgress);
						scriptTransform.localRotation = Quaternion.Slerp(lastRot, targetFrame.rotation, frameProgress);

						if(targetFrame.FOV > 0f)
							cam.UCamera.fieldOfView = Mathf.SmoothStep(lastFov, targetFrame.FOV, frameProgress);
					}
					break;
				}

				if(++frameIndex >= loadedScript.frames.Count) {
					frameIndex = 0;
					break;
				}
			}

			return true;
		}
	}
}
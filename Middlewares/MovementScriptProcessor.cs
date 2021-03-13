using System.Linq;
using UnityEngine;
using Camera2.Interfaces;
using Camera2.Utils;
using static Camera2.Configuration.MovementScript;
using Camera2.Configuration;
using Camera2.Managers;
using Camera2.Behaviours;
using System;

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

		Transformer scriptTransformer = null;

		MovementScript loadedScript = null;
		float currentAnimationTime = 0f;


		int frameIndex = 0;

		float lastFov = 0f;
		Vector3 lastPos = Vector3.zero;
		Quaternion lastRot = Quaternion.identity;

		Frame targetFrame => loadedScript.frames[frameIndex];

		private void DoParent() {
			if(scriptTransformer != null)
				return;

			scriptTransformer = cam.GetOrCreateTransformer("MovementScript", TransformerOrders.MovementScriptProcessor);
		}

		private void Reset() {
			if(loadedScript == null)
				return;

			if(scriptTransformer != null)
				GameObject.Destroy(scriptTransformer);

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

			var x = cam.GetTransformer("Position");

			if(x == null)
				return;

			// Having a custom position on a camera thats executing a movement script is PROBABLY not what the user wants
			x.transform.localPosition = Vector3.zero;
			x.transform.localRotation = Quaternion.identity;
		}

		public void OnDisable() => Reset();

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
				if(targetFrame.startTime > currentAnimationTime)
					break;
				
				if(targetFrame.endTime <= currentAnimationTime) {
					lastPos = scriptTransformer.transform.localPosition = targetFrame.position;
					lastRot = scriptTransformer.transform.localRotation = targetFrame.rotation;
					if(targetFrame.FOV > 0)
						lastFov = cam.UCamera.fieldOfView = targetFrame.FOV;
				} else if(targetFrame.startTime <= currentAnimationTime) {
					var frameProgress = (currentAnimationTime - targetFrame.startTime) / targetFrame.duration;

					if(targetFrame.transition == MoveType.Eased)
						frameProgress = Easings.EaseInOutCubic01(frameProgress);

					scriptTransformer.transform.localPosition = Vector3.LerpUnclamped(lastPos, targetFrame.position, frameProgress);
					scriptTransformer.transform.localRotation = Quaternion.LerpUnclamped(lastRot, targetFrame.rotation, frameProgress);

					if(targetFrame.FOV > 0f)
						cam.UCamera.fieldOfView = Mathf.LerpUnclamped(lastFov, targetFrame.FOV, frameProgress);

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
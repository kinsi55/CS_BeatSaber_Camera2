using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2.Configuration {
	class Settings_Smoothfollow : CameraSubSettings {
		public float position = 10f;
		public float rotation = 4f;

		public bool forceUpright = false;
		public bool followReplayPosition = true;

		private bool _pivotingOffset = true;
		[JsonProperty("pivotingOffset")]
		public bool pivotingOffset {
			get { return _pivotingOffset; }
			set {
				if(value == _pivotingOffset)
					return;

				_pivotingOffset = value;

				settings.cam.transformer.applyAsAbsolute = !value;
			}
		}

		[JsonIgnore] internal bool useLocalPosition = true;
		[JsonIgnore] internal Transform parent;


		[JsonIgnore] internal Transformer transformer;

		public string targetParent = "";

		public bool ShouldSerializetargetParent() => settings.type == CameraType.Attached;
	}
}

namespace Camera2.Middlewares {
	class Smoothfollow : CamMiddleware, IMHandler {
		Scene lastScene;

		Transform parent { get { return settings.Smoothfollow.parent; } set { settings.Smoothfollow.parent = value; } }

		bool teleportOnNextFrame = false;

		public void OnEnable() {
			/*
			 * If the camera was just enabled we want to teleport the positon / rotation.
			 * This is useful when you switch to a scene with a firstperson camera that was
			 * not enabled for a while to make it have a "correct" initial position instead
			 * of smoothing it to the correct position over time
			 */
			teleportOnNextFrame = true;
		}

		new public bool Pre() {
			if(settings.type == Configuration.CameraType.Positionable) {
				if(settings.Smoothfollow.transformer != null) {
					settings.Smoothfollow.transformer.position = Vector3.zero;
					settings.Smoothfollow.transformer.rotation = Quaternion.identity;
				}

				return true;
			}

			Transform parentToUse = null;
			bool isAttachedToReplayCam = false;
			if(
				ScoresaberUtil.isInReplay &&
				//UnityEngine.XR.XRDevice.isPresent && 
				settings.type == Configuration.CameraType.FirstPerson
			) {
				if(settings.Smoothfollow.followReplayPosition) {
					parentToUse = ScoresaberUtil.replayCamera?.transform;
					settings.Smoothfollow.useLocalPosition = true;
					isAttachedToReplayCam = true;
				} else {
					// This is complete garbage
					if(ScoresaberUtil.spectateParent != null)
						HookRoomAdjust.ApplyCustom(ScoresaberUtil.spectateParent.position, ScoresaberUtil.spectateParent.rotation);
				}

				if(parent == ScoresaberUtil.replayCamera?.transform)
					parent = null;
			}

			if(parentToUse == null && settings.type == Configuration.CameraType.FirstPerson && HookFPFC.isInFPFC && HookFPFC.cameraInstance != null) {
				parentToUse = HookFPFC.cameraInstance?.transform;
				settings.Smoothfollow.useLocalPosition = false;
			}

			if(parentToUse == null)
				parentToUse = parent;

			if(parentToUse == null || parentToUse.gameObject?.activeInHierarchy != true) {
				if(settings.type == Configuration.CameraType.FirstPerson) {
					parent = parentToUse = Camera.main?.transform;
					settings.Smoothfollow.useLocalPosition = true;
				} else if(settings.type == Configuration.CameraType.Attached) {
					parent = parentToUse = GameObject.Find(settings.Smoothfollow.targetParent)?.transform;
					settings.Smoothfollow.useLocalPosition = false;
				}
			}

			//System.Console.WriteLine("FP cam is attached to {0}", parentToUse);

			// If we dont have a parent we should not render.
			if(parentToUse == null)
				return false;

			var targetPosition = parentToUse.position;
			var targetRotation = parentToUse.rotation;

			if(settings.Smoothfollow.useLocalPosition) {
				targetPosition = parentToUse.localPosition;
				targetRotation = parentToUse.localRotation;

				if(HookRoomAdjust.position != Vector3.zero || HookRoomAdjust.rotation != Quaternion.identity) {
					// Not exactly sure why we gotta exclude replays from this, but thats what it is
					if(settings.type == Configuration.CameraType.FirstPerson && !isAttachedToReplayCam) {
						targetPosition = (HookRoomAdjust.rotation * targetPosition) + HookRoomAdjust.position;
						targetRotation = HookRoomAdjust.rotation * targetRotation;
					}
				}
			}

			if(settings.Smoothfollow.forceUpright)
				targetRotation *= Quaternion.Euler(0, 0, -parentToUse.transform.localEulerAngles.z);

			if(!teleportOnNextFrame) {
				teleportOnNextFrame =
					lastScene != SceneUtil.currentScene ||
					(HookFPFC.isInFPFC && (!settings.Smoothfollow.followReplayPosition || !ScoresaberUtil.isInReplay));
			}

			if(settings.Smoothfollow.transformer == null) {
				settings.Smoothfollow.transformer = cam.transformchain.AddOrGet("SmoothFollow", TransformerOrders.SmoothFollow);

				teleportOnNextFrame = true;
			}

			var theTransform = settings.Smoothfollow.transformer;

			// If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
			if(teleportOnNextFrame) {
				theTransform.position = targetPosition;
				theTransform.rotation = targetRotation;

				lastScene = SceneUtil.currentScene;
				teleportOnNextFrame = false;
			} else {
				theTransform.position = Vector3.Lerp(theTransform.position, targetPosition, cam.timeSinceLastRender * settings.Smoothfollow.position);
				theTransform.rotation = Quaternion.Slerp(theTransform.rotation, targetRotation, cam.timeSinceLastRender * settings.Smoothfollow.rotation);
			}
			return true;
		}
	}
}

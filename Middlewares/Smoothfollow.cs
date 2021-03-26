using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Camera2.Interfaces;
using Camera2.Utils;
using Camera2.HarmonyPatches;
using Camera2.Behaviours;

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
			set {
				if(value == _pivotingOffset)
					return;

				_pivotingOffset = value;

				settings.cam.transformer.applyAsAbsolute = !value;
			}
		}

		[JsonIgnore] internal bool isAttachedToFP = true;
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

		new public bool Pre() {
			if(settings.type == Configuration.CameraType.Positionable) {
				if(settings.Smoothfollow.transformer != null) {
					settings.Smoothfollow.transformer.position = Vector3.zero;
					settings.Smoothfollow.transformer.rotation = Quaternion.identity;
				}

				return true;
			}

			var parentToUse = parent;

			if(ScoresaberUtil.isInReplay && settings.Smoothfollow.followReplayPosition && settings.type == Configuration.CameraType.Attached) {
				parentToUse = ScoresaberUtil.replayCamera?.transform;
				settings.Smoothfollow.isAttachedToFP = false;
			}
			
			if(HookFPFC.isInFPFC && settings.type == Configuration.CameraType.FirstPerson && HookFPFC.cameraInstance != null) {
				parentToUse = HookFPFC.cameraInstance?.transform;
				settings.Smoothfollow.isAttachedToFP = false;
			} else if(parentToUse == null || parentToUse.gameObject?.activeInHierarchy != true) {
				if(settings.type == Configuration.CameraType.FirstPerson) {
					parent = parentToUse = Camera.main?.transform;
					settings.Smoothfollow.isAttachedToFP = true;
				} else if(settings.type == Configuration.CameraType.Attached) {
					parent = parentToUse = GameObject.Find(settings.Smoothfollow.targetParent)?.transform;
					settings.Smoothfollow.isAttachedToFP = false;
				}
			}

			//System.Console.WriteLine("FP cam is attached to {0}", parentToUse);
			
			// If we dont have a parent we should not render.
			if(parentToUse == null)
				return false;

			var targetPosition = parentToUse.position;
			var targetRotation = parentToUse.rotation;

			// This is stupid
			if(settings.Smoothfollow.isAttachedToFP && !(HookMultiplayerFail.hasFailed || HookMultiplayer.instance?.isSpectating == true)) {
				targetPosition = parentToUse.localPosition;
				targetRotation = parentToUse.localRotation;

				if(HookRoomAdjust.position != Vector3.zero || HookRoomAdjust.rotation != Quaternion.identity) {
					/*
						* This is complete garbage and I need to fix this issue better some day because this will almost certainly cause issues down the line.
						* The issue is that the room offset is essentially already "Pre-applied" in FP cams (Because the player has to move to "correct" for his offset), but
						* in third person cams we need to un-apply it when being parented to the song origin because if we dont keep the cams world positon on parent it would
						* change the 0;0;0 point of the cam and thus move to a place its not supposed to be in as "room offset" offsets the player, not the room.
						*/
					bool doApply =
						settings.type == Configuration.CameraType.FirstPerson &&
						(!HookLeveldata.isModdedMap || !settings.ModmapExtensions.moveWithMap || !SceneUtil.isInSong);

					if(doApply) {
						targetPosition = (HookRoomAdjust.rotation * targetPosition) + HookRoomAdjust.position;
						targetRotation *= HookRoomAdjust.rotation;
					}
				}
			}

			if(settings.Smoothfollow.forceUpright)
				targetRotation *= Quaternion.Euler(0, 0, -parentToUse.transform.localEulerAngles.z);

			bool teleport =
				lastScene != SceneUtil.currentScene ||
				(HookFPFC.isInFPFC && (!settings.Smoothfollow.followReplayPosition || !ScoresaberUtil.isInReplay));

			if(settings.Smoothfollow.transformer == null) {
				settings.Smoothfollow.transformer = cam.transformchain.AddOrGet("SmoothFollow", TransformerOrders.SmoothFollow);

				teleport = true;
			}

			var theTransform = settings.Smoothfollow.transformer;

			// If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
			if(teleport) {
				theTransform.position = targetPosition;
				theTransform.rotation = targetRotation;

				lastScene = SceneUtil.currentScene;
			} else {
				theTransform.position = Vector3.Lerp(theTransform.position, targetPosition, cam.timeSinceLastRender * settings.Smoothfollow.position);
				theTransform.rotation = Quaternion.Slerp(theTransform.rotation, targetRotation, cam.timeSinceLastRender * settings.Smoothfollow.rotation);
			}
			return true;
		}
	}
}

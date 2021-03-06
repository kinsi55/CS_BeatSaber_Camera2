﻿using UnityEngine;
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

				// Destroying it here will cause it to get re-created in the correct hierarchy-position next frame
				if(transformer != null)
					GameObject.Destroy(transformer);
			}
		}

		[JsonIgnore]
		internal Transform parent;

		[JsonIgnore]
		internal Transformer transformer;

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
				if(settings.Smoothfollow.transformer != null)
					Destroy(settings.Smoothfollow.transformer);

				return true;
			}

			var parentToUse = parent;
			var useLocalPosition = true;

			if(ScoresaberUtil.isInReplay && settings.Smoothfollow.followReplayPosition) {
				parentToUse = ScoresaberUtil.replayCamera?.transform;
			} if(HookFPFC.isInFPFC && settings.type == Configuration.CameraType.FirstPerson && HookFPFC.cameraInstance != null) {
				parentToUse = HookFPFC.cameraInstance?.transform;
				useLocalPosition = false;
			} else if(parentToUse == null || parentToUse.gameObject?.activeInHierarchy != true) {
				if(settings.type == Configuration.CameraType.FirstPerson) {
					parent = parentToUse = Camera.main?.transform;
				} else if(settings.type == Configuration.CameraType.Attached) {
					parent = parentToUse = GameObject.Find(settings.Smoothfollow.targetParent)?.transform;
				}
			}

			//System.Console.WriteLine("FP cam is attached to {0}", parentToUse);
			
			// If we dont have a parent we should not render.
			if(parentToUse == null)
				return false;

			var targetRotation = useLocalPosition ? parentToUse.localRotation : parentToUse.rotation;
			var targetPosition = useLocalPosition ? parentToUse.localPosition : parentToUse.position;

			if(settings.Smoothfollow.forceUpright)
				targetRotation *= Quaternion.Euler(0, 0, -parentToUse.transform.localEulerAngles.z);

			bool teleport =
				lastScene != SceneUtil.currentScene ||
				(HookFPFC.isInFPFC && (!settings.Smoothfollow.followReplayPosition || !ScoresaberUtil.isInReplay));

			if(settings.Smoothfollow.transformer == null) {
				settings.Smoothfollow.transformer = cam.GetOrCreateTransformer(
					"SmoothFollow",
						settings.Smoothfollow.pivotingOffset ? TransformerOrders.SmoothFollowPivoting : TransformerOrders.SmoothFollowAbsolute
				);

				teleport = true;
			}

			var theTransform = settings.Smoothfollow.transformer.transform;

			// If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
			if(teleport) {
				theTransform.SetLocalPositionAndRotation(targetPosition, targetRotation);

				lastScene = SceneUtil.currentScene;
			} else {
				theTransform.localPosition = Vector3.Lerp(theTransform.localPosition, targetPosition, cam.timeSinceLastRender * settings.Smoothfollow.position);
				theTransform.localRotation = Quaternion.Slerp(theTransform.localRotation, targetRotation, cam.timeSinceLastRender * settings.Smoothfollow.rotation);
			}
			return true;
		}
	}
}

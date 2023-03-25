using System;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.SDK;
using Camera2.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2.Configuration {
	class Settings_Smoothfollow : CameraSubSettings {
		public float position = 10f;
		public float rotation = 4f;

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

		public CameraBoundsConfig limits = new CameraBoundsConfig();

		// TODO: Yeet this at some point
		bool forceUpright { get => false; set => limits.rot_z_min = limits.rot_z_min = 0; }
		public bool ShouldSerializeforceUpright() => false;

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

		float ClampAngle(float angle, float from, float to) {
			// accepts e.g. -80, 80
			if(angle < 0f)
				angle = 360 + angle;
			if(angle > 180f)
				return Math.Max(angle, 360 + from);
			return Math.Min(angle, to);
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
			ReplaySources.ISource currentReplaySource = null;

			if(settings.type == Configuration.CameraType.FirstPerson && settings.Smoothfollow.followReplayPosition) {
				foreach(var source in ReplaySources.sources) {
					if(!source.isInReplay)
						continue;

					currentReplaySource = source;
					break;
				}
			}

			if(settings.type == Configuration.CameraType.FirstPerson && HookFPFCToggle.isInFPFC) {
				parentToUse = HookFPFCToggle.fpfcTransform;
				currentReplaySource = null;
				settings.Smoothfollow.useLocalPosition = HookFPFCToggle.isSiraSettingLocalPostionYes;
			}

			Vector3 targetPosition = Vector3.zero;
			Quaternion targetRotation = Quaternion.identity;

			if(currentReplaySource == null) {
				if(parentToUse == null)
					parentToUse = parent;

				if(parentToUse == null || !parentToUse.gameObject.activeInHierarchy) {
					if(settings.type == Configuration.CameraType.FirstPerson) {
						var a = Camera.main;

						parent = parentToUse = a == null ? null : a.transform;
						// Dont even try to understand why this is necessary
						settings.Smoothfollow.useLocalPosition = !ScoresaberUtil.isInReplay;
					} else if(settings.type == Configuration.CameraType.Attached) {
						parent = parentToUse = GameObject.Find(settings.Smoothfollow.targetParent)?.transform;
						settings.Smoothfollow.useLocalPosition = false;
					}
				}

				//System.Console.WriteLine("FP cam is attached to {0}", parentToUse);

				// If we dont have a parent we should not render.
				if(parentToUse == null)
					return false;

				if(settings.Smoothfollow.useLocalPosition) {
					targetPosition = parentToUse.localPosition;
					targetRotation = parentToUse.localRotation;

					if(settings.type == Configuration.CameraType.FirstPerson && (HookRoomAdjust.position != Vector3.zero || HookRoomAdjust.rotation != Quaternion.identity)) {
						if(!HookFPFCToggle.isInFPFC) {
							targetPosition = (HookRoomAdjust.rotation * targetPosition) + HookRoomAdjust.position;
							targetRotation = HookRoomAdjust.rotation * targetRotation;
						} else {
							//var parentsParent = parentToUse.parent;
							//var parentsParentLocalRotation = parentsParent.rotation;
							//targetPosition += parentsParent.localPosition;
							//targetRotation *= Quaternion.Inverse(parentsParentLocalRotation);
						}
					}
				} else {
					targetPosition = parentToUse.position;
					targetRotation = parentToUse.rotation;
				}
			} else {
				targetPosition = currentReplaySource.localHeadPosition;
				targetRotation = currentReplaySource.localHeadRotation;
			}

			if(!HookFPFCToggle.isInFPFC) {
				// TODO: This is kinda shit
				var l = settings.Smoothfollow.limits;

				if(!float.IsNegativeInfinity(l.pos_x_min) || !float.IsPositiveInfinity(l.pos_x_max))
					targetPosition.x = Mathf.Clamp(targetPosition.x, l.pos_x_min, l.pos_x_max);

				if(!float.IsNegativeInfinity(l.pos_y_min) || !float.IsPositiveInfinity(l.pos_y_max))
					targetPosition.y = Mathf.Clamp(targetPosition.y, l.pos_y_min, l.pos_y_max);

				if(!float.IsNegativeInfinity(l.pos_z_min) || !float.IsPositiveInfinity(l.pos_z_max))
					targetPosition.z = Mathf.Clamp(targetPosition.z, l.pos_z_min, l.pos_z_max);

				var E = targetRotation.eulerAngles;

				if(!float.IsNegativeInfinity(l.rot_x_min) || !float.IsPositiveInfinity(l.rot_x_max))
					E.x = ClampAngle(E.x, l.rot_x_min, l.rot_x_max);

				if(!float.IsNegativeInfinity(l.rot_y_min) || !float.IsPositiveInfinity(l.rot_y_max))
					E.y = ClampAngle(E.y, l.rot_y_min, l.rot_y_max);

				if(!float.IsNegativeInfinity(l.rot_z_min) || !float.IsPositiveInfinity(l.rot_z_max))
					E.z = ClampAngle(E.z, l.rot_z_min, l.rot_z_max);

				targetRotation.eulerAngles = E;
			}

			if(!teleportOnNextFrame) {
				teleportOnNextFrame =
					lastScene != SceneUtil.currentScene ||
					(HookFPFCToggle.isInFPFC && currentReplaySource == null);
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

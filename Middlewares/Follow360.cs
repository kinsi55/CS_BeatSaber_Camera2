using System;
using UnityEngine;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using Camera2.Behaviours;

namespace Camera2.Configuration {
	class Settings_Follow360 : CameraSubSettings {
		public bool enabled = true;
		public float smoothing = 10f;
	}
}

namespace Camera2.Middlewares {
	class Follow360 : CamMiddleware, IMHandler {
		public void OnDisable() => Reset();

		Transformer rotationApplier = null;
		float currentRotateAmount = 0f;

		private void Reset() {
			if(rotationApplier != null) {
				currentRotateAmount = 0f;
				rotationApplier.rotation = Quaternion.identity;
				rotationApplier.position = Vector3.zero;
			}
		}

		new public bool Pre() {
			if(
				!enabled ||
				!settings.Follow360.enabled ||
				SceneUtil.isInMenu ||
				!HookLeveldata.is360Level ||
				HookLevelRotation.Instance == null ||
				(settings.type != Configuration.CameraType.Positionable && settings.Smoothfollow.pivotingOffset)
			) {
				Reset();

				return true;
			}

			if(rotationApplier == null) {
				rotationApplier = cam.transformchain.AddOrGet("Follow360", TransformerOrders.Follow360);
				rotationApplier.applyAsAbsolute = true;
			}

			if(HookLevelRotation.Instance.targetRotation != 0f) {
				if(currentRotateAmount == HookLevelRotation.Instance.targetRotation)
					return true;

				var rotateStep = HookLevelRotation.Instance.targetRotation;

				// Make sure we dont spam unnecessary calculations / rotation steps for the last little bit
				if(Math.Abs(currentRotateAmount - HookLevelRotation.Instance.targetRotation) > 0.3f)
					rotateStep = Mathf.LerpAngle(currentRotateAmount, HookLevelRotation.Instance.targetRotation, cam.timeSinceLastRender * settings.Follow360.smoothing);

				var rot = Quaternion.Euler(0, rotateStep, 0);

				rotationApplier.position = rot * (cam.transformer.position - HookRoomAdjust.position) + HookRoomAdjust.position;

				rotationApplier.position -= cam.transformer.position;

				if(settings.type == Configuration.CameraType.Positionable) {
					rotationApplier.rotation = rot;
				} else {
					rotationApplier.rotation = Quaternion.identity;
				}

				currentRotateAmount = rotateStep;
			}

			return true;
		}
	}
}
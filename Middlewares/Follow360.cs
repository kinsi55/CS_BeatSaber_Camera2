using System;
using UnityEngine;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;

namespace Camera2.Configuration {
	class Settings_Follow360 : CameraSubSettings {
		public bool enabled = true;
		public float smoothing = 10f;
	}
}

namespace Camera2.Middlewares {
	class Follow360 : CamMiddleware, IMHandler {
		float currentRotateAmount = 0f;
		public void OnDisable() {
			Reset();
		}

		private void Reset() {
			if(currentRotateAmount != 0f) {
				cam.settings.ApplyPositionAndRotation();

				currentRotateAmount = 0f;
			}
		}

		new public bool Pre() {
			if(
				!enabled || 
				!settings.Follow360.enabled || 
				SceneUtil.isInMenu || 
				HookLevelRotation.Instance == null ||
				settings.type != Configuration.CameraType.Positionable
				// CBA to deal with 360 support for absolute offsets for now
				//(settings.type != Configuration.CameraType.Positionable && settings.Smoothfollow.pivotingOffset)
			) {
				Reset();

				return true;
			}

			if(HookLevelRotation.Instance.targetRotation != 0f) {
				// Make sure we dont spam unnecessary calculations / rotation steps for the last little bit
				if(Math.Abs(currentRotateAmount - HookLevelRotation.Instance.targetRotation) < 1f)
					return true;

				var rotateStep = Mathf.LerpAngle(currentRotateAmount, HookLevelRotation.Instance.targetRotation, cam.timeSinceLastRender * settings.Follow360.smoothing);

				cam.transform.RotateAround(
					SceneUtil.songWorldTransform != null ? SceneUtil.songWorldTransform.position : Vector3.zero,
					Vector3.up, (rotateStep - currentRotateAmount)
				);

				currentRotateAmount = rotateStep;
			}

			return true;
		}
	}
}
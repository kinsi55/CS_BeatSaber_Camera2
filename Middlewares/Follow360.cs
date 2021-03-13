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
				GameObject.Destroy(rotationApplier);
				rotationApplier = null;
				currentRotateAmount = 0f;
			}
		}

		new public bool Pre() {
			if(
				!enabled ||
				!settings.Follow360.enabled ||
				SceneUtil.isInMenu ||
				!HookLeveldata.is360Level ||
				HookLevelRotation.Instance == null ||
				settings.type != Configuration.CameraType.Positionable
				// CBA to deal with 360 support for absolute offsets for now
				//(settings.type != Configuration.CameraType.Positionable && settings.Smoothfollow.pivotingOffset)
			) {
				Reset();

				return true;
			}

			if(rotationApplier == null)
				rotationApplier = cam.GetOrCreateTransformer("Follow360", TransformerOrders.Follow360);

			if(HookLevelRotation.Instance.targetRotation != 0f) {
				// Make sure we dont spam unnecessary calculations / rotation steps for the last little bit
				if(Math.Abs(currentRotateAmount - HookLevelRotation.Instance.targetRotation) < 1f)
					return true;

				var rotateStep = Mathf.LerpAngle(currentRotateAmount, HookLevelRotation.Instance.targetRotation, cam.timeSinceLastRender * settings.Follow360.smoothing);

				rotationApplier.transform.RotateAround(
					SceneUtil.songWorldTransform != null ? SceneUtil.songWorldTransform.position : Vector3.zero,
					Vector3.up, (rotateStep - currentRotateAmount)
				);

				/*
				 * Firstperson cameras w/ non-pivotingOffset get rotated by this, but since the HMD rotation is
				 * applied to the camera we dont want to rotate the camera with the map rotation
				 */
				if(settings.type != Configuration.CameraType.Positionable)
					rotationApplier.transform.rotation = Quaternion.identity;

				currentRotateAmount = rotateStep;
			}

			return true;
		}
	}
}
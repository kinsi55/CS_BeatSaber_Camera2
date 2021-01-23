using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Middlewares {
	class Follow360 : CamMiddleware, IMHandler {
		float currentRotateAmount = 0f;
		new public bool Pre() {
			if(!this.enabled || SceneUtil.isInMenu || HookLevelRotation.Instance == null || settings.type != Configuration.CameraType.Positionable) {
				currentRotateAmount = 0f;
				return true;
			}
				
			if(HookLevelRotation.Instance.currentRotation != 0f) {
				if(currentRotateAmount == HookLevelRotation.Instance.currentRotation)
					return true;

				cam.transform.RotateAround(
					SceneUtil.songWorldTransform != null ? SceneUtil.songWorldTransform.position : Vector3.zero,
					Vector3.up, -(currentRotateAmount - HookLevelRotation.Instance.currentRotation)
				);

				currentRotateAmount = HookLevelRotation.Instance.currentRotation;
			}

			return true;
		}
	}
}

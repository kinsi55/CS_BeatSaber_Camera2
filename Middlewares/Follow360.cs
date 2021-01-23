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
		/* 
		 * Applying 360 / 90 rotations is kind of a hack but then again, not really. Eventho there is no 
		 * Noodle extension 360 maps I wanted to make it so *in theory* its compatible, so I manually add 
		 * the 360 rotation on top of the possible noodle translation and remove it again after rendering, 
		 * every frame. I dont think this is that expensive, especially since we only translate one angle
		 */
		float currentRotateAmount = 0f;
		new public bool Pre() {
			if(!this.enabled || SceneUtil.isInMenu || HookLevelRotation.Instance == null || settings.type != Configuration.CameraType.Positionable)
				return true;
				
			if(HookLevelRotation.Instance.currentRotation != 0f) {
				currentRotateAmount = HookLevelRotation.Instance.currentRotation;

				cam.transform.RotateAround(
					SceneUtil.songWorldTransform != null ? SceneUtil.songWorldTransform.position : Vector3.zero, 
					Vector3.up, currentRotateAmount
				);
			}

			return true;
		}

		new public void Post() {
			if(currentRotateAmount != 0f) {
				cam.transform.RotateAround(
					SceneUtil.songWorldTransform != null ? SceneUtil.songWorldTransform.position : Vector3.zero,
					Vector3.down, currentRotateAmount
				);
				currentRotateAmount = 0f;
			}
		}
	}
}

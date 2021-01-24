using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace Camera2.Behaviours {
	//UnityEngine.EventSystems.EventSystem.current.currentInputModule.currentPointerData


	class CamPositioner : MonoBehaviour {
		//private static VRPointer pointer;
		private static VRController controller;
		private static Cam2 grabbedCamera = null;

		private static Vector3 grabStartPos;
		private static Quaternion grabStartRot;

		// These should in theory always be zero and unnecessary but whatever.
		private static Vector3 oldLocalPosition;
		private static Quaternion oldLocalRotation;


		public static void BeingDragCamera(Cam2 camera, PointerEventData eventData) {
			if(eventData.currentInputModule is VRInputModule) {
				if(grabbedCamera != null)
					FinishCameraMove();

				controller = Resources.FindObjectsOfTypeAll<VRLaserPointer>()
					.LastOrDefault(x => x.gameObject.activeInHierarchy)
					?.GetComponentInParent<VRController>();

				if(controller == null)
					return;

				grabbedCamera = camera;

				oldLocalPosition = grabbedCamera.UCamera.transform.localPosition;
				oldLocalRotation = grabbedCamera.UCamera.transform.localRotation;

				grabStartPos = controller.transform.InverseTransformPoint(grabbedCamera.UCamera.transform.position);
				grabStartRot = Quaternion.Inverse(controller.transform.rotation) * grabbedCamera.UCamera.transform.rotation;

				grabbedCamera.worldCam.SetPreviewPositionAndSize(false);
			}
		}
		public void Awake() {
			DontDestroyOnLoad(this);
		}

		public void Update() {
			if(grabbedCamera != null) {
				if(controller != null) {
					grabbedCamera.UCamera.transform.position = controller.transform.TransformPoint(grabStartPos);
					grabbedCamera.UCamera.transform.rotation = controller.transform.rotation * grabStartRot;

					if(controller.triggerValue > 0.5f)
						return;
				}

				FinishCameraMove();
			}
		}

		private static void FinishCameraMove() {
			if(grabbedCamera == null) return;

			// If the song translated the camera position we must account for that so it doesnt end up in the config
			var posDeviation = controller.transform.TransformPoint(grabbedCamera.transform.position) - controller.transform.TransformPoint(grabbedCamera.settings.targetPos);
			var rotDeviation = Quaternion.Inverse(grabbedCamera.transform.rotation) * Quaternion.Euler(grabbedCamera.settings.targetRot);


			grabbedCamera.settings.targetPos = grabbedCamera.UCamera.transform.position - posDeviation - oldLocalPosition;
			grabbedCamera.settings.targetRot = (grabbedCamera.UCamera.transform.rotation * Quaternion.Inverse(rotDeviation) * Quaternion.Inverse(oldLocalRotation)).eulerAngles;

			grabbedCamera.UCamera.transform.localPosition = oldLocalPosition;
			grabbedCamera.UCamera.transform.localRotation = oldLocalRotation;

			grabbedCamera.worldCam.SetPreviewPositionAndSize(true);
			grabbedCamera.settings.ApplyPositionAndRotation();

			grabbedCamera = null;
		}
	}
}

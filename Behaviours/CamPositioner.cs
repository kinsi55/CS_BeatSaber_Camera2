using System.Linq;
using UnityEngine;
using VRUIControls;
using Camera2.HarmonyPatches;

namespace Camera2.Behaviours {
	class CamPositioner : MonoBehaviour {
		//private static VRPointer pointer;
		private static VRController controller;
		private static Cam2 grabbedCamera = null;
		private static Transform camTransform;

		private static Vector3 grabStartPos;
		private static Quaternion grabStartRot;

		public void Awake() {
			DontDestroyOnLoad(gameObject);
		}

		public static void BeingDragCamera(Cam2 camera) {
			if(grabbedCamera != null)
				FinishCameraMove();

			controller = Resources.FindObjectsOfTypeAll<VRLaserPointer>()
				.LastOrDefault(x => x.gameObject.activeInHierarchy)
				?.GetComponentInParent<VRController>();

			if(controller == null)
				return;

			//TODO: I should probably move this to use a Transformer...
			grabbedCamera = camera;
			camTransform = camera.UCamera.transform;

			grabStartPos = controller.transform.InverseTransformPoint(camTransform.position);
			grabStartRot = Quaternion.Inverse(controller.rotation) * camTransform.rotation;

			grabbedCamera.worldCam.SetPreviewPositionAndSize(false);
		}

		public void Update() {
			if(grabbedCamera != null) {
				if(controller != null && grabbedCamera.worldCam.isActiveAndEnabled) {
					camTransform.position = controller.transform.TransformPoint(grabStartPos);
					camTransform.rotation = controller.rotation * grabStartRot;

					if(controller.triggerValue > 0.5f || (HookFPFC.isInFPFC && Input.GetMouseButton(0)))
						return;
				}

				FinishCameraMove();
			}
		}

		private static void FinishCameraMove() {
			if(grabbedCamera == null) return;

			grabbedCamera.settings.targetPos = camTransform.position;
			grabbedCamera.settings.targetRot =  camTransform.eulerAngles;

			camTransform.localPosition = Vector3.zero;
			camTransform.localRotation = Quaternion.identity;

			grabbedCamera.settings.ApplyPositionAndRotation();

			grabbedCamera.worldCam.SetPreviewPositionAndSize(true);

			grabbedCamera.settings.Save();

			grabbedCamera = null;
		}
	}
}

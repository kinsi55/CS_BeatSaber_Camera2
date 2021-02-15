using System.Linq;
using UnityEngine;
using VRUIControls;

namespace Camera2.Behaviours {
	class CamPositioner : MonoBehaviour {
		//private static VRPointer pointer;
		private static VRController controller;
		private static Cam2 grabbedCamera = null;

		private static Vector3 grabStartPos;
		private static Quaternion grabStartRot;

		public void Awake() {
			DontDestroyOnLoad(this);
		}

		public static void BeingDragCamera(Cam2 camera) {
			if(grabbedCamera != null)
				FinishCameraMove();

			controller = Resources.FindObjectsOfTypeAll<VRLaserPointer>()
				.LastOrDefault(x => x.gameObject.activeInHierarchy)
				?.GetComponentInParent<VRController>();

			if(controller == null)
				return;

			grabbedCamera = camera;

			grabbedCamera.transform.position = grabbedCamera.UCamera.transform.position;
			grabbedCamera.transform.rotation = grabbedCamera.UCamera.transform.rotation;

			grabStartPos = controller.transform.InverseTransformPoint(grabbedCamera.transform.position);
			grabStartRot = Quaternion.Inverse(controller.rotation) * grabbedCamera.transform.rotation;

			grabbedCamera.worldCam.SetPreviewPositionAndSize(false);
		}

		public void Update() {
			if(grabbedCamera != null) {
				if(controller != null && grabbedCamera.worldCam.isActiveAndEnabled) {
					grabbedCamera.transform.position = controller.transform.TransformPoint(grabStartPos);
					grabbedCamera.transform.rotation = controller.rotation * grabStartRot;

					if(controller.triggerValue > 0.5f)
						return;
				}

				FinishCameraMove();
			}
		}

		private static void FinishCameraMove() {
			if(grabbedCamera == null) return;

			grabbedCamera.settings.targetPos = grabbedCamera.transform.position;
			grabbedCamera.settings.targetRot = grabbedCamera.transform.rotation.eulerAngles;

			grabbedCamera.worldCam.SetPreviewPositionAndSize(true);

			grabbedCamera.settings.Save();

			grabbedCamera = null;
		}
	}
}

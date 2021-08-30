using Camera2.HarmonyPatches;
using System.Linq;
using UnityEngine;
using VRUIControls;

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

			var controllers = Resources.FindObjectsOfTypeAll<VRLaserPointer>();

			controller = (!HookFPFC.isInFPFC ? 
				controllers.LastOrDefault(x => x.isActiveAndEnabled) : 
				controllers.LastOrDefault(x => x.transform.eulerAngles + x.transform.position != Vector3.zero)
			)?.GetComponentInParent<VRController>();

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
					var p = controller.transform.TransformPoint(grabStartPos);
					var r = (controller.rotation * grabStartRot).eulerAngles;

					//grabbedCamera.transformchain.BacktrackTo(grabbedCamera.transformer, ref p, ref r);

					grabbedCamera.transformer.position = p;
					void snap(ref float a, float snap = 4f, float step = 45f) {
						var l = a % step;

						if(l <= snap)
							a -= l;
						else if(l >= step - snap)
							a += step - l;
					}
					//grabbedCamera.transformer.rotation = r;
					snap(ref r.x);
					snap(ref r.y);
					snap(ref r.z);
					grabbedCamera.transformer.rotation = Quaternion.Euler(r.x, r.y, r.z);

					grabbedCamera.transformchain.Calculate();

					if(controller.triggerValue > 0.5f || (HookFPFC.isInFPFC && Input.GetMouseButton(0)))
						return;
				}

				FinishCameraMove();
			}
		}

		private static void FinishCameraMove() {
			if(grabbedCamera == null) return;

			grabbedCamera.settings.targetPos = grabbedCamera.transformer.position;
			grabbedCamera.settings.targetRot = grabbedCamera.transformer.rotation.eulerAngles;

			grabbedCamera.settings.ApplyPositionAndRotation();

			grabbedCamera.worldCam.SetPreviewPositionAndSize(true);

			grabbedCamera.settings.Save();

			grabbedCamera = null;
		}
	}
}

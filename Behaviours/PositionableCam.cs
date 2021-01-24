using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Camera2.Behaviours {
	class PositionableCam : MonoBehaviour, IPointerClickHandler {
		public Cam2 cam { get; private set; }

		private GameObject camOrigin;
		private GameObject camPreview;
		private Material viewMaterial;

		public void Awake() {
			DontDestroyOnLoad(this);

			camOrigin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			camOrigin.transform.parent = transform;

			camOrigin.transform.localScale = new Vector3(0.1f, 0.1f, 0.15f);
			camOrigin.transform.localPosition = new Vector3(0, 0, -(camOrigin.transform.localScale.z / 2));
			camOrigin.transform.localEulerAngles = new Vector3(90f, 0, 0);

			//var x = new Material(Shader.Find("Hidden/Internal-DepthNormalsTexture"));
			//camOriginCube.GetComponent<MeshRenderer>().material = x;

			camPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
			Destroy(camPreview.GetComponent<Collider>());
			camPreview.transform.parent = transform;
			
			camPreview.transform.localEulerAngles = new Vector3(0, 180f, 0);

			viewMaterial = new Material(Shader.Find("Hidden/BlitCopyWithDepth"));
			camPreview.GetComponent<MeshRenderer>().material = viewMaterial;
		}

		public void SetSource(Cam2 cam) {
			this.cam = cam;

			viewMaterial.SetTexture("_MainTex", cam.renderTexture);
			SetPreviewPositionAndSize();
		}

		public void SetPreviewPositionAndSize(bool small = true) {
			var size = small ? 0.3f : 0.75f;

			camPreview.transform.localScale = new Vector3(size, size / cam.UCamera.aspect, 0);
			camPreview.transform.localPosition = new Vector3(0, camPreview.transform.localScale.y, camOrigin.transform.localPosition.z / 2);
		}

		public void OnPointerClick(PointerEventData eventData) {
			CamPositioner.BeingDragCamera(cam, eventData);
		}
	}
}

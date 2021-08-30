using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Camera2.Behaviours {
	class PositionableCam : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
		public Cam2 cam { get; private set; }

		private GameObject camOrigin;
		private GameObject camPreview;
		private Material viewMaterial;

		private static Material hoverMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "HandleHologram"));
		private static Material normalMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "MenuShockwave"));

		private MeshRenderer renderer;

		public void Awake() {
			DontDestroyOnLoad(gameObject);

			camOrigin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			camOrigin.transform.parent = transform;

			camOrigin.transform.localScale = new Vector3(0.1f, 0.10f, 0.25f);
			camOrigin.transform.localPosition = new Vector3(0, 0, -(camOrigin.transform.localScale.z / 2));
			camOrigin.transform.localEulerAngles = new Vector3(90f, 0, 0);

			renderer = camOrigin.GetComponent<MeshRenderer>();
			normalMaterial.color = new Color(255, 255, 175, 0.7f);
			renderer.material = normalMaterial;

			camPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
			Destroy(camPreview.GetComponent<Collider>());
			camPreview.transform.parent = transform;

			viewMaterial = new Material(Plugin.Shader_VolumetricBlit);
			camPreview.GetComponent<MeshRenderer>().material = viewMaterial;
		}

		public void SetSource(Cam2 cam) {
			this.cam = cam;

			viewMaterial.SetTexture("_MainTex", cam.renderTexture);
			SetPreviewPositionAndSize();
		}

		public void SetPreviewPositionAndSize(bool small = true) {
			var size = small ? cam.settings.previewScreenSize : Math.Min(cam.settings.previewScreenSize * 2f, 4);

			if(cam.UCamera.aspect > 1f) {
				camPreview.transform.localScale = new Vector3(size, size / cam.UCamera.aspect, 0);
			} else {
				camPreview.transform.localScale = new Vector3(size * cam.UCamera.aspect, size, 0);
			}

			camPreview.transform.localPosition = new Vector3(
				0, 
				cam.settings.worldCamUnderScreen ? 0.15f + (camPreview.transform.localScale.y / 2f) : 0, 
				camOrigin.transform.localPosition.z / 2
			);
		}

		public void OnPointerClick(PointerEventData eventData) {
			if(!(eventData.currentInputModule is VRUIControls.VRInputModule))
				return;

			CamPositioner.BeingDragCamera(cam);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			renderer.material = hoverMaterial;
		}

		public void OnPointerExit(PointerEventData eventData) {
			renderer.material = normalMaterial;
		}
	}
}

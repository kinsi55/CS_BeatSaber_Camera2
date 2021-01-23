using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Behaviours {
	class PositionableCam : MonoBehaviour {
		Cam2 cam;

		GameObject camOriginCube;
		GameObject camPreview;
		Material viewMaterial;

		public void Awake() {
			DontDestroyOnLoad(this);

			camOriginCube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			camOriginCube.transform.parent = transform;

			camOriginCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.15f);
			camOriginCube.transform.localPosition = new Vector3(0, 0, -(camOriginCube.transform.localScale.z / 2));
			camOriginCube.transform.localEulerAngles = new Vector3(90f, 0, 0);

			//var x = new Material(Shader.Find("Hidden/Internal-DepthNormalsTexture"));
			//camOriginCube.GetComponent<MeshRenderer>().material = x;

			camPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
			Destroy(camPreview.GetComponent<Collider>());
			camPreview.transform.parent = transform;
			
			camPreview.transform.localEulerAngles = new Vector3(0, 180f, 0);
			camPreview.layer = 1;

			viewMaterial = new Material(Shader.Find("Hidden/BlitCopyWithDepth"));
			camPreview.GetComponent<MeshRenderer>().material = viewMaterial;
		}

		public void SetSource(Cam2 cam) {
			this.cam = cam;

			viewMaterial.SetTexture("_MainTex", cam.renderTexture);
			SetPreviewPositionAndSize(true);
		}

		public void SetPreviewPositionAndSize(bool small = true) {
			var size = small ? 0.3f : 0.75f;

			camPreview.transform.localScale = new Vector3(size, size / cam.UCamera.aspect, 0);
			camPreview.transform.localPosition = new Vector3(0, camPreview.transform.localScale.y, camOriginCube.transform.localPosition.z / 2);
		}
	}
}

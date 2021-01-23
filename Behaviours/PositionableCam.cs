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

		public void Awake() {
			DontDestroyOnLoad(this);

			camOriginCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			camOriginCube.transform.parent = transform;

			camOriginCube.transform.localScale = new Vector3(0.15f, 0.15f, 0.22f);
			//camOriginCube.layer = 0;


			camPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
			DestroyImmediate(camPreview.GetComponent<Collider>());
			camPreview.transform.parent = camOriginCube.transform;

			//camPreview.transform.localPosition = new Vector3(-1f * ((16f/ - 1) / 2 + 1), 0, 0.22f);
			camPreview.transform.localEulerAngles = new Vector3(0, 180, 0);
			camPreview.transform.localScale = new Vector3(2, 1, 1);
			//camPreview.layer = 0;
		}

		public void Init(Cam2 cam) {
			this.cam = cam;

			camPreview.GetComponent<MeshRenderer>().material = cam.screenImage.material;
		}
	}
}

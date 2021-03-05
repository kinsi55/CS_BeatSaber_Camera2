using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Behaviours {
	// When setting the parent of a camera this "shield" is placed in between so that, when the parent
	// is destroyed, the camera is "saved" by unparenting before the destroy can cascade to it
	class DestroyShield : MonoBehaviour {
		Cam2 cam;

		public void Init(Cam2 cam, Transform parent) {
			this.cam = cam;

			transform.parent = parent;
		}

		void OnDestroy() => cam.SetOrigin(null);
	}
}

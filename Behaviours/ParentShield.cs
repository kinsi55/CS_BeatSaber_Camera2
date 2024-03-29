﻿using UnityEngine;

namespace Camera2.Behaviours {
	// When setting the parent of a camera this "shield" is placed in between so that, when the parent
	// is destroyed, the camera is "saved" by unparenting before the destroy can cascade to it
	class ParentShield : MonoBehaviour {
		Cam2 cam;
		bool unparentOnDisable;

		void Awake() => enabled = false;

		public void Init(Cam2 cam, Transform parent, bool worldPositionStays, bool unparentOnDisable = false) {
			this.cam = cam;
			this.unparentOnDisable = unparentOnDisable;

			transform.SetParent(parent, worldPositionStays);
		}

		public void OnDestroy() => cam.SetOrigin(null);
		public void OnDisable() { if(unparentOnDisable) cam.SetOrigin(null); }
	}
}

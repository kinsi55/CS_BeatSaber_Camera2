using UnityEngine;
using Camera2.Configuration;
using Camera2.Behaviours;

namespace Camera2.Interfaces {
	interface IMHandler {
		bool Pre();
		void Post();
	}

	abstract class CamMiddleware : MonoBehaviour {
		protected Cam2 cam;
		protected CameraSettings settings { get { return cam.settings; } }
		
		public IMHandler Init(Cam2 cam) {
			this.cam = cam;
			return (IMHandler)this;
		}
		// Prevents the cam from rendering this frame if returned false
		public bool Pre() { return true; }
		public void Post() { }
	}
}

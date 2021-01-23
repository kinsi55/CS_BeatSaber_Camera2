using Camera2.Behaviours;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Camera2.Middlewares;
using Camera2.Configuration;

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
		public bool Pre() { return true; }
		public void Post() { }
	}
}

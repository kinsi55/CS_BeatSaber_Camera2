using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Camera2.Behaviours;
using Newtonsoft.Json;
using Camera2.Utils;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Camera2.Configuration {
	enum CameraType {
		FirstPerson,
		Attached,
		Positionable
	}
	
	class CameraSettings {
		[JsonIgnore]
		private Cam2 cam;
		public CameraSettings(Cam2 cam) {
			this.cam = cam;
		}

		public void Load(bool loadConfig = true) {
			// Set default values incase they're removed from the JSON because of user stoopid
			FOV = 90;
			viewRect = new Rect(0, 0, Screen.width, Screen.height);

			if(loadConfig && System.IO.File.Exists(cam.configPath)) {
				JsonConvert.PopulateObject(System.IO.File.ReadAllText(cam.configPath, Encoding.UTF8), this, new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore
				});
			} else {
				layer = CamManager.cams.Count == 0 ? -1000 : CamManager.cams.Max(x => x.Value.settings.layer) - 1;

				Save();
			}

			ApplyPositionAndRotation();
		}

		public void ApplyPositionAndRotation() {
			if(type != CameraType.Positionable)
				return;

			cam.UCamera.transform.position = targetPos;
			cam.UCamera.transform.eulerAngles = targetRot;
		}

		public void Save() {
			System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public CameraType type = CameraType.Attached;
		public float FOV { get { return cam.UCamera.fieldOfView; } set { cam.UCamera.fieldOfView = value; } }
		public int layer { get { return (int)cam.UCamera.depth; } set { cam.UCamera.depth = value; CamManager.ApplyViewLayers(); } }


		private float _renderScale = 1;
		public float renderScale {
			get { return _renderScale; }
			set {
				if(_renderScale == value) return;
				_renderScale = Math.Min(value, 3);
				cam.SetRenderTexture();
			}
		}

		private Rect _viewRect = Rect.zero;
		[JsonConverter(typeof(RectConverter))]
		public Rect viewRect {
			get { return _viewRect; }
			set {
				var doUpdate = cam.renderTexture == null || _viewRect.width != value.width || _viewRect.height != value.height;
				_viewRect = value;

				if(doUpdate)
					cam.SetRenderTexture();

				cam.UCamera.aspect = value.width / value.height;
			}
		}
		
		public Settings_FPSLimiter FPSLimiter = new Settings_FPSLimiter();
		public Settings_Smoothfollow Smoothfollow = new Settings_Smoothfollow();
		public Settings_NoodleExtensions NoodleExtensions = new Settings_NoodleExtensions();


		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetPos = new Vector3(0, 1.5f);// { get { return cam.UCamera.transform.position; } set { cam.UCamera.transform.position = value; } }
		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetRot = new Vector3(5f, 0, 0);// { get { return cam.UCamera.transform.rotation; } set { cam.UCamera.transform.rotation = value; } }

		//public bool ShouldSerializetargetPos() { return this.type == CameraType.Positionable; }
		//public bool ShouldSerializetargetRot() { return this.type == CameraType.Positionable; }

	}
}
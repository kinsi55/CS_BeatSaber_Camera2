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
		Attached, //Unused for now, but mostly implemented - Parent to arbitrary things
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

			cam.transform.position = targetPos;
			cam.transform.eulerAngles = targetRot;
		}

		public void ApplyLayerBitmask() {

		}

		public void Save() {
			System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
		}

		private CameraType _type = CameraType.Attached;
		[JsonConverter(typeof(StringEnumConverter))]
		public CameraType type { get { return _type; } set { _type = value; cam.ActivateWorldCamIfNecessary(); } }

		private bool _showWorldCam = true;
		public bool showWorldCam { get { return _showWorldCam; } set { _showWorldCam = value; cam.ActivateWorldCamIfNecessary(); } }

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
		public Settings_ModmapExtensions ModmapExtensions = new Settings_ModmapExtensions();


		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetPos = new Vector3(0, 1.5f, -1.5f);
		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetRot = new Vector3(3f, 0, 0);
	}
}
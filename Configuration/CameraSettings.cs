using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Camera2.Behaviours;
using Camera2.Utils;

namespace Camera2.Configuration {
	enum CameraType {
		FirstPerson,
		Attached, //Unused for now, but mostly implemented - For parenting to arbitrary things
		Positionable
	}

	class GameObjects {
		private CameraSettings parentSetting;
		public GameObjects(CameraSettings parentSetting) {
			this.parentSetting = parentSetting;
		}

		private bool _Walls = true;
		private bool _Debris = true; //Maybe make Enum w/ Show / Hide / Linked like in Cam Plus
		private bool _UI = true;
		private bool _Avatar = true;

		public bool Walls { get { return _Walls; } set { _Walls = value; parentSetting.ApplyLayerBitmask(); } }
		public bool Debris { get { return _Debris; } set { _Debris = value; parentSetting.ApplyLayerBitmask(); } }
		public bool UI { get { return _UI; } set { _UI = value; parentSetting.ApplyLayerBitmask(); } }
		public bool Avatar { get { return _Avatar; } set { _Avatar = value; parentSetting.ApplyLayerBitmask(); } }
	}
	
	class CameraSettings {
		[JsonIgnore]
		private Cam2 cam;
		public CameraSettings(Cam2 cam) {
			this.cam = cam;

			visibleObjects = new GameObjects(this);
		}

		public void Load(bool loadConfig = true) {
			// Set default values incase they're removed from the JSON because of user stoopid
			FOV = 90;
			viewRect = new Rect(0, 0, Screen.width, Screen.height);

			if(loadConfig && System.IO.File.Exists(cam.configPath)) {
				JsonConvert.PopulateObject(System.IO.File.ReadAllText(cam.configPath, Encoding.UTF8), this, new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore,
					Error = (se, ev) => { ev.ErrorContext.Handled = true; }
				});
			} else {
				layer = CamManager.cams.Count == 0 ? -1000 : CamManager.cams.Max(x => x.Value.settings.layer) - 1;

				Save();
			}

			ApplyPositionAndRotation();
			ApplyLayerBitmask();
		}

		public void ApplyPositionAndRotation() {
			if(type != CameraType.Positionable)
				return;

			cam.transform.position = targetPos;
			cam.transform.eulerAngles = targetRot;
		}

		public void ApplyLayerBitmask() {
			var maskBuilder = cam.UCamera.cullingMask;

			foreach(int mask in Enum.GetValues(typeof(VisibilityMasks)))
				maskBuilder &= ~mask;

			if(visibleObjects.Walls || (ModmapExtensions.autoOpaqueWalls && SceneUtil.isProbablyInWallMap))
				maskBuilder |= (int)VisibilityMasks.Walls;

			if(visibleObjects.Debris) maskBuilder |= (int)VisibilityMasks.Debris;
			if(visibleObjects.UI) maskBuilder |= (int)VisibilityMasks.UI;
			if(visibleObjects.Avatar) maskBuilder |= (int)VisibilityMasks.Avatar;

			maskBuilder |= (int)(type == CameraType.FirstPerson ? VisibilityMasks.FirstPerson : VisibilityMasks.ThirdPerson);

			if(cam.UCamera.cullingMask != maskBuilder)
				cam.UCamera.cullingMask = maskBuilder;
		}

		public void Save() {
			System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
		}

		private CameraType _type = CameraType.Attached;
		[JsonConverter(typeof(StringEnumConverter))]
		public CameraType type {
			get { return _type; }
			set {
				_type = value;
				cam.ActivateWorldCamIfNecessary();
				ApplyLayerBitmask();
			}
		}

		private bool _showWorldCam = true;
		public bool showWorldCam { get { return _showWorldCam; } set { _showWorldCam = value; cam.ActivateWorldCamIfNecessary(); } }

		public float FOV { get { return cam.UCamera.fieldOfView; } set { cam.UCamera.fieldOfView = value; } }
		public int layer { get { return (int)cam.UCamera.depth; } set { cam.UCamera.depth = value; CamManager.ApplyViewportLayers(); } }

		public GameObjects visibleObjects { get; private set; }

		private float _renderScale = 1;
		public float renderScale {
			get { return _renderScale; }
			set {
				if(_renderScale == value) return;
				_renderScale = Math.Min(value, 3);
				cam.UpdateRenderTexture();
			}
		}

		private Rect _viewRect = Rect.zero;
		[JsonConverter(typeof(RectConverter))]
		public Rect viewRect {
			get { return _viewRect; }
			set {
				var sizeChanged = cam.renderTexture == null || _viewRect.width != value.width || _viewRect.height != value.height;
				_viewRect = value;

				if(sizeChanged) {
					cam.UCamera.aspect = value.width / value.height;
					cam.UpdateRenderTexture();
				}
			}
		}
		
		public Settings_FPSLimiter FPSLimiter { get; private set; } = new Settings_FPSLimiter();
		public Settings_Smoothfollow Smoothfollow { get; private set; } = new Settings_Smoothfollow();
		public Settings_ModmapExtensions ModmapExtensions { get; private set; } = new Settings_ModmapExtensions();
		public Settings_Follow360 Follow360 { get; private set; } = new Settings_Follow360();


		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetPos = new Vector3(0, 1.5f, -1.5f);
		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetRot = new Vector3(3f, 0, 0);
	}
}
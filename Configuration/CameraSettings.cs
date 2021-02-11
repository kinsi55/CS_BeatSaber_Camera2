using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Camera2.Behaviours;
using Camera2.Utils;
using Camera2.Managers;

namespace Camera2.Configuration {
	enum CameraType {
		FirstPerson,
		Attached, //Unused for now, but mostly implemented - For parenting to arbitrary things like possibly to a saber, etc.
		Positionable
	}
	enum WallVisiblity {
		Visible,
		Transparent,
		Hidden
	}
	enum WorldCamVisibility {
		Visible = 0,
		//Always = 0,
		HiddenWhilePlaying,
		Hidden = 2,
		//Never = 2
	}
	enum NoteVisibility {
		Hidden,
		Visible,
		ForceCustomNotes
	}

	[JsonObject(MemberSerialization.OptIn)]
	class GameObjects {
		private CameraSettings parentSetting;
		public GameObjects(CameraSettings parentSetting) {
			this.parentSetting = parentSetting;
		}

		[JsonConverter(typeof(StringEnumConverter)), JsonProperty("Walls")]
		private WallVisiblity _Walls = WallVisiblity.Visible;
		[JsonProperty("Debris")]
		private bool _Debris = true; //Maybe make Enum w/ Show / Hide / Linked like in Cam Plus
		[JsonProperty("UI")]
		private bool _UI = true;
		[JsonProperty("Avatar")]
		private bool _Avatar = true;
		[JsonProperty("Floor")]
		private bool _Floor = true;
		[JsonProperty("CutParticles")]
		private bool _CutParticles = true;
		[JsonConverter(typeof(StringEnumConverterMigrateFromBool)), JsonProperty("Notes")]
		private NoteVisibility _Notes = NoteVisibility.Visible;
		//[JsonProperty("EverythingElse")]
		//private bool _EverythingElse = true;


		public WallVisiblity Walls { get { return _Walls; } set { _Walls = value; parentSetting.ApplyLayerBitmask(); } }
		public bool Debris { get { return _Debris; } set { _Debris = value; parentSetting.ApplyLayerBitmask(); } }
		public bool UI { get { return _UI; } set { _UI = value; parentSetting.ApplyLayerBitmask(); } }
		public bool Avatar { get { return _Avatar; } set { _Avatar = value; parentSetting.ApplyLayerBitmask(); } }
		public bool Floor { get { return _Floor; } set { _Floor = value; parentSetting.ApplyLayerBitmask(); } }
		public bool CutParticles { get { return _CutParticles; } set { _CutParticles = value; parentSetting.ApplyLayerBitmask(); } }
		public NoteVisibility Notes { get { return _Notes; } set { _Notes = value; parentSetting.ApplyLayerBitmask(); } }
		// Wouldnt be very useful since I havent figured out yet how to make cams have transparency
		//public bool EverythingElse { get { return _EverythingElse; } set { _EverythingElse = value; parentSetting.ApplyLayerBitmask(); } }
	}
	
	class CameraSettings {
		private Cam2 cam;
		private bool isLoaded = false;

		public CameraSettings(Cam2 cam) {
			this.cam = cam;

			visibleObjects = new GameObjects(this);
		}

		public void Load(bool loadConfig = true) {
			// Set default values incase they're removed from the JSON because of user stoopid
			isLoaded = false;
			FOV = 90;
			viewRect = new Rect(0, 0, Screen.width, Screen.height);

			if(loadConfig && System.IO.File.Exists(cam.configPath)) {
				JsonConvert.PopulateObject(System.IO.File.ReadAllText(cam.configPath), this, JsonHelpers.leanDeserializeSettings);
			} else {
				layer = CamManager.cams.Count == 0 ? -1000 : CamManager.cams.Max(x => x.Value.settings.layer) - 1;
			}
			// We always save after loading, even if its a fresh load. This will make sure to migrate configs after updates.
			Save();

			ApplyPositionAndRotation();
			ApplyLayerBitmask();
			cam.ShowWorldCamIfNecessary();
			// Trigger setter for cam aspect ratio
			viewRect = viewRect;
			cam.UpdateRenderTextureAndView();
			isLoaded = true;
		}

		public void Save() {
			System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public void Reload() {
			Load();
			foreach(var x in cam.middlewares)
				x.CamConfigReloaded();
		}

		public void ApplyPositionAndRotation() {
			if(type == CameraType.Positionable) {
				cam.transform.position = targetPos;
				cam.transform.eulerAngles = targetRot;
				cam.UCamera.transform.localPosition = Vector3.zero;
				cam.UCamera.transform.localEulerAngles = Vector3.zero;
			} else {
				/*
				 * The cam game object is what is parented, so I'm using the Cameras 
				 * position (Which is nested / parented) to apply an offset
				 */
				cam.UCamera.transform.localPosition = targetPos;
				cam.UCamera.transform.localEulerAngles = targetRot;
			}
		}

		public void ApplyLayerBitmask() {
			VisibilityMasks maskBuilder = (VisibilityMasks)CamManager.clearedBaseCullingMask;

			if(visibleObjects.Walls == WallVisiblity.Visible || (ModmapExtensions.autoOpaqueWalls && SceneUtil.isProbablyInWallMap)) {
				maskBuilder |= VisibilityMasks.Walls | VisibilityMasks.WallTextures;
			} else if(visibleObjects.Walls == WallVisiblity.Transparent) {
				maskBuilder |= VisibilityMasks.Walls;
			}

			if(visibleObjects.Notes != NoteVisibility.Hidden) {
				if(visibleObjects.Notes == NoteVisibility.ForceCustomNotes && CustomNotesUtil.HasHMDOnlyEnabled()) {
					maskBuilder |= VisibilityMasks.CustomNotes;
				} else {
					maskBuilder |= VisibilityMasks.Notes;
				}
			}

			if(visibleObjects.Avatar) {
				maskBuilder |= VisibilityMasks.Avatar;

				maskBuilder |= type == CameraType.FirstPerson ? VisibilityMasks.FirstPersonAvatar : VisibilityMasks.ThirdPersonAvatar;
			}

			if(visibleObjects.Floor) maskBuilder |= VisibilityMasks.Floor;
			if(visibleObjects.Debris) maskBuilder |= VisibilityMasks.Debris;
			if(visibleObjects.CutParticles) maskBuilder |= VisibilityMasks.CutParticles;
			if(visibleObjects.UI && (!ModmapExtensions.autoHideHUD || !SceneUtil.isProbablyInWallMap))
				maskBuilder |= VisibilityMasks.UI;

			if(cam.UCamera.cullingMask != (int)maskBuilder)
				cam.UCamera.cullingMask = (int)maskBuilder;
		}
		
		private CameraType _type = CameraType.FirstPerson;
		[JsonConverter(typeof(StringEnumConverter))]
		public CameraType type {
			get { return _type; }
			set {
				_type = value;
				//TODO: Temporary implementation to migrate Attached to FirstPerson as I had Attached as the default before accidently
				if(_type == CameraType.Attached)
					_type = CameraType.FirstPerson;

				if(!isLoaded)
					return;
				cam.ShowWorldCamIfNecessary();
				ApplyLayerBitmask();
			}
		}
		
		private WorldCamVisibility _worldCamVisibility = WorldCamVisibility.HiddenWhilePlaying;
		[JsonConverter(typeof(StringEnumConverter))]
		public WorldCamVisibility worldCamVisibility {
			get { return _worldCamVisibility; }
			set {
				_worldCamVisibility = value;
				if(isLoaded)
					cam.ShowWorldCamIfNecessary();
			}
		}

		private float _previewScreenSize = 0.3f;
		public float previewScreenSize {
			get { return _previewScreenSize; }
			set {
				_previewScreenSize = Mathf.Clamp(value, 0.3f, 3f);
				if(isLoaded)
					cam.worldCam?.SetPreviewPositionAndSize();
			}
		}

		public float FOV { get { return cam.UCamera.fieldOfView; } set { cam.UCamera.fieldOfView = value; } }
		public int layer {
			get { return (int)cam.UCamera.depth; }
			set {
				cam.UCamera.depth = value;
				CamManager.ApplyCameraValues();
			}
		}

		private int _antiAliasing = 1;
		public int antiAliasing {
			get { return _antiAliasing; }
			set {
				_antiAliasing = Mathf.Clamp(value, 1, 8);
				if(isLoaded) 
					cam.UpdateRenderTextureAndView();
			}
		}

		private float _renderScale = 1;
		public float renderScale {
			get { return _renderScale; }
			set {
				_renderScale = Mathf.Clamp(value, 0.2f, 3f);
				if(isLoaded)
					cam.UpdateRenderTextureAndView();
			}
		}


		public GameObjects visibleObjects { get; private set; }

		[JsonConverter(typeof(RectConverter)), JsonProperty("viewRect")]
		private Rect _viewRect = Rect.zero;
		[JsonIgnore]
		public Rect viewRect {
			get { return _viewRect; }
			set {
				if(value.width <= 0f) value.width = Screen.width - value.x;
				if(value.height <= 0f) value.height = Screen.height - value.y;

				_viewRect = value;
				cam.UCamera.aspect = value.width / value.height;
				if(isLoaded)
					cam.UpdateRenderTextureAndView();
				if(!isLoaded)
					cam.screenImage.SetPositionClamped(Vector3.zero, false);
			}
		}
		
		public Settings_FPSLimiter FPSLimiter { get; private set; } = new Settings_FPSLimiter();
		public Settings_Smoothfollow Smoothfollow { get; private set; } = new Settings_Smoothfollow();
		public Settings_ModmapExtensions ModmapExtensions { get; private set; } = new Settings_ModmapExtensions();
		public Settings_Follow360 Follow360 { get; private set; } = new Settings_Follow360();


		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetPos = new Vector3(0, 0, 0);
		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetRot = new Vector3(0, 0, 0);
		public Settings_MovementScript MovementScript { get; private set; } = new Settings_MovementScript();
	}
}
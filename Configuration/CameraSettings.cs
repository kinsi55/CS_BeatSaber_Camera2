using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Camera2.Behaviours;
using Camera2.Utils;
using Camera2.Managers;
using Camera2.Interfaces;
using Camera2.HarmonyPatches;
using Camera2.SDK;

namespace Camera2.Configuration {
	public enum CameraType {
		FirstPerson,
		Attached, //Unused for now, but mostly implemented - For parenting to arbitrary things like possibly to a saber, etc.
		Positionable
	}
	public enum WallVisiblity {
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
	public enum NoteVisibility {
		Hidden,
		Visible,
		ForceCustomNotes
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class GameObjects {
		private CameraSettings parentSetting;
		internal GameObjects(CameraSettings parentSetting) {
			this.parentSetting = parentSetting;
		}

		public GameObjects GetCopy() {
			var x = new GameObjects(null);
			x._Walls = _Walls;
			x._Debris = _Debris;
			x._UI = _UI;
			x._Avatar = _Avatar;
			x._Floor = _Floor;
			x._CutParticles = _CutParticles;
			x._Notes = _Notes;
			return x;
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


		public WallVisiblity Walls { get => _Walls; set { _Walls = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Debris { get => _Debris; set { _Debris = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool UI { get => _UI; set { _UI = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Avatar { get => _Avatar; set { _Avatar = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Floor { get => _Floor; set { _Floor = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool CutParticles { get => _CutParticles; set { _CutParticles = value; parentSetting?.ApplyLayerBitmask(); } }
		public NoteVisibility Notes { get => _Notes; set { _Notes = value; parentSetting?.ApplyLayerBitmask(); } }
		// Wouldnt be very useful since I havent figured out yet how to make cams have transparency
		//public bool EverythingElse { get { return _EverythingElse; } set { _EverythingElse = value; parentSetting.ApplyLayerBitmask(); } }
	}
	
	class CameraSettings {
		internal Cam2 cam { get; private set; }
		internal bool isLoaded { get; private set; } = false;

		internal OverrideToken overrideToken = null;

		public CameraSettings(Cam2 cam) {
			this.cam = cam;

			_visibleObjects = new GameObjects(this);

			FPSLimiter = CameraSubSettings.GetFor<Settings_FPSLimiter>(this);
			Smoothfollow = CameraSubSettings.GetFor<Settings_Smoothfollow>(this);
			ModmapExtensions = CameraSubSettings.GetFor<Settings_ModmapExtensions>(this);
			Follow360 = CameraSubSettings.GetFor<Settings_Follow360>(this);
			VMCProtocol = CameraSubSettings.GetFor<Settings_VMCAvatar>(this);
		}

		public void Load(bool loadConfig = true) {
			isLoaded = false;
			// Set default value incase they're not loaded from JSON
			viewRect = new Rect(0, 0, -1, -1);
			FOV = 90f;

			if(System.IO.File.Exists(cam.configPath)) {
				if(loadConfig)
					JsonConvert.PopulateObject(System.IO.File.ReadAllText(cam.configPath), this, JsonHelpers.leanDeserializeSettings);
			} else {
				layer = CamManager.cams.Count == 0 ? 1 : CamManager.cams.Max(x => x.Value.settings.layer) + 1;
			}
			// We always save after loading, even if its a fresh load. This will make sure to migrate configs after updates.
#if !DEV
			Save();
#endif
			
			ApplyPositionAndRotation();
			ApplyLayerBitmask();
			cam.UpdateRenderTextureAndView();
			cam.ShowWorldCamIfNecessary();
			isLoaded = true;
		}

		public void Save() {
			if(cam != null && cam.gameObject != null) {
				var x = overrideToken; overrideToken = null;
				try {
					System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
				} catch(Exception ex) {
					Plugin.Log.Error($"Failed to save Config for Camera {cam.name}:");
					Plugin.Log.Error(ex);
				}
				overrideToken = x;
			}
		}

		public void Reload() {
			Load();
			foreach(var x in cam.middlewares)
				x.CamConfigReloaded();
		}

		public void ApplyPositionAndRotation() {
			cam.transformer.position = targetPos;
			cam.transformer.rotationEuler = targetRot;

			cam.transformer.applyAsAbsolute = type != CameraType.Positionable && !Smoothfollow.pivotingOffset;
		}

		public void ApplyLayerBitmask() {
			VisibilityMasks maskBuilder = (VisibilityMasks)CamManager.clearedBaseCullingMask;

			if(visibleObjects.Walls == WallVisiblity.Visible || (ModmapExtensions.autoOpaqueWalls && HookLeveldata.isWallMap)) {
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

			if(visibleObjects.Floor) maskBuilder |= VisibilityMasks.Floor | VisibilityMasks.PlayerPlattform;
			if(visibleObjects.Debris) maskBuilder |= VisibilityMasks.Debris;
			if(visibleObjects.CutParticles) maskBuilder |= VisibilityMasks.CutParticles;
			if(visibleObjects.UI && (!ModmapExtensions.autoHideHUD || !HookLeveldata.isWallMap))
				maskBuilder |= VisibilityMasks.UI;

			if(cam.UCamera.cullingMask != (int)maskBuilder)
				cam.UCamera.cullingMask = (int)maskBuilder;
		}

		public void Unoverriden(Action accessor) {
			var x = overrideToken; overrideToken = null;
			try {
				accessor();
			} catch(Exception ex) {
				throw ex;
			} finally {
				overrideToken = x;
			}
		}
		


		private CameraType _type = CameraType.FirstPerson;
		[JsonConverter(typeof(StringEnumConverter))]
		public CameraType type {
			get => _type;
			set {
				_type = value;

				if(!isLoaded)
					return;
				cam.ShowWorldCamIfNecessary();
				ApplyLayerBitmask();
				// Pos / Rot is applied differently depending on if its a FP or TP cam
				ApplyPositionAndRotation();
			}
		}
		
		private WorldCamVisibility _worldCamVisibility = WorldCamVisibility.HiddenWhilePlaying;
		[JsonConverter(typeof(StringEnumConverter))]
		public WorldCamVisibility worldCamVisibility {
			get => _worldCamVisibility;
			set {
				_worldCamVisibility = value;
				if(isLoaded)
					cam.ShowWorldCamIfNecessary();
			}
		}

		public bool ShouldSerializeworldCamVisibility() => type == CameraType.Positionable;

		private float _previewScreenSize = 0.3f;
		public float previewScreenSize {
			get => _previewScreenSize;
			set {
				_previewScreenSize = Mathf.Clamp(value, 0.3f, 3f);
				if(isLoaded)
					cam.worldCam?.SetPreviewPositionAndSize();
			}
		}

		public bool ShouldSerializepreviewScreenSize() => type == CameraType.Positionable;

		private float _FOV;
		public float FOV { 
			get => overrideToken?.FOV ?? _FOV; 
			set { _FOV = cam.UCamera.fieldOfView = value; cam.UCamera.orthographicSize = _FOV / 30f; } 
		}

		public int layer {
			get => (int)cam.UCamera.depth;
			set {
				cam.UCamera.depth = value;
				if(isLoaded)
					CamManager.ApplyCameraValues(viewLayer: true);
			}
		}

		private int _antiAliasing = 2;
		public int antiAliasing {
			get => _antiAliasing;
			set {
				_antiAliasing = Mathf.Clamp(value, 1, 8);
				if(isLoaded) 
					cam.UpdateRenderTextureAndView();
			}
		}

		private float _renderScale = 1.3f;
		public float renderScale {
			get => _renderScale;
			set {
				_renderScale = Mathf.Clamp(value, 0.2f, 3f);
				if(isLoaded)
					cam.UpdateRenderTextureAndView();
			}
		}

		public bool orthographic {
			get => cam.UCamera.orthographic;
			set => cam.UCamera.orthographic = value;
		}

		public float farZ {
			get => cam.UCamera.farClipPlane;
			set => cam.UCamera.farClipPlane = value;
		}

		[JsonProperty("visibleObjects")]
		private GameObjects _visibleObjects;
		[JsonIgnore]
		public GameObjects visibleObjects => overrideToken?.visibleObjects ?? _visibleObjects;


		Rect GetClampedViewRect(Rect input) {
			Rect p = new Rect();

			p.x = Math.Max(0, Math.Min(input.x, Screen.width - Math.Max(input.width, LessRawImage.MIN_SIZE)));
			p.y = Math.Max(0, Math.Min(input.y, Screen.height - Math.Max(input.height, LessRawImage.MIN_SIZE)));

			p.width = Mathf.Clamp(input.width, LessRawImage.MIN_SIZE, Screen.width - p.x);
			p.height = Mathf.Clamp(input.height, LessRawImage.MIN_SIZE, Screen.height - p.y);

			return p;
		}

		public Rect UpdateViewRect() {
			viewRect = _viewRectCfg;
			return _viewRect;
		}


		[JsonConverter(typeof(RectConverter)), JsonProperty("viewRect")]
		private Rect iCant {
			get => _viewRectCfg;
			set { viewRect = value; }
		}

		private Rect _viewRectCfg = Rect.zero;
		private Rect _viewRect = Rect.zero;

		[JsonIgnore]
		public Rect viewRect {
			get => _viewRect;
			set {
				if(value.width <= 0) value.width = Screen.width;
				if(value.height <= 0) value.height = Screen.height;

				var x = GetClampedViewRect(value);

				_viewRect = new Rect(x);

				if(x.width >= Screen.width/* && _viewRectCfg.width <= 0*/)
					x.width = -1;
				if(x.height >= Screen.height/* && _viewRectCfg.height <= 0 */)
					x.height = -1;

				_viewRectCfg = x;

				if(isLoaded)
					cam.UpdateRenderTextureAndView();
			}
		}
		
		public Settings_FPSLimiter FPSLimiter { get; private set; }
		public Settings_Smoothfollow Smoothfollow { get; private set; }
		public Settings_ModmapExtensions ModmapExtensions { get; private set; }
		public Settings_Follow360 Follow360 { get; private set; }
		public Settings_VMCAvatar VMCProtocol { get; private set; }

		public bool ShouldSerializeFollow360() => type == CameraType.Positionable;
		public bool ShouldSerializeSmoothfollow() => type != CameraType.Positionable;
		public bool ShouldSerializeVMCProtocol() => type == CameraType.Positionable;

		private Vector3 _targetPos = Vector3.zero;
		private Vector3 _targetRot = Vector3.zero;

		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetPos { get => overrideToken?.position ?? _targetPos; set { _targetPos = value; } }
		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 targetRot { get => overrideToken?.rotation ?? _targetRot; set { _targetRot = value; } }

		public Settings_MovementScript MovementScript { get; private set; } = new Settings_MovementScript();

		public bool ShouldSerializeMovementScript() => type == CameraType.Positionable;
	}
}
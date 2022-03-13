using Camera2.Behaviours;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.SDK;
using Camera2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

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
	public enum AvatarVisibility {
		Hidden,
		Visible,
		ForceVisibleInFP
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
		[JsonConverter(typeof(StringEnumConverterMigrateFromBool)), JsonProperty("Avatar")]
		private AvatarVisibility _Avatar = AvatarVisibility.Visible;
		[JsonProperty("Floor")]
		private bool _Floor = true;
		[JsonProperty("CutParticles")]
		private bool _CutParticles = true;
		[JsonConverter(typeof(StringEnumConverterMigrateFromBool)), JsonProperty("Notes")]
		private NoteVisibility _Notes = NoteVisibility.Visible;
		[JsonProperty("EverythingElse")]
		private bool _EverythingElse = true;
		[JsonProperty("Sabers")]
		private bool _Sabers = true;


		public WallVisiblity Walls { get => _Walls; set { _Walls = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Debris { get => _Debris; set { _Debris = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool UI { get => _UI; set { _UI = value; parentSetting?.ApplyLayerBitmask(); } }
		public AvatarVisibility Avatar { get => _Avatar; set { _Avatar = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Floor { get => _Floor; set { _Floor = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool CutParticles { get => _CutParticles; set { _CutParticles = value; parentSetting?.ApplyLayerBitmask(); } }
		public NoteVisibility Notes { get => _Notes; set { _Notes = value; parentSetting?.ApplyLayerBitmask(); } }
		public bool Sabers { get { return _Sabers; } set { _Sabers = value; parentSetting.ApplyLayerBitmask(); } }
		public bool EverythingElse { get { return _EverythingElse; } set { _EverythingElse = value; parentSetting.ApplyLayerBitmask(); } }
	}

	class CameraSettings {
		internal Cam2 cam { get; private set; }
		internal bool isLoaded { get; private set; } = false;

		internal OverrideToken overrideToken = null;

		public CameraSettings(Cam2 cam) {
			this.cam = cam;

			_visibleObjects = new GameObjects(this);

			Multiplayer = CameraSubSettings.GetFor<Settings_Multiplayer>(this);
			FPSLimiter = CameraSubSettings.GetFor<Settings_FPSLimiter>(this);
			Smoothfollow = CameraSubSettings.GetFor<Settings_Smoothfollow>(this);
			ModmapExtensions = CameraSubSettings.GetFor<Settings_ModmapExtensions>(this);
			Follow360 = CameraSubSettings.GetFor<Settings_Follow360>(this);
			VMCProtocol = CameraSubSettings.GetFor<Settings_VMCAvatar>(this);
			PostProcessing = CameraSubSettings.GetFor<Settings_PostProcessing>(this);
		}

		public void Load(bool loadConfig = true) {
			isLoaded = false;
			// Set default value incase they're not loaded from JSON
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

			viewRect ??= new ScreenRect(0, 0, 1, 1, false);

			ApplyPositionAndRotation();
			ApplyLayerBitmask();
			cam.UpdateRenderTextureAndView();
			cam.ShowWorldCamIfNecessary();
			isLoaded = true;
		}

		public void Save() {
			if(cam == null)
				return;
			var x = overrideToken; overrideToken = null;
			try {
				System.IO.File.WriteAllText(cam.configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
			} catch(Exception ex) {
				Plugin.Log.Error($"Failed to save Config for Camera {cam.name}:");
				Plugin.Log.Error(ex);
			}
			overrideToken = x;
		}

		public void Reload() {
			Load();
			foreach(var x in cam.middlewares)
				x.CamConfigReloaded();
		}

		public void ApplyPositionAndRotation() {
			cam.transformer.position = targetPos;
			cam.transformer.rotationEuler = targetRot;

			// Force pivoting offset for 360 Levels - Non-Pivoting offset on 360 levels just looks outright trash
			cam.transformer.applyAsAbsolute = type != CameraType.Positionable && !Smoothfollow.pivotingOffset && !HookLeveldata.is360Level;
		}

		public void ApplyLayerBitmask() {
			VisibilityMasks maskBuilder = visibleObjects.EverythingElse ? (VisibilityMasks)CamManager.clearedBaseCullingMask : 0;

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

			if(visibleObjects.Avatar != AvatarVisibility.Hidden) {
				maskBuilder |= VisibilityMasks.Avatar;

				maskBuilder |= (type == CameraType.FirstPerson && visibleObjects.Avatar != AvatarVisibility.ForceVisibleInFP) ? VisibilityMasks.FirstPersonAvatar : VisibilityMasks.ThirdPersonAvatar;
			}

			if(visibleObjects.Floor) maskBuilder |= VisibilityMasks.Floor | VisibilityMasks.PlayerPlattform;
			if(visibleObjects.Debris) maskBuilder |= VisibilityMasks.Debris;
			if(visibleObjects.CutParticles) maskBuilder |= VisibilityMasks.CutParticles;
			if(visibleObjects.Sabers) maskBuilder |= VisibilityMasks.Sabers;
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
				if(isLoaded && cam.worldCam != null)
					cam.worldCam.SetPreviewPositionAndSize();
			}
		}

		private bool _worldCamUnderScreen = true;
		public bool worldCamUnderScreen {
			get => _worldCamUnderScreen;
			set {
				_worldCamUnderScreen = value;
				if(isLoaded && cam.worldCam != null)
					cam.worldCam.SetPreviewPositionAndSize();
			}
		}

		public bool ShouldSerializepreviewScreenSize() => type == CameraType.Positionable;
		public bool ShouldSerializeworldCamUnderScreen() => type == CameraType.Positionable;

		private float _FOV;
		public float FOV {
			get => overrideToken?.FOV ?? _FOV;
			set { _FOV = cam.UCamera.fieldOfView = value; cam.UCamera.orthographicSize = _FOV * 0.0333f; }
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
			set {
				/* TODO: Remove this at some point 😀
				 * Ingame farZ changed to 5k from 1k at some point without me noticing
				 */
				if(value >= 1000f)
					value = 5000f;

				cam.UCamera.farClipPlane = value;
			}
		}

		[JsonProperty("visibleObjects")]
		private GameObjects _visibleObjects;
		[JsonIgnore]
		public GameObjects visibleObjects => overrideToken?.visibleObjects ?? _visibleObjects;


		internal class ScreenRect {
			public float x;
			public float y;
			public float width;
			public float height;
			public bool locked;

			public ScreenRect(float x, float y, float width, float height, bool locked) {
				this.x = x;
				this.y = y;
				this.width = width;
				this.height = height;
				this.locked = locked;
			}

			public Rect ToRect() => new Rect(x, y, width, height);

			public Vector2 MinAnchor() => new Vector2(x, y);
			public Vector2 MaxAnchor() => new Vector2(x + width, y + height);
		}

		[JsonConverter(typeof(ScreenRectConverter)), JsonProperty("viewRect")]
		ScreenRect _viewRectCfg {
			get => viewRect; set {
				value.width = Math.Min(1, Math.Abs(value.width));
				value.height = Math.Min(1, Math.Abs(value.height));
				value.x = Math.Min(1 - value.width, Math.Abs(value.x));
				value.y = Math.Min(1 - value.height, Math.Abs(value.y));

				viewRect = value;
			}
		}

		[JsonIgnore]
		internal ScreenRect viewRect { get; private set; }

		public void SetViewRect(float? x, float? y, float? width, float? height) {
			viewRect.x = x ?? viewRect.x;
			viewRect.y = y ?? viewRect.y;
			viewRect.width = width ?? viewRect.width;
			viewRect.height = height ?? viewRect.height;

			_viewRectCfg = viewRect;

			if(isLoaded)
				cam.UpdateRenderTextureAndView();
		}

		internal bool isScreenLocked {
			get => viewRect.locked;
			set => viewRect.locked = value;
		}

		public Settings_Multiplayer Multiplayer { get; private set; }
		public Settings_Smoothfollow Smoothfollow { get; private set; }
		public Settings_ModmapExtensions ModmapExtensions { get; private set; }
		public Settings_Follow360 Follow360 { get; private set; }
		public Settings_VMCAvatar VMCProtocol { get; private set; }
		public Settings_FPSLimiter FPSLimiter { get; private set; }
		public Settings_PostProcessing PostProcessing { get; private set; }

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

		public bool ShouldSerializeMovementScript() => type != CameraType.FirstPerson;
	}
}
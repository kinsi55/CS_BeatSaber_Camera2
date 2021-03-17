//#define FPSCOUNT

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IPA.Utilities;
using Camera2.Interfaces;
using Camera2.Middlewares;
using Camera2.Configuration;
using Camera2.Utils;
using System.Reflection;
using Camera2.HarmonyPatches;

namespace Camera2.Behaviours {

#if DEBUG
	public
#endif
	class Cam2 : MonoBehaviour {
		internal new string name { get; private set; } = null;
		internal string configPath { get => ConfigUtil.GetCameraPath(name); }

		internal Camera UCamera { get; private set; } = null;
		internal CameraSettings settings { get; private set; } = null;
		internal RenderTexture renderTexture { get; private set; } = null;

		internal LessRawImage previewImage { get; private set; } = null;
		internal PositionableCam worldCam { get; private set; } = null;

		internal List<IMHandler> middlewares { get; private set; } = new List<IMHandler>();

		internal Transform transformer;
		
		public void Awake() {
			DontDestroyOnLoad(gameObject);
		}


		ParentShield shield;
		public void SetOrigin(Transform parent, bool unparentOnDisable = true) {
			if(transform.parent == parent)
				return;

			if(parent == null) {
				transform.parent = null;
				// When parented to a / the origin it should already contain the offset, otherwise we need to manually apply it (unparented)
				transform.localPosition = HookRoomAdjust.instance != null ? HookRoomAdjust.instance.transform.position : Vector3.zero;
				transform.localRotation = HookRoomAdjust.instance != null ? HookRoomAdjust.instance.transform.rotation : Quaternion.identity;

				DontDestroyOnLoad(gameObject);
			} else {
				if(shield == null)
					shield = new GameObject($"Cam2_{name}_Parenter").AddComponent<ParentShield>();

				shield.Init(this, parent, false);

				transform.SetParent(shield.transform, false);
			}

			// Previous parent might've messed up the rot/pos, so lets fix it.
			settings.ApplyPositionAndRotation();
		}

		List<KeyValuePair<string, Transformer>> transformers = new List<KeyValuePair<string, Transformer>>();
		public Transformer GetTransformer(string type) {
			foreach(var x in transformers)
				if(x.Key == type) return x.Value;

			return null;
		}
		public Transformer GetOrCreateTransformer(string type, TransformerOrders order = TransformerOrders.Default) {
			foreach(var x in transformers)
				if(x.Key == type) return x.Value;

			Transform parent = transform;
			Transform child = UCamera.transform;
			int index = transformers.FindIndex(x => x.Value.order >= (int)order);

			if(index == -1)
				index = transformers.Count();

			if(transformers.Count() > 0) {
				if(index > 0)
					parent = transformers[index - 1].Value.transform;

				if(transformers.Count() > index)
					child = transformers[index].Value.transform;
			}


			var n = Transformer.Get(type, (int)order, this, transformers);
			
			n.transform.SetParent(parent, false);
			child.SetParent(n.transform, false);

			transformers.Insert(index, new KeyValuePair<string, Transformer>(type, n));


			return n;
		}



		internal void UpdateRenderTextureAndView() {
			var w = (int)Math.Round(settings.viewRect.width * settings.renderScale);
			var h = (int)Math.Round(settings.viewRect.height * settings.renderScale);

			var sizeChanged = renderTexture?.width != w || renderTexture?.height != h || renderTexture?.antiAliasing != settings.antiAliasing;

			if(sizeChanged) {
				renderTexture?.Release();
				renderTexture = new RenderTexture(w, h, 24) { //, RenderTextureFormat.ARGB32
					autoGenerateMips = false,
					antiAliasing = settings.antiAliasing,
					anisoLevel = 1,
					useDynamicScale = false
				};

				UCamera.aspect = (float)w / (float)h;
				UCamera.targetTexture = renderTexture;
				worldCam?.SetSource(this);
			}

			if(sizeChanged || previewImage.position.x != settings.viewRect.x || previewImage.position.y != settings.viewRect.y)
				previewImage?.SetSource(this);
		}

		internal void ShowWorldCamIfNecessary() {
			if(worldCam == null)
				return;

			bool doShowCam = 
				settings.type == Configuration.CameraType.Positionable &&
				settings.worldCamVisibility != WorldCamVisibility.Hidden &&
				(settings.worldCamVisibility != WorldCamVisibility.HiddenWhilePlaying || !SceneUtil.isSongPlaying);

			worldCam.gameObject.SetActive(doShowCam || (UI.SettingsView.cam == this && settings.type == Configuration.CameraType.Positionable));
		}

		public void Init(string name, LessRawImage presentor = null, bool loadConfig = false, bool rename = false) {
			if(this.name != null) {
				if(rename) {
					this.name = name;
					if(loadConfig) settings.Load(true);
				}
				return;
			}

			this.name = name;
			previewImage = presentor;

			var camClone = Instantiate(SceneUtil.GetMainCameraButReally(), Vector3.zero, Quaternion.identity, transform);
			camClone.name = "Cam";


			UCamera = camClone.GetComponent<Camera>();
			UCamera.enabled = false;
			UCamera.clearFlags = CameraClearFlags.SolidColor;
			UCamera.stereoTargetEye = StereoTargetEyeMask.None;
			//UCamera.depthTextureMode = DepthTextureMode.None;
			//UCamera.renderingPath = RenderingPath.DeferredLighting;

			transformer = GetOrCreateTransformer("Position", TransformerOrders.PositionOffset).transform;


			foreach(var child in camClone.transform.Cast<Transform>())
				Destroy(child.gameObject);
			
			var trash = new string[] { "AudioListener", "LIV", "MainCamera", "MeshCollider" };
			foreach(var component in camClone.GetComponents<Behaviour>())
				if(trash.Contains(component.GetType().Name)) Destroy(component);


			//Cloning post process stuff to make it controlable on a per camera basis
			//BloomShite.InstantiateBloomForCamera(UCamera).tag = null;
			//typeof(VisualEffectsController)
			//.GetField("_depthTextureEnabled", BindingFlags.Instance | BindingFlags.NonPublic)
			//.SetValue(camClone.GetComponent<VisualEffectsController>(), new BoolSO() { value = UCamera.depthTextureMode != DepthTextureMode.None });



			worldCam = new GameObject("WorldCam").AddComponent<PositionableCam>();
			worldCam.transform.parent = camClone.transform;

			settings = new CameraSettings(this);
			settings.Load(loadConfig);


			AddMiddleware<FPSLimiter>();
			AddMiddleware<Smoothfollow>();
			AddMiddleware<ModmapExtensions>();
			AddMiddleware<Follow360>();
			AddMiddleware<MovementScriptProcessor>();

#if DEV
			AddTransformer<PostProcessor>();
#endif
		}

		private void AddMiddleware<T>() where T: CamMiddleware, IMHandler {
			middlewares.Add(gameObject.AddComponent<T>().Init(this));
		}

		internal float timeSinceLastRender { get; private set; } = 0f;

		private bool hadUpdate = false;
		private void Update() {
			timeSinceLastRender += Time.deltaTime;
			hadUpdate = true;
		}

#if FPSCOUNT
		int renderedFrames = 0;
		System.Diagnostics.Stopwatch sw = null;
#endif
		private void OnGUI() {
			if(UCamera == null || renderTexture == null || !hadUpdate)
				return;
#if FPSCOUNT
			if(sw == null) {
				sw = new System.Diagnostics.Stopwatch();
				sw.Start();
			}
#endif

			foreach(var t in middlewares) {
				if(!t.Pre())
					return;
			}

			hadUpdate = false;
			UCamera.Render();

			foreach(var t in middlewares)
				t.Post();

			timeSinceLastRender = 0f;
#if FPSCOUNT
			renderedFrames++;
			if(sw.ElapsedMilliseconds > 500) {
				Console.WriteLine("Rendered FPS for {1}: {0}", renderedFrames * 2, name);
				renderedFrames = 0;
				sw.Restart();
			}
#endif
		}
		
		private void OnEnable() {
			// Force a render here so we dont end up with a stale image after having just enabled this camera
			UCamera?.Render();
			if(previewImage != null)
				previewImage.gameObject?.SetActive(true);
			ShowWorldCamIfNecessary();
		}
		
		private void OnDisable() {
			if(previewImage != null) previewImage.gameObject?.SetActive(false);
			ShowWorldCamIfNecessary();
		}

		internal bool destroying { get; private set; } = false;
		private void OnDestroy() {
			destroying = true;
			gameObject.SetActive(false);

			foreach(var component in UCamera.gameObject.GetComponents<Behaviour>())
				if(component.GetType() != typeof(Camera))
					Destroy(component);

			//if(UCamera != null) Destroy(UCamera);
			if(previewImage != null) Destroy(previewImage.gameObject);
			if(shield != null) Destroy(shield.gameObject);
			Destroy(gameObject);
		}
	}
}

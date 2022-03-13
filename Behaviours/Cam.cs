//#define FPSCOUNT

using Camera2.Configuration;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Middlewares;
using Camera2.UI;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Camera2.Behaviours {

#if DEBUG
	public
#endif
	class Cam2 : MonoBehaviour {
		internal new string name { get; private set; } = null;
		internal string configPath { get => ConfigUtil.GetCameraPath(name); }

		internal bool isCurrentlySelectedInSettings => Coordinator.instance && Coordinator.instance.settingsView.isActiveAndEnabled && SettingsView.cam == this;

		internal Camera UCamera { get; private set; } = null;
		internal CameraSettings settings { get; private set; } = null;
		internal RenderTexture renderTexture { get; private set; } = null;

		internal CameraDesktopView previewImage { get; private set; } = null;
		internal PositionableCam worldCam { get; private set; } = null;

		internal IMHandler[] middlewares { get; private set; }

		internal Transformer transformer;
		internal TransformChain transformchain;

		public void Awake() {
			DontDestroyOnLoad(gameObject);
		}

		ParentShield shield;
		public void SetOrigin(Transform parent, bool startFromParentTransform = true, bool unparentOnDisable = true) {
			if(transform.parent == parent)
				return;

			if(parent == null) {
				transform.parent = null;

				DontDestroyOnLoad(gameObject);
			} else {
				if(shield == null)
					shield = new GameObject($"Cam2_{name}_Parenter").AddComponent<ParentShield>();

				shield.Init(this, parent, !startFromParentTransform);

				transform.SetParent(shield.transform, !startFromParentTransform);
			}

			settings.ApplyPositionAndRotation();
		}


		internal void UpdateRenderTextureAndView() {
			var w = (int)Math.Round(settings.viewRect.width * Screen.width * settings.renderScale);
			var h = (int)Math.Round(settings.viewRect.height * Screen.height * settings.renderScale);

			var sizeChanged = renderTexture == null || renderTexture.width != w || renderTexture.height != h || renderTexture.antiAliasing != settings.antiAliasing;

			if(sizeChanged) {
				if(renderTexture != null)
					renderTexture.Release();

				renderTexture = new RenderTexture(w, h, 24) { //, RenderTextureFormat.ARGB32
					useMipMap = false,
					antiAliasing = settings.antiAliasing,
					anisoLevel = 1,
					useDynamicScale = false
				};

				UCamera.aspect = (float)w / (float)h;
				UCamera.targetTexture = renderTexture;
				if(worldCam != null)
					worldCam.SetSource(this);

				PrepareMiddlewaredRender(true);
			}

			if(previewImage != null && (sizeChanged || previewImage.rekt.anchorMin != settings.viewRect.MinAnchor()))
				previewImage.SetSource(this);
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

		internal void UpdateDepthTextureActive() {
			if(UCamera != null)
				UCamera.depthTextureMode = InitOnMainAvailable.useDepthTexture || settings?.PostProcessing.forceDepthTexture == true ? DepthTextureMode.Depth : DepthTextureMode.None;
		}

		static readonly HashSet<string> CameraBehavioursToDestroy = new[] { "AudioListener", "LIV", "MainCamera", "MeshCollider" }.ToHashSet();

		public void Init(string name, CameraDesktopView presentor = null, bool loadConfig = false, bool rename = false) {
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
			UCamera.farClipPlane = 5000f;
			UCamera.enabled = false;
			UCamera.tag = "Untagged";
			UCamera.clearFlags = CameraClearFlags.SolidColor;
			UCamera.stereoTargetEye = StereoTargetEyeMask.None;
			UpdateDepthTextureActive();

			transformchain = new TransformChain(transform, UCamera.transform);
			transformer = transformchain.AddOrGet("Position", TransformerOrders.PositionOffset, false);


			foreach(Transform child in camClone.transform)
				Destroy(child.gameObject);

			foreach(var component in camClone.GetComponents<Behaviour>())
				if(CameraBehavioursToDestroy.Contains(component.GetType().Name)) DestroyImmediate(component);


			//Cloning post process stuff to make it controlable on a per camera basis
			//BloomShite.InstantiateBloomForCamera(UCamera).tag = null;
			//typeof(VisualEffectsController)
			//.GetField("_depthTextureEnabled", BindingFlags.Instance | BindingFlags.NonPublic)
			//.SetValue(camClone.GetComponent<VisualEffectsController>(), new BoolSO() { value = UCamera.depthTextureMode != DepthTextureMode.None });



			worldCam = new GameObject("WorldCam").AddComponent<PositionableCam>();
			worldCam.transform.parent = camClone.transform;

			settings = new CameraSettings(this);
			settings.Load(loadConfig);


			middlewares = new[] {
				MakeMiddleware<Multiplayer>(),
				MakeMiddleware<FPSLimiter>(),
				MakeMiddleware<Smoothfollow>(),
				MakeMiddleware<ModmapExtensions>(),
				MakeMiddleware<Follow360>(),
				MakeMiddleware<MovementScriptProcessor>(),
				MakeMiddleware<VMCAvatar>()
			};

			camClone.AddComponent<CamPostProcessor>().Init(this);
		}

		private IMHandler MakeMiddleware<T>() where T : CamMiddleware, IMHandler {
			return gameObject.AddComponent<T>().Init(this);
		}

		internal float timeSinceLastRender { get; private set; } = 0f;

#if FPSCOUNT
		int renderedFrames = 0;
		System.Diagnostics.Stopwatch sw = null;
#endif
		private void LateUpdate() {
			timeSinceLastRender += Time.deltaTime;

			if(!UCamera || !renderTexture)
				return;
#if FPSCOUNT
			if(sw == null) {
				sw = new System.Diagnostics.Stopwatch();
				sw.Start();
			}
#endif

			PrepareMiddlewaredRender();
#if FPSCOUNT
			if(sw.ElapsedMilliseconds > 500) {
				Console.WriteLine("Rendered FPS for {1}: {0}", renderedFrames * 2, name);
				renderedFrames = 0;
				sw.Restart();
			}
#endif
		}

		internal void PrepareMiddlewaredRender(bool forceRender = false) {
			if(!UCamera || !renderTexture || middlewares == null)
				return;

			foreach(var t in middlewares) {
				if(!t.Pre() && !forceRender)
					return;
			}

			transformchain.Calculate();
			UCamera.enabled = true;

			if(forceRender)
				UCamera.Render();
		}

		internal void PostprocessCompleted() {
			UCamera.enabled = false;

			foreach(var t in middlewares)
				t.Post();

			timeSinceLastRender = 0f;
#if FPSCOUNT
			renderedFrames++;
#endif
		}

		private void OnEnable() {
			// Force a render here so we dont end up with a stale image after having just enabled this camera
			PrepareMiddlewaredRender(true);
			if(previewImage != null)
				previewImage.gameObject.SetActive(true);
			ShowWorldCamIfNecessary();
		}

		private void OnDisable() {
			if(previewImage != null)
				previewImage.gameObject.SetActive(false);

			ShowWorldCamIfNecessary();
		}

		internal bool destroying { get; private set; } = false;
		private void OnDestroy() {
			destroying = true;
			gameObject.SetActive(false);

			if(previewImage != null) Destroy(previewImage.gameObject);
			if(shield != null) Destroy(shield.gameObject);
			Destroy(gameObject);
		}
	}
}

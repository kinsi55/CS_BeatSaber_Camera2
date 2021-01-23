using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using Camera2.Interfaces;
using Camera2.Middlewares;
using Camera2.Configuration;
using Camera2.Utils;

namespace Camera2.Behaviours {

	class Cam2 : MonoBehaviour {
		public new string name { get; private set; }
		public string configPath { get { return ConfigUtil.getCameraPath(name); } }

		internal Camera UCamera { get; private set; }
		public CameraSettings settings { get; private set; }
		internal RenderTexture renderTexture { get; private set; }

		internal LessRawImage screenImage { get; private set; }
		internal PositionableCam worldCam { get; private set; }

		List<IMHandler> middlewares = new List<IMHandler>();

		/// <summary>
		/// Only ever called once, mainly used to initialize variables.
		/// </summary>
		public void Awake() {
			DontDestroyOnLoad(this);
		}

		public void SetParent(Transform parent) {
			if(transform.parent == parent)
				return;

			transform.parent = parent;
			//transform.SetParent(parent, true);

			if(parent == null) {
				DontDestroyOnLoad(this);

				// Previous parent might've messed up the rot/pos, so lets fix it.
				settings.ApplyPositionAndRotation();
			}
		}

		internal void SetRenderTexture() {
			renderTexture?.Release();
			renderTexture = new RenderTexture((int)Math.Round(settings.viewRect.width * settings.renderScale), (int)Math.Round(settings.viewRect.height * settings.renderScale), 24) {
				autoGenerateMips = false,
				antiAliasing = 1,
				anisoLevel = 1,
				useDynamicScale = false
			};

			UCamera.targetTexture = renderTexture;

			screenImage?.SetSource(this);
			worldCam?.SetSource(this);
		}

		internal void ActivateWorldCamIfNecessary() {
			worldCam?.gameObject.SetActive(settings.type != Configuration.CameraType.FirstPerson && settings.showWorldCam);
		}

		public void Init(string name, LessRawImage presentor, bool loadConfig = false) {
			this.name = name;
			screenImage = presentor;

			var camClone = Instantiate(Camera.main.gameObject);
			camClone.name = "Cam";


			UCamera = camClone.GetComponent<Camera>();
			UCamera.enabled = false;
			UCamera.clearFlags = CameraClearFlags.SolidColor;
			UCamera.stereoTargetEye = StereoTargetEyeMask.None;


			foreach(var child in camClone.transform.Cast<Transform>()) Destroy(child.gameObject);

			//TODO: Not sure if VisualEffectsController is really unnecessary, doesnt seem to do anything.
			var trash = new string[] { "AudioListener", "LIV", "MainCamera", "MeshCollider", "VisualEffectsController" };
			foreach(var component in camClone.GetComponents<Behaviour>())
				if(trash.Contains(component.GetType().Name)) Destroy(component);


			camClone.transform.parent = transform;
			camClone.transform.localRotation = Quaternion.identity;
			camClone.transform.localPosition = Vector3.zero;


			//TODO: maybe clone the effectcontroller+>_mainEffectContainer+>_mainEffect so we can customize bloom on a per-camera basis
			
			worldCam = new GameObject("WorldCam").AddComponent<PositionableCam>();
			worldCam.transform.parent = transform;

			settings = new CameraSettings(this);
			settings.Load(loadConfig);

			//worldCam.SetPreviewPositionAndSize();


			AddTransformer<FPSLimiter>();
			AddTransformer<Smoothfollow>();
			AddTransformer<NoodleExtensions>();
			AddTransformer<Follow360>();
			//TODO: FlyingGameHUDRotation - Set as child for 360 maps to apply
		}

		private void AddTransformer<T>() where T: CamMiddleware, IMHandler {
			middlewares.Add(gameObject.AddComponent<T>().Init(this));
		}

		private void Update() {
			if(UCamera != null && renderTexture != null) {
				foreach(var t in middlewares) {
					if(!t.Pre())
						return;
				}

				UCamera.Render();

				foreach(var t in middlewares)
					t.Post();
			}
		}

		/// <summary>
		/// Called every frame after every other enabled script's Update().
		/// </summary>
		private void LateUpdate() {
			
		}

		/// <summary>
		/// Called when the script becomes enabled and active
		/// </summary>
		private void OnEnable() {
			if(screenImage != null) screenImage.enabled = true;
		}

		/// <summary>
		/// Called when the script becomes disabled or when it is being destroyed.
		/// </summary>
		private void OnDisable() {
			if(screenImage != null) screenImage.enabled = false;
		}

		/// <summary>
		/// Called when the script is being destroyed.
		/// </summary>
		private void OnDestroy() {
			settings?.Save();
			Destroy(UCamera);
			Destroy(screenImage);
		}
	}
}

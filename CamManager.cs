﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Camera2.Behaviours;
using Camera2.Utils;

namespace Camera2 {
	static class CamManager {
		public static Dictionary<string, Cam2> cams { get; private set; } = new Dictionary<string, Cam2>();
		static CamerasViewport customScreen;
		public static int baseCullingMask { get; private set; }

		public static void Init() {
			baseCullingMask = Camera.main.cullingMask;

			customScreen = new GameObject("Cam2_Renderer").AddComponent<CamerasViewport>();

			LoadCameras();

			if(cams.Count() == 0) {
				var cam = InitCamera("Main", false);
			}
			
			ScenesManager.settings.Load();

			XRSettings.gameViewRenderMode = GameViewRenderMode.None;
		}

		private static void LoadCameras(bool reload = false) {
			if(!Directory.Exists(ConfigUtil.CamsDir)) {
				Directory.CreateDirectory(ConfigUtil.CamsDir);
			} else {
				var loadedNames = new List<string>();

				foreach(var cam in Directory.GetFiles(ConfigUtil.CamsDir)) {
					if(!cam.ToLower().EndsWith(".json"))
						continue;

					try {
						var name = Path.GetFileNameWithoutExtension(cam);

						InitCamera(name, true, reload);

						if(reload)
							loadedNames.Add(name);
					} catch(Exception ex) {
						Plugin.Log.Error($"Failed to load Camera {Path.GetFileName(cam)}");
						Plugin.Log.Error(ex);
					}
				}
				if(reload) foreach(var deletedCam in cams.Where(x => !loadedNames.Contains(x.Key))) {
					GameObject.Destroy(deletedCam.Value);
					cams.Remove(deletedCam.Key);
				}

				ApplyCameraValues(viewLayer: true);
			}
		}

		public static void Reload() {
			LoadCameras(true);
			ScenesManager.settings.Load();
		}

		/* 
		 * Unfortunately the Canvas Images cannot have their "layer" / z-index set to arbitrary numbers,
		 * so we need to sort the cams by their set layer number and set the sibling index accordingly
		 */
		public static void ApplyCameraValues(bool viewLayer = false, bool bitMask = false, bool worldCam = false, bool posRot = false) {
			var collection = viewLayer ? cams.Values.OrderBy(x => x.settings.layer).AsEnumerable() : cams.Values;
			
			foreach(var cam in collection) {
				if(viewLayer) cam.screenImage.transform.SetAsLastSibling();
				if(bitMask) cam.settings.ApplyLayerBitmask();
				if(worldCam) cam.ShowWorldCamIfNecessary();
				if(posRot) cam.settings.ApplyPositionAndRotation();
			}
		}

		public static Cam2 InitCamera(string name, bool loadConfig = true, bool reload = false) {
			if(cams.ContainsKey(name)) {
				if(reload) {
					cams[name].settings.Load();
					return cams[name];
				}

				throw new Exception("Already exists??");
			}

			var cam = new GameObject($"Cam2_{name}").AddComponent<Cam2>();

			cam.Init(name, customScreen.AddNewView(), loadConfig);

			cams.Add(name, cam);

			//Newly added cameras should always be the last child and thus on top
			//ApplyCameraValues(viewLayer: true);

			return cam;
		}
	}
}

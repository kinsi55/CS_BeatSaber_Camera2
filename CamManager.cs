using System;
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

		public static void Init() {
			customScreen = new GameObject("Cam2_Renderer").AddComponent<CamerasViewport>();

			if(!Directory.Exists(ConfigUtil.CamsDir)) {
				Directory.CreateDirectory(ConfigUtil.CamsDir);
			} else {
				foreach(var cam in Directory.GetFiles(ConfigUtil.CamsDir)) {
					try {
						AddCamera(Path.GetFileNameWithoutExtension(cam));
					} catch(Exception ex) {
						Plugin.Log.Error($"Failed to load Camera {Path.GetFileName(cam)}");
						Plugin.Log.Error(ex);
					}
				}
			}

			if(cams.Count() == 0) {
				var cam = AddCamera("Main", false);
			}
			
			ScenesManager.settings.Load();

			XRSettings.gameViewRenderMode = GameViewRenderMode.None;
		}

		// Unfortunately the Canvas Images cannot have their "layer" / index set to arbitrary numbers
		public static void ApplyViewLayers() {
			foreach(var cam in cams.Values.OrderBy(x => x.settings.layer))
				cam.screenImage.transform.SetAsLastSibling();
		}

		public static Cam2 AddCamera(string name, bool loadConfig = true) {
			if(cams.ContainsKey(name))
				throw new Exception("Already exists??");

			var cam = new GameObject($"Cam2_{name}").AddComponent<Cam2>();

			cam.Init(name, customScreen.AddNewView(), loadConfig);

			cams.Add(name, cam);

			ApplyViewLayers();

			return cam;
		}
	}
}

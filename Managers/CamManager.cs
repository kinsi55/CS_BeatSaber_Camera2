using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Camera2.Behaviours;
using Camera2.Utils;
using Camera2.Configuration;

namespace Camera2.Managers {
	static class CamManager {
		public static Dictionary<string, Cam2> cams { get; private set; } = new Dictionary<string, Cam2>();
		static CamerasViewport customScreen;
		public static int baseCullingMask { get; private set; }
		public static int clearedBaseCullingMask { get; private set; }

		public static void Init() {
			clearedBaseCullingMask = baseCullingMask = SceneUtil.GetMainCameraButReally().GetComponent<Camera>().cullingMask;

			foreach(int mask in Enum.GetValues(typeof(VisibilityMasks)))
				clearedBaseCullingMask &= ~mask;

			customScreen = new GameObject("Cam2_Renderer").AddComponent<CamerasViewport>();

			LoadCameras();
			
			ScenesManager.settings.Load();

			XRSettings.gameViewRenderMode = GameViewRenderMode.None;

			new GameObject("Cam2_Positioner").AddComponent<CamPositioner>();
			
			Settings.SpaghettiUI.Init();
		}

		private static void LoadCameras(bool reload = false) {
			if(!Directory.Exists(ConfigUtil.CamsDir)) {
				Directory.CreateDirectory(ConfigUtil.CamsDir);
			} else {
				var loadedNames = new List<string>();

				foreach(var cam in Directory.GetFiles(ConfigUtil.CamsDir, "*.json")) {
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
				if(reload) foreach(var deletedCam in cams.Where(x => !loadedNames.Contains(x.Key)).ToList()) {
					GameObject.Destroy(deletedCam.Value);
					cams.Remove(deletedCam.Key);
				}

				if(cams.Count() == 0) {
					var cam = InitCamera("Main", false);
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
				if(viewLayer) cam.previewImage.transform.SetAsLastSibling();
				if(bitMask) cam.settings.ApplyLayerBitmask();
				if(worldCam) cam.ShowWorldCamIfNecessary();
				if(posRot) cam.settings.ApplyPositionAndRotation();
			}
		}

		public static Cam2 InitCamera(string name, bool loadConfig = true, bool reload = false) {
			if(cams.ContainsKey(name)) {
				if(reload) {
					cams[name].settings.Reload();
					return cams[name];
				}

				throw new Exception("Already exists??");
			}

			var cam = new GameObject($"Cam2_{name}").AddComponent<Cam2>();

			cam.Init(name, customScreen.AddNewView(), loadConfig);

			cams[name] = cam;

			//Newly added cameras should always be the last child and thus on top
			//ApplyCameraValues(viewLayer: true);

			return cam;
		}

		public static Cam2 AddNewCamera() {
			var i = 1;

			while(cams.ContainsKey($"NewCamera{i}"))
				i++;

			return InitCamera($"NewCamera{i}", false);
		}

		public static void DeleteCamera(Cam2 cam) {
			if(!cams.Values.Contains(cam))
				return;

			if(cams[cam.name] != cam)
				return;

			cams.Remove(cam.name);

			var cfgPath = ConfigUtil.GetCameraPath(cam.name);

			GameObject.DestroyImmediate(cam);

			if(File.Exists(cfgPath))
				File.Delete(cfgPath);
		}

		public static bool RenameCamera(Cam2 cam, string newName) {
			if(cams.ContainsKey(newName))
				return false;

			if(!cams.ContainsValue(cam))
				return false;

			var oldName = cam.name;

			cams[newName] = cam;
			cams.Remove(oldName);

			foreach(var scene in ScenesManager.settings.scenes.Values) {
				if(!scene.Contains(oldName))
					continue;

				scene.Add(newName);
				scene.Remove(oldName);
			}

			cam.settings.Save();
			File.Move(cam.configPath, ConfigUtil.GetCameraPath(newName));
			cam.Init(newName, rename: true);
			ScenesManager.settings.Save();

			return true;
		}
	}
}

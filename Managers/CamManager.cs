using Camera2.Behaviours;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Camera2.Managers {

#if DEBUG
	public
#endif
	static class CamManager {
		public static Dictionary<string, Cam2> cams { get; private set; } = new Dictionary<string, Cam2>();
		internal static CamerasViewport customScreen { get; private set; }
		public static int baseCullingMask { get; internal set; }
		public static int clearedBaseCullingMask { get; private set; }

		public static void Init() {
			clearedBaseCullingMask = baseCullingMask != 0 ? baseCullingMask : SceneUtil.GetMainCameraButReally().GetComponent<Camera>().cullingMask;

			foreach(int mask in Enum.GetValues(typeof(VisibilityMasks)))
				clearedBaseCullingMask &= ~mask;

			//Adding _THIS_IS_NORMAL so that ends up in the stupid warning Unity logs when having a SS overlay w/ active VR
			customScreen = new GameObject("Cam2_Viewport_THIS_IS_NORMAL").AddComponent<CamerasViewport>();

			LoadCameras();

			ScenesManager.settings.Load();

			XRSettings.gameViewRenderMode = GameViewRenderMode.None;

			new GameObject("Cam2_Positioner", typeof(CamPositioner));

			UI.SpaghettiUI.Init();
		}

		private static void LoadCameras(bool reload = false) {
			if(!Directory.Exists(ConfigUtil.CamsDir))
				Directory.CreateDirectory(ConfigUtil.CamsDir);

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
			if(reload) foreach(var deletedCam in cams.Where(x => !loadedNames.Contains(x.Key))) {
				GameObject.Destroy(deletedCam.Value);
				cams.Remove(deletedCam.Key);
			}

			if(cams.Count == 0) {
				var cam = InitCamera("Main", false);
			}

			ApplyCameraValues(viewLayer: true);
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
			var collection = viewLayer ? cams.Values.OrderBy(x => x.isCurrentlySelectedInSettings ? int.MaxValue : x.settings.layer).AsEnumerable() : cams.Values;

			foreach(var cam in collection) {
				if(viewLayer) cam.previewImage.transform.SetAsLastSibling();
				if(bitMask) cam.settings.ApplyLayerBitmask();
				if(worldCam) cam.ShowWorldCamIfNecessary();
				if(posRot) cam.settings.ApplyPositionAndRotation();
			}
		}

		public static Cam2 InitCamera(string name, bool loadConfig = true, bool reload = false) {
			if(cams.TryGetValue(name, out var cam)) {
				if(reload) {
					cam.settings.Reload();
					return cam;
				}

				throw new Exception("Already exists??");
			}

			cam = new GameObject($"Cam2_{name}").AddComponent<Cam2>();

			try {
				cam.Init(name, customScreen.AddNewView(), loadConfig);
			} catch {
				GameObject.DestroyImmediate(cam);
				throw;
			}

			cams[name] = cam;

			//Newly added cameras should always be the last child and thus on top
			//ApplyCameraValues(viewLayer: true);

			return cam;
		}

		public static Cam2 AddNewCamera(string namePrefix = "Unnamed Camera") {
			var nameToUse = namePrefix;
			var i = 2;

			while(cams.ContainsKey(nameToUse))
				nameToUse = $"{namePrefix}{i++}";

			return InitCamera(nameToUse, false);
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

			foreach(var x in ScenesManager.settings.scenes.Values)
				if(x.Contains(cam.name))
					x.RemoveAll(x => x == cam.name);

			foreach(var x in ScenesManager.settings.customScenes.Values)
				if(x.Contains(cam.name))
					x.RemoveAll(x => x == cam.name);

			ScenesManager.settings.Save();
		}

		public static bool RenameCamera(Cam2 cam, string newName) {
			if(cams.ContainsKey(newName))
				return false;

			if(!cams.ContainsValue(cam))
				return false;

			newName = string.Concat(newName.Split(Path.GetInvalidFileNameChars())).Trim();

			if(newName.Length == 0)
				return false;

			var oldName = cam.name;

			if(newName == oldName)
				return true;

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

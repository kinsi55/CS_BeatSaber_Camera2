using Camera2.HarmonyPatches;
using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using IPA;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Camera2 {
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		internal static Harmony harmony { get; private set; }

		internal static Material ShaderMat_LuminanceKey;
		internal static Material ShaderMat_Outline;
		internal static Material ShaderMat_CA;
		internal static Shader Shader_VolumetricBlit;

		[Init]
		/// <summary>
		/// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
		/// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
		/// Only use [Init] with one Constructor.
		/// </summary>
		public void Init(IPALogger logger) {
			Instance = this;
			Log = logger;

			Log.Info("Camera2 loaded");
			LoadShaders();
		}

		internal static void LoadShaders() {
			void LoadNormalShaders(AssetBundle bundle) {
				ShaderMat_LuminanceKey = new Material(bundle.LoadAsset<Shader>("luminancekey.shader"));
				ShaderMat_CA = new Material(bundle.LoadAsset<Shader>("chromaticaberration.shader"));
				bundle.Unload(false);
			}

			void LoadVRShaders(AssetBundle bundle) {
				Shader_VolumetricBlit = bundle.LoadAsset<Shader>("volumetricblit.shader");
				bundle.Unload(false);
			}

#if !DEV
			using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utils"))
				LoadNormalShaders(AssetBundle.LoadFromStream(stream));

			using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utilsvr"))
				LoadVRShaders(AssetBundle.LoadFromStream(stream));
#else
			LoadNormalShaders(AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\camera2utils"));
			LoadVRShaders(AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\camera2utilsvr"));
#endif
		}

		[OnStart]
		public void OnApplicationStart() {
			harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			MovementScriptManager.LoadMovementScripts();
			GlobalFPSCap.Init();

			SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchSelf();
		}
	}

#if DEBUG
	public static class lol {
		public static string GetPath(this Transform current) {
			if(current.parent == null)
				return "/" + current.name;
			return current.parent.GetPath() + "/" + current.name;
		}
	}
#endif
}

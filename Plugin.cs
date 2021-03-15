using System.Reflection;
using UnityEngine.SceneManagement;
using IPA;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using Camera2.Utils;
using Camera2.Managers;
using UnityEngine;
using System.IO;

namespace Camera2 {
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		internal static Harmony harmony { get; private set; }

#if DEV
		public static Shader PostShader;
		public static Material PostMaterial;
#endif

		[Init]
		/// <summary>
		/// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
		/// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
		/// Only use [Init] with one Constructor.
		/// </summary>
		public void Init(IPALogger logger) {
			Instance = this;
			Log = logger;

			Log.Info("Camera2 initialized.");

			harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

#if DEV
			//using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.PostProcessingShaders.chromaticaberration")) {
			//	var bundle = AssetBundle.LoadFromStream(stream);

			//	PostShader = bundle.LoadAsset<Shader>("chromaticaberration.shader");
			//	bundle.Unload(false);
			//}
			var bundle = AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\postprocessing");

			PostShader = bundle.LoadAsset<Shader>("MotionBlur.shader");

			PostMaterial = new Material(PostShader);
#endif
		}

		[OnStart]
		public void OnApplicationStart() {
			Log.Debug("OnApplicationStart");

			MovementScriptManager.LoadMovementScripts();

			SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;
		}

		[OnExit]
		public void OnApplicationQuit() {
			Log.Debug("OnApplicationQuit");
			harmony.UnpatchAll();

			ScenesManager.settings.Save();
			foreach(var cam in CamManager.cams)
				cam.Value.settings.Save();
		}
	}
}

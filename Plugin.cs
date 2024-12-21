using BeatSaberMarkupLanguage.Util;
using Camera2.Managers;
using Camera2.Middlewares;
using Camera2.Utils;
using HarmonyLib;
using IPA;
using System.Reflection;
using System.Threading.Tasks;
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
				ShaderMat_Outline = new Material(bundle.LoadAsset<Shader>("texouline.shader"));
				Shader_VolumetricBlit = bundle.LoadAsset<Shader>("volumetricblit.shader");
				bundle.Unload(false);
			}

#if !DEV
			using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utils"))
				LoadNormalShaders(AssetBundle.LoadFromStream(stream));
#else
			LoadNormalShaders(AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\camera2utils"));
#endif
		}

		[OnStart]
		public void OnApplicationStart() {
			harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			MovementScriptManager.LoadMovementScripts();
			//GlobalFPSCap.Init();

			SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;

			// Marinate the Reflection stuff off-thread so the loading of cameras later is less blocking
			Task.Run(() => {
				ModmapExtensions.Reflect();
				CustomNotesUtil.Reflect();
				if(ScoresaberUtil.Reflect())
					SDK.ReplaySources.Register(new ScoresaberUtil.SSReplaySource());
			});

			MainMenuAwaiter.MainMenuInitializing += delegate {
				UI.SpaghettiUI.Init();
			};
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

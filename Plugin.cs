using System.Reflection;
using UnityEngine.SceneManagement;
using IPA;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using Camera2.Utils;
using Camera2.Managers;

namespace Camera2 {

	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		internal static Harmony harmony { get; private set; }

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
		}


		#region BSIPA Config
		//Uncomment to use BSIPA's config
		/*
		[Init]
		public void InitWithConfig(Config conf)
		{
			Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
			Log.Debug("Config loaded");
		}
		*/
		#endregion

		[OnStart]
		public void OnApplicationStart() {
			Log.Debug("OnApplicationStart");

			ScoresaberUtil.Init();
			ModMapUtil.Init();
			MovementScriptManager.LoadMovementScripts();

			SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;
		}

		[OnExit]
		public void OnApplicationQuit() {
			Log.Debug("OnApplicationQuit");
			ScenesManager.settings.Save();
			foreach(var cam in CamManager.cams)
				cam.Value.settings.Save();
		}
	}
}

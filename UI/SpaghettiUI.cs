
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage;
using HMUI;
using BeatSaberMarkupLanguage.Components;
using Camera2.Managers;
using Camera2.Behaviours;
using BeatSaberMarkupLanguage.Components.Settings;
using Camera2.Configuration;
using System.ComponentModel;
using HarmonyLib;
using UnityEngine.UI;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Camera2.UI {
	// Theres a reason this is called Spaghetti UI, I will definitely maybe possibly make this not spaghetti one day.

	class SpaghettiUI {
		private static Coordinator _flow;

		public static void Init() {
			MenuButtons.instance.RegisterButton(new MenuButton("Camera2", "This is a Camera plugin 2 4Head", ShowFlow, true));
		}

		private static void ShowFlow() {
			if(_flow == null)
				_flow = BeatSaberUI.CreateFlowCoordinator<Coordinator>();
			BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_flow);
		}
	}

//	class PreviewView : BSMLResourceViewController {
//		public override string ResourceName => "Camera2.UI.Views.camPreview.bsml";
//#pragma warning disable 649
//		//[UIComponent("previewImage")] public UnityEngine.UI.RawImage image;
//#pragma warning restore 649

//		internal ImageView dd;

//		[UIAction("#post-parse")]
//		private void Parsed() {
//			var x = transform.Find("BSMLBackground");

//			//Destroy(x.GetComponentInChildren<UnityEngine.UI.RawImage>());

//			dd = x.gameObject.GetComponentInChildren<ImageView>();
//		}
//	}

	class SettingsView : BSMLResourceViewController, INotifyPropertyChanged {
		public override string ResourceName => "Camera2.UI.Views.camSettings.bsml";

		[UIComponent("zOffsetSlider")] SliderSetting zOffsetSlider = null;
		[UIComponent("previewSizeSlider")] SliderSetting previewSizeSlider = null;
		[UIComponent("modmapExt_moveWithMapCheckbox")] ToggleSetting modmapExt_moveWithMapSlider = null;
		[UIComponent("worldcamVisibilityInput")] LayoutElement worldcamVisibilityObj = null;
		[UIComponent("smoothfollowTab")] Tab smoothfollowTab = null;
		[UIComponent("follow360Tab")] Tab follow360Tab = null;
		[UIComponent("viewRectTab")] internal Tab viewRectTab = null;
		[UIComponent("tabSelector")] TabSelector tabSelector = null;

		internal static Cam2 cam { get; private set; }

		static List<string> props;

		void Awake() {
			// Dont really care which cam it is, this is just for BSML to init
			cam = CamManager.cams.Values.First();

			if(props == null) props = typeof(SettingsView).GetProperties(
				BindingFlags.Instance | BindingFlags.NonPublic
			).Select(x => x.Name).ToList();

			if(scenes == null)
				scenes = Enum.GetValues(typeof(SceneTypes)).Cast<SceneTypes>().Select(x => new SceneToggle() { type = x, host = this }).ToList();
		}
 #region variables
		internal string camName {
			get => cam.name;
			set {
				if(CamManager.RenameCamera(cam, value))
					Coordinator.instance.camList.list.tableView.ReloadData();
				NotifyPropertyChanged("camName");
			}
		}
		internal CameraType type {
			get => cam.settings.type;
			set {
				// When switching to FP reset Rot / Pos so that the previous TP values arent used as the FP offset
				if(value == CameraType.FirstPerson) {
					cam.settings.targetPos = new UnityEngine.Vector3(0, 0, zOffset);
					NotifyPropertyChanged("zOffset");
				} else if(value == CameraType.Positionable) {
					cam.settings.targetPos = new UnityEngine.Vector3(0, 1.5f, 1f);
				}
				cam.settings.targetRot = UnityEngine.Vector3.zero;
				cam.settings.type = value;
				ToggleSettingVisibility();
			}
		}
		internal WorldCamVisibility worldCamVisibility { get => cam.settings.worldCamVisibility; set { cam.settings.worldCamVisibility = value; } }
		internal float FOV { get => cam.settings.FOV; set { cam.settings.FOV = value; } }
		internal int fpsLimit { get => cam.settings.FPSLimiter.fpsLimit; set { cam.settings.FPSLimiter.fpsLimit = value; } }
		internal float renderScale { get => cam.settings.renderScale; set { cam.settings.renderScale = value; } }
		internal int antiAliasing { get => cam.settings.antiAliasing; set { cam.settings.antiAliasing = value; } }
		internal float previewSize { get => cam.settings.previewScreenSize; set { cam.settings.previewScreenSize = value; } }

		internal float zOffset {
			get {
				float ret = 0f;
				cam.settings.Unoverriden(delegate () {
					ret = cam.settings.targetPos.z;
				});
				return ret;
			}
			set {
				cam.settings.Unoverriden(delegate () {
					var x = cam.settings.targetPos;
					x.z = value;

					cam.settings.targetPos = x;
				});
				cam.settings.ApplyPositionAndRotation();
			}
		}

		internal WallVisiblity visibility_Walls {
			get => cam.settings.visibleObjects.Walls; set { cam.settings.visibleObjects.Walls = value; }
		}
		internal NoteVisibility visibility_Notes {
			get => cam.settings.visibleObjects.Notes; set { cam.settings.visibleObjects.Notes = value; }
		}
		internal bool visibility_Debris {
			get => cam.settings.visibleObjects.Debris; set { cam.settings.visibleObjects.Debris = value; }
		}
		internal bool visibility_UI {
			get => cam.settings.visibleObjects.UI; set { cam.settings.visibleObjects.UI = value; }
		}
		internal bool visibility_Avatar {
			get => cam.settings.visibleObjects.Avatar; set { cam.settings.visibleObjects.Avatar = value; }
		}
		internal bool visibility_Floor {
			get => cam.settings.visibleObjects.Floor; set { cam.settings.visibleObjects.Floor = value; }
		}
		internal bool visibility_CutParticles {
			get => cam.settings.visibleObjects.CutParticles; set { cam.settings.visibleObjects.CutParticles = value; }
		}

		internal bool smoothFollow_forceUpright {
			get => cam.settings.Smoothfollow.forceUpright; set { cam.settings.Smoothfollow.forceUpright = value; }
		}
		internal bool smoothFollow_followReplayPosition {
			get => cam.settings.Smoothfollow.followReplayPosition; set { cam.settings.Smoothfollow.followReplayPosition = value; }
		}
		internal float smoothFollow_position {
			get => cam.settings.Smoothfollow.position; set { cam.settings.Smoothfollow.position = value; }
		}
		internal float smoothFollow_rotation {
			get => cam.settings.Smoothfollow.rotation; set { cam.settings.Smoothfollow.rotation = value; }
		}

		internal bool modmapExt_moveWithMap {
			get => cam.settings.ModmapExtensions.moveWithMap; set { cam.settings.ModmapExtensions.moveWithMap = value; }
		}
		internal bool modmapExt_autoOpaqueWalls {
			get => cam.settings.ModmapExtensions.autoOpaqueWalls; set { cam.settings.ModmapExtensions.autoOpaqueWalls = value; }
		}
		internal bool modmapExt_autoHideHUD {
			get => cam.settings.ModmapExtensions.autoHideHUD; set { cam.settings.ModmapExtensions.autoHideHUD = value; }
		}

		internal bool follow360_moveWithMap {
			get => cam.settings.Follow360.enabled; set { cam.settings.Follow360.enabled = value; }
		}
		internal float follow360_smoothing {
			get => cam.settings.Follow360.smoothing; set { cam.settings.Follow360.smoothing = value; }
		}

		internal int viewRect_x {
			get => (int)cam.settings.viewRect.x; set { var n = cam.settings.viewRect; n.x = value; cam.settings.viewRect = n; }
		}
		internal int viewRect_y {
			get => (int)cam.settings.viewRect.y; set { var n = cam.settings.viewRect; n.y = value; cam.settings.viewRect = n; }
		}
		internal int viewRect_width {
			get => (int)cam.settings.viewRect.width; set { var n = cam.settings.viewRect; n.width = value; cam.settings.viewRect = n; }
		}
		internal int viewRect_height {
			get => (int)cam.settings.viewRect.height; set { var n = cam.settings.viewRect; n.height = value; cam.settings.viewRect = n; }
		}
		#endregion


		private static readonly List<object> types = new object[] { CameraType.FirstPerson, CameraType.Positionable }.ToList();
		private static readonly List<object> antiAliasingLevels = new object[] { 1, 2, 4, 8 }.ToList();
		private static readonly List<object> worldCamVisibilities = Enum.GetValues(typeof(WorldCamVisibility)).Cast<object>().ToList();
		private static readonly List<object> visibilities_Walls = Enum.GetValues(typeof(WallVisiblity)).Cast<object>().ToList();
		private static readonly List<object> visibilities_Notes = Enum.GetValues(typeof(NoteVisibility)).Cast<object>().ToList();
		private List<SceneToggle> scenes;

		class SceneToggle : NotifiableSettingsObj {
			internal SettingsView host;
			internal SceneTypes type;

			internal bool val {
				get => ScenesManager.settings.scenes[type].Contains(host.camName);
				set {
					var x = ScenesManager.settings.scenes[type];

					if(!value) {
						x.RemoveAll(c => c == host.camName);
					} else if(!x.Contains(host.camName)) {
						x.Add(host.camName);
					}
				}
			}
		}


		internal bool SetCam(Cam2 newCam) {
			if(cam == newCam)
				return false;

			SaveSettings();
			cam = newCam;

			ScenesManager.LoadGameScene(forceReload: true);

			//if(Coordinator.instance.previewView?.dd != null) {
			//	Coordinator.instance.previewView.dd.material = cam?.worldCam.viewMaterial;
			//	Coordinator.instance.previewView.dd.SetMaterialDirty();
			//}


			CamManager.ApplyCameraValues(worldCam: true);

			if(cam == null)
				return true;

			cam.gameObject.SetActive(true);

			ToggleSettingVisibility();

			foreach(var prop in props)
				NotifyPropertyChanged(prop);

			foreach(var x in scenes)
				x.NotifyPropertyChanged("val");

			return true;
		}

		internal void SaveSettings() {
			cam?.settings.Save();
			ScenesManager.settings.Save();
		}
		
		[UIAction("#post-parse")]
		private void Parsed() {
			cam = null;

			// Since BSML doesnt make all the inputs have the same width I need to to it myself for my own sanity
			foreach(var x in GetComponentsInChildren<StringSetting>(true)) {
				var picker = x.transform.Find("ValuePicker/DecButton");
				picker.gameObject.SetActive(false);
				picker.parent.localScale = new UnityEngine.Vector3(1.06f, 1f, 1f);
			}
			foreach(var x in GetComponentsInChildren<DropDownListSetting>(true))
				x.transform.localScale = new UnityEngine.Vector3(1.09f, 1f, 1f);

			viewRectTab.IsVisible = false;

			Coordinator.instance.ShowSettingsForCam(CamManager.cams.Values.First());
		}

		private void ToggleSettingVisibility() {
			if(zOffsetSlider == null) return;

			zOffsetSlider.gameObject.SetActive(type == CameraType.FirstPerson);
			previewSizeSlider.gameObject.SetActive(type == CameraType.Positionable);
			modmapExt_moveWithMapSlider.gameObject.SetActive(type == CameraType.Positionable);
			worldcamVisibilityObj.gameObject.SetActive(type == CameraType.Positionable);
			smoothfollowTab.IsVisible = type == CameraType.FirstPerson;
			follow360Tab.IsVisible = type == CameraType.Positionable;

			// Apparently this is the best possible way to programmatically switch the selected tab
			tabSelector.textSegmentedControl.SelectCellWithNumber(0);
			AccessTools.Method(typeof(TabSelector), "TabSelected").Invoke(tabSelector, new object[] { tabSelector.textSegmentedControl, 0 });
		}
	}

	class CamList : BSMLResourceViewController {
		public override string ResourceName => "Camera2.UI.Views.camList.bsml";

		private readonly string cam2Version = $"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} by Kinsi55";

		[UIComponent("deleteButton")] public NoTransitionsButton deleteButton = null;
		[UIComponent("camList")] public CustomCellListTableData list = null;
		public List<CamListCellWrapper> listData = new List<CamListCellWrapper>();
		public IEnumerable<CamListCellWrapper> listDataOrdered => listData.AsEnumerable().OrderByDescending(x => x.cam.settings.layer);
		List<object> cams => listDataOrdered.Cast<object>().ToList();

		public class CamListCellWrapper {
			public Cam2 cam { get; private set; }

			public CamListCellWrapper(Cam2 cam) => this.cam = cam;

			public string name => cam.name;

			int sceneCount => ScenesManager.settings.scenes.Values.Count(x => x.Contains(name)) + ScenesManager.settings.customScenes.Values.Count(x => x.Contains(name));

			string details => $"{cam.settings.type}, assigned to {sceneCount} {(sceneCount == 1 ? "Scene" : "Scenes")}";
			string layerUIText => $"Layer {cam.settings.layer}{(CamManager.cams.Values.Count(x => x.settings.layer == cam.settings.layer) > 1 ? " <color=#d5a145>⚠</color>" : "")}";

			[UIComponent("bgContainer")] ImageView bg = null;

			[UIAction("refresh-visuals")]
			public void Refresh(bool selected, bool highlighted) {
				var x = new UnityEngine.Color(0, 0, 0, 0.45f);

				if(selected || highlighted)
					x.a = selected ? 0.9f : 0.6f;

				bg.color = x;
			}
		}

		internal void UpdateCamListUI() {
			//var x = Sprite.Create(cam.screenImage.material, new Rect(0, 0, cam.renderTexture.width, cam.renderTexture.width), new Vector2(0.5f, 0.5f));
			list.data = cams;
			list.tableView.ReloadData();
			deleteButton.interactable = listData.Count > 1;
		}

		[UIAction("#post-parse")]
		internal void Init() {
			listData.Clear();

			listData.AddRange(CamManager.cams.Values.Select(x => new CamListCellWrapper(x)));
			UpdateCamListUI();
		}

		[UIAction("SelectCamera")]
		void SelectCamera(TableView tableView, CamListCellWrapper row) => Coordinator.instance.ShowSettingsForCam(row.cam);

		Cam2 GetCam(string name) {
			var cam = CamManager.AddNewCamera(name);
			cam.settings.viewRect = new UnityEngine.Rect(UnityEngine.Random.Range(0, 200), UnityEngine.Random.Range(0, 200), UnityEngine.Screen.width / 3, UnityEngine.Screen.height / 3);

			return cam;
		}

		void AddCam(Cam2 cam) {
			cam.settings.ApplyPositionAndRotation();
			cam.settings.ApplyLayerBitmask();
			cam.UpdateRenderTextureAndView();

			listData.Insert(0, new CamListCellWrapper(cam));
			UpdateCamListUI();
			Coordinator.instance.ShowSettingsForCam(cam);
		}

		void AddCamDefault() => AddCam(GetCam("Unnamed Camera"));

		void AddCamSideview() {
			var cam = CamManager.AddNewCamera("Side View");

			cam.settings.type = CameraType.Positionable;
			cam.settings.FOV = 75;
			cam.settings.viewRect = new UnityEngine.Rect(0, 0, UnityEngine.Screen.width * .195f, UnityEngine.Screen.height * .395f);
			cam.settings.targetPos = new UnityEngine.Vector3(-3, 1.2f, 0);
			cam.settings.targetRot = new UnityEngine.Vector3(0, 90f, 0);
			cam.settings.visibleObjects.Walls = WallVisiblity.Hidden;
			cam.settings.visibleObjects.Debris = false;
			cam.settings.visibleObjects.UI = false;
			cam.settings.visibleObjects.Floor = false;
			cam.settings.visibleObjects.CutParticles = false;

			AddCam(cam);
		}

		void AddCamThirdperson() {
			var cam = GetCam("Static Thirdperson");

			cam.settings.type = CameraType.Positionable;
			cam.settings.FOV = 75;
			cam.settings.targetPos = new UnityEngine.Vector3(0.62f, 0.56f, -1.81f);
			cam.settings.targetRot = new UnityEngine.Vector3(348.1f, 348.1f, 1.1f);

			AddCam(cam);
		}


		void DeleteCam() {
			listData.Remove(listData.Find(x => x.cam == SettingsView.cam));
			CamManager.DeleteCamera(SettingsView.cam);
			UpdateCamListUI();
			Coordinator.instance.ShowSettingsForCam(listDataOrdered.First().cam);
		}

		void ChangeLayer(int diff) {
			SettingsView.cam.settings.layer += diff;
			Coordinator.instance.camList.UpdateCamListUI();
			Coordinator.instance.ShowSettingsForCam(SettingsView.cam, true);
		}

		void LayerIncrease() => ChangeLayer(1);
		void LayerDecrease() => ChangeLayer(-1);

		void UnlockCamPosTab() => Coordinator.instance.settingsView.viewRectTab.IsVisible = true;

		void ShowGithub() => Process.Start("https://github.com/kinsi55/CS_BeatSaber_Camera2");
		void ShowWiki() => Process.Start("https://github.com/kinsi55/CS_BeatSaber_Camera2/wiki");
	}

	class Coordinator : FlowCoordinator {
		internal static Coordinator instance { get; private set; }

		internal SettingsView settingsView;
		internal CamList camList;

		public void Awake() {
			instance = this;

			if(camList == null)
				camList = BeatSaberUI.CreateViewController<CamList>();

			if(settingsView == null)
				settingsView = BeatSaberUI.CreateViewController<SettingsView>();

			//if(previewView == null)
			//	previewView = BeatSaberUI.CreateViewController<PreviewView>();
		}

		public void ShowSettingsForCam(Cam2 cam, bool reselect = false) {
			SetTitle($"Camera2 | {cam.name}");

			if(!settingsView.SetCam(cam) && !reselect)
				return;

			var cellIndex = camList.listDataOrdered.ToList().FindIndex(x => x.cam == cam);

			camList.list.tableView.SelectCellWithIdx(cellIndex);
			// This is literally the only thing making the Cam2 incompatible between 1.13.2 and 1.13.4, so it works like this now.
			AccessTools.Method(typeof(TableView), nameof(TableView.ScrollToCellWithIdx)).Invoke(camList.list.tableView, new object[] { cellIndex, 1, false });
		}

		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
			try {
				if(!firstActivation) {
					camList?.Init();
					ShowSettingsForCam(CamManager.cams.Values.First());
					return;
				}
				
				showBackButton = true;

				ProvideInitialViewControllers(settingsView, camList);
			} catch(Exception ex) {
				Plugin.Log.Error(ex);
			}
		}

		protected override void BackButtonWasPressed(ViewController topViewController) {
			settingsView.SetCam(null);
			ScenesManager.LoadGameScene(forceReload: true);
			BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal);
		}
	}

	class NotifiableSettingsObj : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		internal void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
			try {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			} catch(Exception ex) {
				Plugin.Log?.Error($"Error Invoking PropertyChanged: {ex.Message}");
				Plugin.Log?.Error(ex);
			}
		}

		internal void NotifyPropertiesChanged() {
			foreach(var x in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
				NotifyPropertyChanged(x.Name);
		}
	}
}

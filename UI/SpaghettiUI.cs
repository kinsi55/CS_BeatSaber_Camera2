
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

namespace Camera2.Settings {
	class SpaghettiUI {
		private static Coordinator _flow;

		public static void Init() {
			MenuButtons.instance.RegisterButton(new MenuButton("Camera2", "This is a Camera plugin 2 4Head", ShowFlow, true));
		}

		private static void ShowFlow() {
			if(_flow == null)
				_flow = BeatSaberUI.CreateFlowCoordinator<Coordinator>();
			BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinatorOrAskForTutorial(_flow);
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

	public class SettingsView : BSMLResourceViewController, INotifyPropertyChanged {
		public override string ResourceName => "Camera2.UI.Views.camSettings.bsml";
#pragma warning disable 649
		[UIComponent("zOffsetSlider")] SliderSetting zOffsetSlider;
		[UIComponent("previewSizeSlider")] SliderSetting previewSizeSlider;
		[UIComponent("modmapExt_moveWithMapCheckbox")] ToggleSetting modmapExt_moveWithMapSlider;
		[UIComponent("worldcamVisibilityInput")] LayoutElement worldcamVisibilityObj;
		[UIComponent("smoothfollowTab")] Tab smoothfollowTab;
		[UIComponent("follow360Tab")] Tab follow360Tab;
		[UIComponent("tabSelector")] TabSelector tabSelector;
#pragma warning restore 649

		internal static Cam2 cam { get; private set; }

		static List<string> props;

		void Awake() {
			// Dont really care which cam it is, this is just for BSML to init
			cam = CamManager.cams.Values.First();

			if(props == null) props = typeof(SettingsView).GetProperties(
				BindingFlags.Instance | BindingFlags.NonPublic
			).Select(x => x.Name).ToList();
		}
#region variables
		internal string camName {
			get => cam.name;
			set {
				if(CamManager.RenameCamera(cam, value)) {
					Coordinator.instance.currentTableCell.text = camName;
					Coordinator.instance.camList.list.tableView.ReloadData();
				}
				NotifyPropertyChanged("camName");
			}
		}
		internal CameraType type {
			get => cam.settings.type;
			set {
				cam.settings.type = value;
				// When switching to FP reset Rot / Pos so that the previous TP values arent used as the FP offset
				if(value == CameraType.FirstPerson) {
					cam.settings.targetRot = UnityEngine.Vector3.zero;
					cam.settings.targetPos = new UnityEngine.Vector3(0, 0, zOffset);
					cam.settings.ApplyPositionAndRotation();
					NotifyPropertyChanged("zOffset");
				}
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
			get => cam.settings.targetPos.z; set { cam.settings.targetPos.z = value; cam.settings.ApplyPositionAndRotation(); }
		}

		internal WallVisiblity visibility_Walls {
			get => cam.settings.visibleObjects.Walls; set { cam.settings.visibleObjects.Walls = value; }
		}
		internal NoteVisibility visibility_Notes {
			get => cam.settings.visibleObjects.Notes; set { cam.settings.visibleObjects.Notes = value; }
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
			get => cam.settings.Follow360.enabled;
			set { cam.settings.Follow360.enabled = value; }
		}
		internal float follow360_smoothing {
			get => cam.settings.Follow360.smoothing;
			set { cam.settings.Follow360.smoothing = value; }
		}
		#endregion



		private static readonly List<object> types = new object[] { CameraType.FirstPerson, CameraType.Positionable }.ToList();
		private static readonly List<object> antiAliasingLevels = new object[] { 1, 2, 4, 8 }.ToList();
		private static readonly List<object> worldCamVisibilities = Enum.GetValues(typeof(WorldCamVisibility)).Cast<object>().ToList();
		private static readonly List<object> visibilities_Walls = Enum.GetValues(typeof(WallVisiblity)).Cast<object>().ToList();
		private static readonly List<object> visibilities_Notes = Enum.GetValues(typeof(NoteVisibility)).Cast<object>().ToList();
		private static readonly List<object> scenes = Enum.GetValues(typeof(SceneTypes)).Cast<object>().Select(x => new { name = x }).Cast<object>().ToList();

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

			if(cam == null)
				return true;

			cam.gameObject.SetActive(true);
			cam.ShowWorldCamIfNecessary();

			ToggleSettingVisibility();

			foreach(var prop in props)
				NotifyPropertyChanged(prop);

			return true;
		}

		internal void SaveSettings() {
			cam?.settings.Save();
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

	public class CamList : BSMLResourceViewController {
		public override string ResourceName => "Camera2.UI.Views.camList.bsml";

		[UIComponent("deleteButton")] public NoTransitionsButton deleteButton;
		[UIComponent("camList")] public CustomListTableData list;

		internal void AddCamToList(Cam2 cam) {
			//var x = Sprite.Create(cam.screenImage.material, new Rect(0, 0, cam.renderTexture.width, cam.renderTexture.width), new Vector2(0.5f, 0.5f));
			list.data.Add(new CustomListTableData.CustomCellInfo(
				cam.name, 
				$"{cam.settings.type.ToString()}, Layer {cam.settings.layer}"
			));

			list.tableView.ReloadData();
			deleteButton.interactable = list.data.Count > 1;
		}

		[UIAction("#post-parse")]
		internal void Init() {
			list.data.Clear();

			foreach(var cam in CamManager.cams.Values)
				AddCamToList(cam);

			list.tableView.ReloadData();
		}

		[UIAction("SelectCamera")]
		void SelectCamera(TableView tableView, int row) {
			Coordinator.instance.ShowSettingsForCam(CamManager.cams[list.data[row].text]);
		}

		[UIAction("AddCam")]
		void AddCam() {
			var cam = CamManager.AddNewCamera();

			AddCamToList(cam);
			Coordinator.instance.ShowSettingsForCam(cam);
		}

		[UIAction("DeleteCam")]
		void DeleteCam() {
			list.data.RemoveAll(x => x.text == SettingsView.cam.name);
			CamManager.DeleteCamera(SettingsView.cam);
			list.tableView.ReloadData();
			Coordinator.instance.ShowSettingsForCam(CamManager.cams.Values.First());
			deleteButton.interactable = list.data.Count > 1;
		}
	}

	class Coordinator : FlowCoordinator {
		internal static Coordinator instance { get; private set; }

		internal SettingsView settingsView;
		internal CamList camList;
		internal CustomListTableData.CustomCellInfo currentTableCell;

		public void Awake() {
			instance = this;

			if(camList == null)
				camList = BeatSaberUI.CreateViewController<CamList>();

			if(settingsView == null)
				settingsView = BeatSaberUI.CreateViewController<SettingsView>();

			//if(previewView == null)
			//	previewView = BeatSaberUI.CreateViewController<PreviewView>();
		}

		public void ShowSettingsForCam(Cam2 cam) {
			SetTitle($"Camera2 | {cam.name}");

			if(!settingsView.SetCam(cam))
				return;

			var cellIndex = camList.list.data.FindIndex(el => el.text == cam.name);
			currentTableCell = camList.list.data[cellIndex];

			camList.list.tableView.ScrollToCellWithIdx(cellIndex, TableViewScroller.ScrollPositionType.Center, false);
			camList.list.tableView.SelectCellWithIdx(cellIndex);
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
}

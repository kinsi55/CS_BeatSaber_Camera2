using Camera2.HarmonyPatches;
using Camera2.Managers;
using Camera2.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Camera2.Behaviours {

#if DEBUG
	public
#endif
	class CameraDesktopView : RawImage {
		public Cam2 cam { get; private set; }
		public RectTransform rekt { get; private set; }

		const float minSizePct = 0.05f;

		public void SetPositionClamped(Vector2 delta, int[] matrix, bool writeToConfig = false) {
			/*
			 * If you can simplify this I happily invite you to do so, this took me way too long lmao
			 * This is probably possible in half the LOC but i already spent way too much time on this
			 */
			var vrc = cam.settings.viewRect;
			var iMin = vrc.MinAnchor();
			var iMax = vrc.MaxAnchor();

			// Theoretical transform, ignoring bounds
			var oMin = new Vector2(iMin.x + (delta.x * matrix[0]), iMin.y + (delta.y * matrix[1]));
			var oMax = new Vector2(iMax.x + (delta.x * matrix[2]), iMax.y + (delta.y * matrix[3]));

			// Constraining min/max to stay within bounds and the size to stay the same
			var oMinConstrained = new Vector2(
				Mathf.Clamp(oMin.x, 0, 1 - (vrc.width * matrix[2])),
				Mathf.Clamp(oMin.y, 0, 1 - (vrc.height * matrix[3]))
			);
			var oMaxConstrained = new Vector2(
				Mathf.Clamp(oMax.x, vrc.width * matrix[0], 1),
				Mathf.Clamp(oMax.y, vrc.height * matrix[1], 1)
			);

			var clampW = matrix[0] | matrix[2];
			var clampH = matrix[1] | matrix[3];

			// Clamp output size to be at least N while staying in bounds
			var oMinClamped = new Vector2(
				matrix[0] == 0 ? iMin.x : Mathf.Clamp(oMinConstrained.x, 0, (clampW * oMaxConstrained.x) - minSizePct),
				matrix[1] == 0 ? iMin.y : Mathf.Clamp(oMinConstrained.y, 0, (clampH * oMaxConstrained.y) - minSizePct)
			);

			var oMaxClamped = new Vector2(
				matrix[2] == 0 ? iMax.x : Mathf.Clamp(oMaxConstrained.x, minSizePct + (clampW * oMinConstrained.x), 1),
				matrix[3] == 0 ? iMax.y : Mathf.Clamp(oMaxConstrained.y, minSizePct + (clampH * oMinConstrained.y), 1)
			);

			rekt.anchorMin = oMinClamped;
			rekt.anchorMax = oMaxClamped;

			if(writeToConfig && delta != Vector2.zero) {
				cam.settings.SetViewRect(oMinClamped.x, oMinClamped.y, oMaxClamped.x - oMinClamped.x, oMaxClamped.y - oMinClamped.y);
				cam.settings.Save();
			}
		}

		new public void Awake() {
			rekt = (transform as RectTransform);
			rekt.pivot = rekt.sizeDelta = new Vector2(0, 0);
		}

		public void SetSource(Cam2 cam) {
			this.cam = cam;

			texture = cam.renderTexture;

			rekt.anchorMin = cam.settings.viewRect.MinAnchor();
			rekt.anchorMax = cam.settings.viewRect.MaxAnchor();

			rekt.anchoredPosition = new Vector2(0, 0);
			gameObject.name = cam.name;
		}
	}

	class CamerasViewport : MonoBehaviour {
		private static Canvas canvas;

		public void Awake() {
			DontDestroyOnLoad(gameObject);

			canvas = gameObject.AddComponent<Canvas>();
			// I know this logs a stupid warning because VR is active, no way to fix that it seems.
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		public CameraDesktopView AddNewView() {
			var img = new GameObject().AddComponent<CameraDesktopView>();

			img.transform.SetParent(gameObject.transform, true); //.parent = gameObject.transform;

			return img;
		}

		enum CamAction {
			None,
			Menu,
			Move,
			Resize_BR,
			Resize_TL,
			Resize_TR,
			Resize_BL,
		}

		static int[][] deltaSchemes = new int[][] {
			new int[] { 1, 1, 1, 1 }, // Drag
			new int[] { 0, 1, 1, 0 }, // Resize from bottom right
			new int[] { 1, 0, 0, 1 }, // Resize from top left
			new int[] { 0, 0, 1, 1 }, // Resize from top right
			new int[] { 1, 1, 0, 0 } // Resize from bottom left
		};

		const float grabbersize = 25;

		CameraDesktopView GetViewAtPoint(Vector2 point, ref CamAction actionAtPoint) {
			// This should already be sorted in the correct order
			foreach(var camScreen in GetComponentsInChildren<CameraDesktopView>(false).Reverse()) {
				var d = new Rect(camScreen.rekt.position, camScreen.rekt.rect.size);

				if(d.Contains(point) && (!camScreen.cam.settings.isScreenLocked || UI.SettingsView.cam == camScreen.cam)) {
					var relativeCursorPos = point - d.position;

					if(relativeCursorPos.y <= grabbersize && relativeCursorPos.x >= d.width - grabbersize) {
						actionAtPoint = CamAction.Resize_BR;
					} else if(relativeCursorPos.y >= d.height - grabbersize && relativeCursorPos.x >= d.width - grabbersize) {
						actionAtPoint = CamAction.Resize_TR;
					} else if(relativeCursorPos.y >= d.height - grabbersize && relativeCursorPos.x <= grabbersize) {
						actionAtPoint = CamAction.Resize_TL;
					} else if(relativeCursorPos.y <= grabbersize && relativeCursorPos.x <= grabbersize) {
						actionAtPoint = CamAction.Resize_BL;
					} else {
						actionAtPoint = CamAction.Move;
					}

					return camScreen;
				}
			}

			actionAtPoint = CamAction.None;

			return null;
		}



		private Vector2 mouseStartPos01;
		private Vector2 lastScreenRes = Vector2.zero;
		public static CameraDesktopView targetCam { get; private set; }
		private CamAction possibleAction = CamAction.None;
		private CamAction currentAction = CamAction.None;

		private Vector3 lastMousePos;

		private bool didShowHint = false;

		void Update() {
			if(Input.anyKeyDown) { //Some custom scenes to do funny stuff with
				if(Input.GetKeyDown(KeyCode.F1)) {
					if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift)) {
						Plugin.Log.Info("Reloading Camera2 Config...");
						MovementScriptManager.LoadMovementScripts(true);
						CamManager.Reload();
					} else {
						ScenesManager.LoadGameScene();
					}
				} else {
#if DEV
					if(Input.GetKeyDown(KeyCode.F7)) {
						Plugin.LoadShaders();
						return;
					}
#endif

					foreach(var k in ScenesManager.settings.customSceneBindings) {
						if(!Input.GetKeyDown(k.Key))
							continue;

						ScenesManager.SwitchToCustomScene(k.Value);
						break;
					}
				}
			}

			// This doesnt really belong here.....
			var curRes = new Vector2(Screen.width, Screen.height);
			if(lastScreenRes != Vector2.zero) {
				foreach(var c in CamManager.cams)
					c.Value.UpdateRenderTextureAndView();
			}

			lastScreenRes = curRes;

			if(HookFPFCToggle.isInFPFC) {
				if((int)currentAction >= 2)
					ProcessCamAction(true);

				targetCam = null;
				return;
			}

			if(currentAction == CamAction.None && lastMousePos != Input.mousePosition) {
				if(!Application.isFocused)
					return;

				possibleAction = CamAction.None;
				lastMousePos = Input.mousePosition;

				if(lastMousePos.x < 0 || lastMousePos.y < 0 || lastMousePos.x > Screen.width || lastMousePos.y > Screen.height)
					return;

				var pCam = targetCam;
				targetCam = GetViewAtPoint(lastMousePos, ref possibleAction);

				if(targetCam != pCam) {
					if(targetCam)
						targetCam.cam.PrepareMiddlewaredRender(true);

					if(pCam)
						pCam.cam.PrepareMiddlewaredRender(true);
				}

				if(possibleAction == CamAction.Resize_BR || possibleAction == CamAction.Resize_TL) {
					WinAPI.SetCursor(WinAPI.WindowsCursor.IDC_SIZENWSE);
				} else if(possibleAction == CamAction.Resize_BL || possibleAction == CamAction.Resize_TR) {
					WinAPI.SetCursor(WinAPI.WindowsCursor.IDC_SIZENESW);
				}
			}

			if(Input.GetMouseButtonUp(1) && currentAction == CamAction.None) {
				if(!didShowHint && (didShowHint = true))
					System.Threading.Tasks.Task.Run(() => WinAPI.MessageBox(IntPtr.Zero, "There is no desktop settings for Camera2, everything is done ingame!\n\nYou can drag around a cameras display with your mouse and resize it on the corners corner from the desktop.", "FYI", 0));
				//currentAction = CamAction.Menu;
				// For now lets not add this as it can result in unclear circumstances with scenes etc.
				//if(Input.GetKey(KeyCode.LeftControl)) {
				//	var newCam = CamManager.AddNewCamera();

				//	newCam.settings.Save();
				//}
			}

			void ProcessCamAction(bool finished) {
				var x = Input.mousePosition / new Vector2(Screen.width, Screen.height);

				if((int)currentAction >= 2) {
					targetCam.SetPositionClamped(
						// We take the current configured position and set the view position to it + the cursor move delta
						x - mouseStartPos01,

						deltaSchemes[(int)currentAction - 2],
						// And only when the button was released, save it to the config to make it the new config value
						finished
					);
				}

				GL.Clear(true, true, Color.black);
				if(finished)
					currentAction = CamAction.None;
			}

			if(possibleAction != CamAction.None) {
				// Drag handler / Resize
				if(Input.GetMouseButtonDown(0) && targetCam && currentAction == CamAction.None) {
					mouseStartPos01 = lastMousePos / new Vector2(Screen.width, Screen.height);
					currentAction = possibleAction;
				}

				if(currentAction == CamAction.None)
					return;

				bool released = !Input.GetMouseButton(0) || !targetCam.isActiveAndEnabled;

				ProcessCamAction(released);
			}
		}
	}
}

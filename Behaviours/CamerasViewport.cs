using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Camera2.Configuration;
using Camera2.Utils;

namespace Camera2.Behaviours {
	class LessRawImage : RawImage {
		public Cam2 cam { get; private set; }
		public RectTransform rekt { get; private set; }

		public Vector2 position { get { return rekt.position; } set { rekt.position = value; } }
		public Vector2 size { get { return rekt.sizeDelta; } set { rekt.sizeDelta = value; } }

		public void SetPositionClamped(Vector2 pos, bool writeToConfig = false) {
			pos.x = Mathf.Clamp(pos.x, 0, Screen.width - rekt.rect.width);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height - rekt.rect.height);

			position = pos;
			if(!writeToConfig)
				return;

			var rect = cam.settings.viewRect;
			rect.position = pos;
			cam.settings.viewRect = rect;
		}

		const int MIN_SIZE = 50;

		public void ModifySizeClamped(Vector2 diff, bool writeToConfig = false) {
			/*
			 * Dont even try to understand this, I wont bother to comment either, 
			 * I have no idea what is going on and kinda brute forced this solution
			 * Feel free to make this simpler tho if you want, I hate every single line of this
			 * Why cant Unity just have an option to make the top left of a canvas 0;0 instead of bottom left
			 */
			var sizex = Mathf.Clamp(diff.x + cam.settings.viewRect.width, MIN_SIZE, Screen.width - position.x);
			var sizey = Mathf.Clamp(cam.settings.viewRect.height - diff.y, MIN_SIZE, cam.settings.viewRect.y + cam.settings.viewRect.height);

			position = new Vector2(
				position.x,
				Mathf.Clamp(cam.settings.viewRect.y + diff.y, 0, cam.settings.viewRect.y + cam.settings.viewRect.height - MIN_SIZE)
			);

			size = new Vector2(sizex, sizey);
			
			if(!writeToConfig)
				return;

			var rect = cam.settings.viewRect;
			rect.position = position;
			rect.size = size;
			cam.settings.viewRect = rect;
		}

		new public void Awake() {
			rekt = gameObject.GetComponent<RectTransform>();
			rekt.pivot = new Vector2(0, 0);

			// Idea: Outline cameras when hovered, doesnt work, cba.
			//var x = gameObject.AddComponent<Outline>();
			//x.effectColor = Color.red;
			//x.effectDistance = new Vector2(50, 50);
			//x.enabled = true;
		}

		public void SetSource(Cam2 cam) {
			this.cam = cam;
			
			//if(material != null) Destroy(material);
			// Cannot reuse the material, else the preview will just stay black.
			material = new Material(CamerasViewport.blitCopyShader);
			material.SetTexture("_MainTex", cam.renderTexture);
			rekt.sizeDelta = cam.settings.viewRect.size;
			position = cam.settings.viewRect.position;
			gameObject.name = cam.name;
		}
		
	}

	class CamerasViewport : MonoBehaviour {
		private static Canvas canvas;
		internal static Shader blitCopyShader = Shader.Find("Hidden/BlitCopy");

		public void Awake() {
			DontDestroyOnLoad(this);

			canvas = gameObject.AddComponent<Canvas>();
			// I know this throws a stupid warning because VR is active, no way to fix that it seems.
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		public LessRawImage AddNewView() {
			var img = new GameObject().AddComponent<LessRawImage>();

			img.transform.SetParent(gameObject.transform, true); //.parent = gameObject.transform;

			return img;
		}

		enum CamAction {
			None,
			Move,
			Menu,
			Resize_BR // Cba to implement scaling on other corners atm
		}

		const int grabbersize = 20;

		LessRawImage GetViewAtPoint(Vector2 point, ref CamAction actionAtPoint) {
			// This should already be sorted in the correct order
			foreach(var cam in GetComponentsInChildren<LessRawImage>(false).Reverse()) {
				var d = new Rect(cam.rekt.position, cam.rekt.sizeDelta);

				if(d.Contains(point)) {
					var relativeCursorPos = point - d.position;

					if(relativeCursorPos.y <= grabbersize && relativeCursorPos.x >= d.width - grabbersize) {
						actionAtPoint = CamAction.Resize_BR;
					} else {
						actionAtPoint = CamAction.Move;
					}

					return cam;
				}
			}

			actionAtPoint = CamAction.None;

			return null;
		}
		

		private Vector2 mouseStartPos;
		private LessRawImage targetCam;
		private CamAction possibleAction = CamAction.None;
		private CamAction currentAction = CamAction.None;

		private Vector3 lastMousePos;

		void Update() {
			if(Input.anyKeyDown) { //Some custom scenes to do funny stuff with
				if(Input.GetKeyDown(KeyCode.F1)) {
					ScenesManager.LoadGameScene(SceneUtil.currentScene.name);
				} else if(Input.GetKeyDown(KeyCode.F2)) {
					ScenesManager.SwitchToScene(SceneTypes.Custom1);
				} else if(Input.GetKeyDown(KeyCode.F3)) {
					ScenesManager.SwitchToScene(SceneTypes.Custom2);
				} else if(Input.GetKeyDown(KeyCode.F4)) {
					ScenesManager.SwitchToScene(SceneTypes.Custom3);
				}
			}

			if(currentAction == CamAction.None && lastMousePos != Input.mousePosition) {
				possibleAction = CamAction.None;
				lastMousePos = Input.mousePosition;

				if(lastMousePos.x < 0 || lastMousePos.y < 0 || lastMousePos.x > Screen.width || lastMousePos.y > Screen.height)
					return;

				targetCam = GetViewAtPoint(lastMousePos, ref possibleAction);

				CursorUtil.SetCursor(possibleAction == CamAction.Resize_BR ? CursorUtil.WindowsCursor.IDC_SIZENWSE : CursorUtil.WindowsCursor.IDC_ARROW);
			}

			if(Input.GetMouseButtonUp(1) && currentAction == CamAction.None) {
				//currentAction = CamAction.Menu;

			}

			if(possibleAction != CamAction.None) {
				// Drag handler / Resize
				if(Input.GetMouseButtonDown(0) && targetCam != null && currentAction == CamAction.None) {
					mouseStartPos = lastMousePos;
					currentAction = possibleAction;
				}

				if(currentAction == CamAction.None)
					return;

				bool released = !Input.GetMouseButton(0);

				if(currentAction == CamAction.Move) {
					targetCam.SetPositionClamped(
						// We take the current configured position and set the view position to it + the cursor move delta
						targetCam.cam.settings.viewRect.position + (Vector2)Input.mousePosition - mouseStartPos,
						// And only when the button was released, save it to the config to make it the new config value
						released
					);
				} else if(currentAction == CamAction.Resize_BR) {
					targetCam.ModifySizeClamped(
						(Vector2)Input.mousePosition - mouseStartPos,
						released
					);
				}
				GL.Clear(true, true, Color.black);
				if(released)
					currentAction = CamAction.None;
			}
		}
	}
}

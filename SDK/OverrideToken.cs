using Camera2.Behaviours;
using Camera2.Configuration;
using Camera2.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Camera2.SDK {
	/// <summary>
	/// Used to temporarily override certain camera settings
	/// </summary>
	public class OverrideToken {
		static internal Dictionary<string, OverrideToken> tokens = new Dictionary<string, OverrideToken>();

		/// <summary>
		/// Request an OverrideToken for the camera with the given name.
		/// If there is no camera with the passed name, or there is already
		/// an active OverrideToken for this camera null will be returned
		/// </summary>
		/// <param name="camName">the name of the Camera</param>
		/// <returns>OverrideToken instance if successful, null otherwise</returns>
		public static OverrideToken GetTokenForCamera(string camName) {
			if(tokens.ContainsKey(camName))
				return null;

			if(!CamManager.cams.ContainsKey(camName))
				return null;

			var token = new OverrideToken(CamManager.cams[camName]);

			// Maybe something worth adding to keep track of who's doing funny
			//token.creator = new StackTrace().GetFrames()[1].GetMethod().DeclaringType.Assembly.GetName().Name;

			return tokens[camName] = token;
		}

		internal static OverrideToken GetTokenForCamera(Cam2 cam) => GetTokenForCamera(cam.name);

		Cam2 cam;
		string camName = "";
		//internal string creator { get; private set; }

		private OverrideToken(Cam2 cam) {
			this.cam = cam;
			camName = cam.name;
			position = new Vector3(cam.settings.targetPos.x, cam.settings.targetPos.y, cam.settings.targetPos.z);
			rotation = new Vector3(cam.settings.targetRot.x, cam.settings.targetRot.y, cam.settings.targetRot.z);
			_FOV = cam.settings.FOV;
			visibleObjects = cam.settings.visibleObjects.GetCopy();

			cam.settings.overrideToken = this;
		}

		/// <summary>
		/// Returns if the camera instance that this OverrideToken was created for still exists
		/// </summary>
		public bool isValid => cam != null && CamManager.cams.ContainsKey(camName);

		/// <summary>
		/// Closes this OverrideToken and returns the camera's values back to their default
		/// </summary>
		public void Close() {
			tokens.Remove(camName);

			if(isValid) {
				cam.settings.overrideToken = null;

				// Trigger setter for update
				cam.settings.FOV = cam.settings.FOV;
				cam.settings.ApplyPositionAndRotation();
				cam.settings.ApplyLayerBitmask();
			}
			cam = null;
		}

		public Vector3 position;
		public Vector3 rotation;

		public Vector3 currentPosition => cam.transformchain.position;
		public Quaternion currentRotation => cam.transformchain.rotation;

		/// <summary>
		/// Applies the currently set position / rotation to the camera, 
		/// needs to be called for changes to have an effect
		/// </summary>
		public void UpdatePositionAndRotation() {
			if(isValid)
				cam.settings.ApplyPositionAndRotation();
		}

		private float _FOV;

		public float FOV {
			get => _FOV;
			set {
				_FOV = value;
				if(isValid)
					cam.UCamera.fieldOfView = _FOV;
			}
		}

		public GameObjects visibleObjects { get; private set; }

		/// <summary>
		/// Applies the currently configured object visibilities,
		/// needs to be called for changes to have an effect
		/// </summary>
		public void UpdateVisibleObjects() {
			if(isValid)
				cam.settings.ApplyLayerBitmask();
		}
	}
}

using Camera2.Configuration;
using Camera2.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Camera2.SDK {
	public static class Cameras {
		/// <summary>
		/// names of all cameras available
		/// </summary>
		public static IEnumerable<string> available => CamManager.cams.Keys.AsEnumerable();

		/// <summary>
		/// Names of all cameras which are currently active
		/// </summary>
		public static IEnumerable<string> active => CamManager.cams.Values.Where(x => x.gameObject.activeSelf).Select(x => x.name);

		/// <summary>
		/// Enables or disables a given camera
		/// </summary>
		/// <param name="cameraName">Name of the camera</param>
		/// <param name="active">true / false depending on if the camera should be active</param>
		public static void SetCameraActive(string cameraName, bool active = false) {
			CamManager.cams[cameraName]?.gameObject.SetActive(active);
		}

		/// <summary>
		/// Returns the type of the camera
		/// </summary>
		/// <param name="cameraName">Name of the camera</param>
		/// <returns>Type of the camera, null if there is no camera with the requested name</returns>
		public static CameraType? GetCameraType(string cameraName) {
			return CamManager.cams[cameraName]?.settings.type;
		}
	}
}

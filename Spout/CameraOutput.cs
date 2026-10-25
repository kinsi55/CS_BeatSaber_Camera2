using System;

namespace Camera2.Spout {
	internal enum SpoutResolution { MatchCamera, HD720, HD1080, QHD1440, UHD2160 }

	internal static class CameraOutput {
		internal static bool ShouldRender(bool inScene, bool selected) => inScene || selected;

		internal static bool ShouldShowDesktop(bool active, bool inScene, bool selected, bool hideDesktop) =>
			active && (inScene || selected) && !hideDesktop;

		internal static void GetOutputSize(SpoutResolution resolution, int cameraWidth, int cameraHeight, out int width, out int height) {
			switch(resolution) {
				case SpoutResolution.HD720: width = 1280; height = 720; break;
				case SpoutResolution.HD1080: width = 1920; height = 1080; break;
				case SpoutResolution.QHD1440: width = 2560; height = 1440; break;
				case SpoutResolution.UHD2160: width = 3840; height = 2160; break;
				default: width = Math.Max(1, cameraWidth); height = Math.Max(1, cameraHeight); break;
			}
		}

		internal static void GetRenderSize(bool spoutEnabled, SpoutResolution resolution, int viewportWidth, int viewportHeight, float scale, out int width, out int height) {
			var baseWidth = viewportWidth;
			var baseHeight = viewportHeight;
			if(spoutEnabled)
				GetOutputSize(resolution, viewportWidth, viewportHeight, out baseWidth, out baseHeight);
			width = Math.Max(1, (int)Math.Round(baseWidth * scale));
			height = Math.Max(1, (int)Math.Round(baseHeight * scale));
		}
	}
}

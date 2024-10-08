using HarmonyLib;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(DepthTextureController), nameof(DepthTextureController.Init))]
	static class HookSettingsManager {
		public static SettingsManager settingsManager { get; private set; }
		public static bool useDepthTexture { get; private set; }
		static void Postfix(SettingsManager settingsManager) {
			HookSettingsManager.settingsManager = settingsManager;
			useDepthTexture = settingsManager.settings.quality.smokeGraphics;
		}
	}
}

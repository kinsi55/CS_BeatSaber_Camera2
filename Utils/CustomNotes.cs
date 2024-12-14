using HarmonyLib;
using System;
using System.Reflection;

namespace Camera2.Utils {
	class CustomNotesUtil {
		static PropertyInfo LayerUtils_HMDOnly;

		public static void Reflect() {
			LayerUtils_HMDOnly = IPA.Loader.PluginManager.GetPluginFromId("CustomNotes")?
				.Assembly.GetType("CustomNotes.Utilities.Configuration")?
				.GetProperty("HmdOnlyEnabled", BindingFlags.Public | BindingFlags.Static);
		}

		public static bool HasHMDOnlyEnabled() => LayerUtils_HMDOnly != null && (bool)LayerUtils_HMDOnly.GetValue(null);
	}
}

using HarmonyLib;
using System;
using System.Reflection;

namespace Camera2.Utils {
	class CustomNotesUtil {
		static PropertyInfo LayerUtils_HMDOnly = IPA.Loader.PluginManager.GetPluginFromId("Custom Notes")?
			.Assembly.GetType("CustomNotes.Utilities.LayerUtils")?
			.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);

		public static bool HasHMDOnlyEnabled() => LayerUtils_HMDOnly?.GetValue(null) == true;
	}
}

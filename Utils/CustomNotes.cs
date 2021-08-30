using HarmonyLib;
using System;
using System.Reflection;

namespace Camera2.Utils {
	class CustomNotesUtil {
		static Type LayerUtils = AccessTools.TypeByName("CustomNotes.Utilities.LayerUtils");
		static PropertyInfo LayerUtils_HMDOnly = LayerUtils?.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);

		public static bool HasHMDOnlyEnabled() => LayerUtils_HMDOnly != null && (bool)LayerUtils_HMDOnly.GetValue(null);
	}
}

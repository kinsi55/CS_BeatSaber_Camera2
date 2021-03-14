using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	class CustomNotesUtil {
		static Type LayerUtils = AccessTools.TypeByName("CustomNotes.Utilities.LayerUtils");
		static PropertyInfo LayerUtils_HMDOnly = LayerUtils?.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);

		public static bool HasHMDOnlyEnabled() => (bool)LayerUtils_HMDOnly?.GetValue(null);
	}
}

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Camera2.Utils {
	class CustomNotesUtil {
		static Type LayerUtils;
		static PropertyInfo LayerUtils_HMDOnly;

		public static void Init() {
			LayerUtils = AccessTools.TypeByName("CustomNotes.Utilities.LayerUtils");
			LayerUtils_HMDOnly = LayerUtils?.GetProperty("HMDOnly", BindingFlags.Public | BindingFlags.Static);
		}

		public static bool HasHMDOnlyEnabled() {
			return LayerUtils_HMDOnly != null && (bool)LayerUtils_HMDOnly.GetValue(null);
		}
	}
}

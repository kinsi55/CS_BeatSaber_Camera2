using UnityEngine;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StretchableObstacle), "SetSizeAndColor")]
	public class TransparentWalls {
		static void Postfix(Transform ____obstacleCore) {
			if(____obstacleCore != null)
				____obstacleCore.gameObject.layer = (int)VisibilityLayers.WallTextures;
		}

		public static void MakeWallsOpaqueForMainCam() {
			if(Camera.main != null)
				Camera.main.cullingMask |= (int)VisibilityMasks.WallTextures;
		}
	}
}

using Camera2.Utils;
using HarmonyLib;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StretchableObstacle), nameof(StretchableObstacle.SetSizeAndColor))]
	static class TransparentWalls {
		static void Postfix(Transform ____obstacleCore, ParametricBoxFakeGlowController ____obstacleFakeGlow, MaterialPropertyBlockController[] ____materialPropertyBlockControllers) {
			if(____obstacleCore) {
				// No-Bloom inner wall texture thingy
				if(____obstacleFakeGlow && ____obstacleFakeGlow.enabled == true) {
					____obstacleFakeGlow.gameObject.layer = (int)VisibilityLayers.Walls;

					// This is PROBABLY not perfect, we'll have to see if this breaks at some point
					if(____materialPropertyBlockControllers.Length > 1)
						____materialPropertyBlockControllers[1].gameObject.layer = (int)VisibilityLayers.WallTextures;
				}

				____obstacleCore.gameObject.layer = (int)VisibilityLayers.WallTextures;
			}

			//____obstacleFakeGlow.enabled = false;
		}

		public static void MakeWallsOpaqueForMainCam() {
			var a = Camera.main;
			if(a) {
#if DEBUG
				Plugin.Log.Info("Made walls opaque for main cam!");
#endif
				a.cullingMask |= (int)VisibilityMasks.WallTextures;
			}
		}
	}
}

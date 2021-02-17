using UnityEngine;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StretchableObstacle), nameof(StretchableObstacle.SetSizeAndColor))]
	class TransparentWalls {
		static void Postfix(Transform ____obstacleCore, ref ParametricBoxFakeGlowController ____obstacleFakeGlow) {
			if(____obstacleCore != null) {
				____obstacleCore.gameObject.layer = (int)VisibilityLayers.WallTextures;

				// No-Bloom inner wall texture thingy
				if(____obstacleFakeGlow.enabled)
					____obstacleCore.GetChild(0).gameObject.layer = (int)VisibilityLayers.WallTextures;
			}

			____obstacleFakeGlow.enabled = false;
		}

		public static void MakeWallsOpaqueForMainCam() {
			if(Camera.main != null) {
#if DEBUG
				Plugin.Log.Info("Made walls opaque for main cam!");
#endif
				Camera.main.cullingMask |= (int)VisibilityMasks.WallTextures;
			}
		}
		
		// No-Bloom fake bloom wall edge
		[HarmonyPatch(typeof(ParametricBoxFakeGlowController), nameof(ParametricBoxFakeGlowController.Awake))]
		class FunnyNoBloom {
			static void Postfix(ParametricBoxFakeGlowController __instance) {
				__instance.gameObject.layer = (int)VisibilityLayers.Walls;
			}
		}
	}
}

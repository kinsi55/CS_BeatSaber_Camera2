using UnityEngine;
using HarmonyLib;

namespace Camera2.HarmonyPatches {
	//[HarmonyPatch(typeof(PyramidBloomRendererSO), "OnEnable")]
	//class BloomRendererInstantiateFix {
	//	static bool Prefix(Shader ____shader) {
	//		return ____shader != null;
	//	}
	//}

	//[HarmonyPatch(typeof(PyramidBloomMainEffectSO), "OnEnable")]
	//class BloomRendererInstantiateFix2 {
	//	static bool Prefix(Shader ____fadeShader, Shader ____mainEffectShader, Material ____fadeMaterial, Material ____mainEffectMaterial) {
	//		return !((____fadeShader == null && ____fadeMaterial == null) || (____mainEffectShader == null && ____mainEffectMaterial == null));
	//	}
	//}

	/*
	 * Ending up at this solution took me half a day. It only applies when you have bloom off - So dont have bloom off!
	 * For WHATEVER reason, when you turn off bloom in the game, NOTHING on the desktop will render.
	 * There were a couple of situations I found that fixed this:
	 * - Disabling the VR camera (Obviously not an option)
	 * - Using XRSettings.gameViewRenderMode to mirror the VR view to the Desktop (Ends up behind the camera canvas, but still kinda bad)
	 * - This. I am not sure why this works, I gave up trying to understand whatever Beat Games cooked up with their effect rendering pipeline
	 */
	[HarmonyPatch(typeof(MainEffectSO), "Render")]
	class BloomRendererInstantiateFix3 {
		static bool Prefix(RenderTexture src, RenderTexture dest, MainEffectSO __instance) {
			var x = RenderTexture.active;
			Graphics.Blit(src, dest);
			RenderTexture.active = x;
			return false;
		}
	}

	[HarmonyPatch(typeof(MainEffectController), "OnPostRender")]
	class BloomRendererInstantiateFix4 {
		static void Postfix(ImageEffectController ____imageEffectController, MainEffectContainerSO ____mainEffectContainer) {
			____imageEffectController.enabled = true;
		}
	}
}

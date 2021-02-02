using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Utils {
	/*
	 * Unused ATM. The idea was being able to configure bloom per camera or have bloom on on the desktop but off in VR.
	 * This works fine for Cloning / recreating everything related to the bloom effect but honestly I CBA to implement
	 * it rn
	 */
	class BloomShite {
		public static MainEffectController InstantiateBloomForCamera(Camera cam) {
			var camClone = cam.gameObject;

			var mainEffectController = camClone.GetComponent<MainEffectController>();


			var effectContainer = GameObject.Instantiate(mainEffectController.GetField<MainEffectContainerSO, MainEffectController>("_mainEffectContainer"));
			var effectInstance = effectContainer.GetField<MainEffectSO, MainEffectContainerSO>("_mainEffect");

			if(effectInstance.GetType() == typeof(PyramidBloomMainEffectSO)) {
				effectInstance = GameObject.Instantiate(effectInstance);
			} else {
				PyramidBloomRendererSO renderer = ScriptableObject.CreateInstance<PyramidBloomRendererSO>();
				renderer.SetField("_shader", Shader.Find("Hidden/PostProcessing/Bloom"));
				typeof(PyramidBloomRendererSO).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(renderer, null);


				var newEffectInstance = ScriptableObject.CreateInstance<PyramidBloomMainEffectSO>();

				newEffectInstance.SetField("_fadeShader", Shader.Find("Hidden/BlitBlendColor"));
				newEffectInstance.SetField("_fadeMaterial", ((NoPostProcessMainEffectSO)effectInstance).GetField<Material, NoPostProcessMainEffectSO>("_fadeMaterial"));
				newEffectInstance.SetField("_mainEffectShader", Shader.Find("Hidden/MainEffect"));
				newEffectInstance.SetField("_bloomRenderer", renderer);
				typeof(PyramidBloomMainEffectSO).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(newEffectInstance, null);

				effectInstance = newEffectInstance;
			}

			effectContainer.Init(effectInstance);

			mainEffectController.SetField("_mainEffectContainer", effectContainer);

			return mainEffectController;
		}
	}
}

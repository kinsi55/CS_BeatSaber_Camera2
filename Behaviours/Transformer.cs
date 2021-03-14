using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Behaviours {
#if DEBUG
	public
#endif
	enum TransformerOrders : int {
		SmoothFollowPivoting = -500,
		PositionOffset = -400,
		Follow360 = -300,
		SmoothFollowAbsolute = -200,
		MovementScriptProcessor = 0,

		Default = 0
	}

#if DEBUG
	public
#endif
	class Transformer : MonoBehaviour {
		List<KeyValuePair<string, Transformer>> camTransformers;
		Cam2 cam;
		public int order { get; private set; }

		void Awake() => enabled = false;

		public static Transformer Get(string type, int order, Cam2 cam, List<KeyValuePair<string, Transformer>> transformerList) {
			var x = new GameObject($"T_{type}").AddComponent<Transformer>();

			x.cam = cam;
			x.camTransformers = transformerList;
			x.order = order;

			return x;
		}

		public void OnDestroy() {
			if(cam.destroying)
				return;

			var prevParent = cam.UCamera.transform;
			for(var i = camTransformers.Count(); i-- > 0;) {
				var x = camTransformers[i].Value;

				if(x == this) {
					prevParent.SetParent(transform.parent, false);

					camTransformers.RemoveAt(i);

					break;
				}

				prevParent = x.transform;
			}
			Destroy(gameObject);
		}
	}
}

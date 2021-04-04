using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Utils {
	static class TransformerOrders {
		public static int ModmapParenting = -600;
		public static int SmoothFollow = -500;
		public static int PositionOffset = -400;
		public static int Follow360 = -300;
		public static int MovementScriptProcessor = 0;
	}

	class Transformer {
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
		public Vector3 rotationEuler { set => rotation = Quaternion.Euler(value); }

		public Vector3 positionSum;
		public Quaternion rotationSum;

		public int order = 0;

		public bool applyAsAbsolute = false;
	}

	class TransformChain {
		private List<Transformer> transformers = new List<Transformer>();
		public Dictionary<string, Transformer> transformerMap = new Dictionary<string, Transformer>();


		public void Resort(bool calculate = true) {
			transformers.Sort((a, b) => a.order - b.order);

			if(calculate) Calculate();
		}

		public Vector3 position { get; private set; }
		public Quaternion rotation { get; private set; }

		Transform tbase;
		Transform target;
		public TransformChain(Transform tbase, Transform target = null) {
			this.tbase = tbase;
			this.target = target;
		}

		public Transformer AddOrGet(string name, int order = 0, bool sortIn = true) {
			if(transformerMap.ContainsKey(name))
				return transformerMap[name];

			var t = new Transformer();

			t.order = order;

			transformers.Add(t);
			transformerMap.Add(name, t);

			if(sortIn)
				Resort(false);

			return t;
		}

		//public void BacktrackTo(Transformer t, ref Vector3 pos, ref Quaternion rot) {
		//	var index = transformers.IndexOf(t);

		//	Transformer x;

		//	for(int i = transformers.Count - 1; i > index; i--) {
		//		x = transformers[i];

		//		if(x.position != Vector3.zero)
		//			pos -= x.applyAsAbsolute ? x.position : Quaternion.Inverse(x.rotation) * x.position;

		//		if(x.rotation != Quaternion.identity) {
		//			if(!x.applyAsAbsolute) {
		//				rot *= Quaternion.Inverse(x.rotation);
		//			} else {
		//				rot = Quaternion.Inverse(x.rotation) * rot;
		//			}
		//		}
		//	}

		//	Calculate(false);

		//	pos = pos - t.positionSum + t.position;

		//	//rot = t.rotation * (t.rotationSum * );
		//}

#if DEBUG
		public string debug = "";
#endif

		public void Calculate(bool apply = true) {
			if(transformers.Count == 0) {
				position = Vector3.zero;
				rotation = Quaternion.identity;
				return;
			}

			Transformer x;
#if DEBUG
			debug = "";
#endif

			position = tbase.position;
			rotation = tbase.rotation;

			for(var i = 0; i != transformers.Count; i++) {
				x = transformers[i];

				if(x.position != Vector3.zero)
					position += x.applyAsAbsolute ? x.position : rotation * x.position;

				if(x.rotation != Quaternion.identity) {
					if(!x.applyAsAbsolute) {
						rotation *= x.rotation;
					} else {
						rotation = x.rotation * rotation;
					}
				}

				x.positionSum = position;
				x.rotationSum = rotation;

#if DEBUG
				debug += $"{i} ({transformerMap.First(y => y.Value == x).Key}): {x.position} {x.rotation} => {position} {rotation}\n";
#endif
			}

			if(target == null || !apply)
				return;

			target.position = position;
			target.rotation = rotation;
		}
	}
}

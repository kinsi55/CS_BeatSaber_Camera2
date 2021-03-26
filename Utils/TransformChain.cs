﻿using System;
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
#if DEBUG
		public string debug = "";
#endif

		public void Calculate() {
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

				if(i == 0 && position == Vector3.zero && rotation == Quaternion.identity) {
					position = x.position;
					rotation = x.rotation;
				} else {
					if(x.position != Vector3.zero)
						position += x.applyAsAbsolute ? x.position : rotation * x.position;

					if(x.rotation != Quaternion.identity)
						rotation *= x.rotation;
				}

				x.positionSum = position;
				x.rotationSum = rotation;

#if DEBUG
				debug += $"{i} ({transformerMap.First(y => y.Value == x).Key}): {x.position} {x.rotation} => {position} {rotation}\n";
#endif
			}

			if(target == null)
				return;

			target.position = position;
			target.rotation = rotation;
		}
	}
}
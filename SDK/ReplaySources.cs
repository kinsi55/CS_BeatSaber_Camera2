using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.SDK {
	public static class ReplaySources {
		internal static HashSet<ISource> sources = new HashSet<ISource>();

		public interface ISource {
			public string name { get; }
			public bool isInReplay { get; }
			public Vector3 localHeadPosition { get; }
			public Quaternion localHeadRotation { get; }
		}

		public class GenericSource : ISource {
			public string name { get; private set; }
			public bool isInReplay { get; private set; }
			public Vector3 localHeadPosition { get; private set; }
			public Quaternion localHeadRotation { get; private set; }

			public GenericSource(string name) {
				this.name = name;
			}

			public void Update(ref Vector3 localHeadPosition, ref Quaternion localHeadRotation) {
				this.localHeadPosition = localHeadPosition;
				this.localHeadRotation = localHeadRotation;
			}

			public void SetActive(bool isInReplay) {
				this.isInReplay = isInReplay;
			}
		}

		public static void Register(ISource source) => sources.Add(source);

		public static void Unregister(ISource source) => sources.Remove(source);
	}
}

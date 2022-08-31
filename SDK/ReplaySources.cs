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
			public bool isPlaying { get; }
			public Vector3 localHeadPosition { get; }
			public Quaternion localHeadRotation { get; }
		}

		public static void Register(ISource source) => sources.Add(source);

		public static void Unregister(ISource source) => sources.Remove(source);
	}
}

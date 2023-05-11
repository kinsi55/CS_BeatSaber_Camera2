using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Managers {
	static class ShaderManager {
		public class LoadedShader {
			public readonly string assetBundlePath;
			public readonly string shaderName;
			public readonly Material shaderMat;

			public readonly IReadOnlyDictionary<string, int> propIds;

			public LoadedShader(string assetBundlePath, string shaderName) {
				if(!File.Exists(assetBundlePath))
					throw new FileNotFoundException();

				this.assetBundlePath = assetBundlePath;
				this.shaderName = shaderName;

				var b = AssetBundle.LoadFromFile(assetBundlePath);

#if DEBUG
				Plugin.Log.Warn("Loaded assetbundle from path " + assetBundlePath);
#endif
				var shader = b.LoadAsset(shaderName);

				shaderMat = shader is Material mat ? mat :
						shader is Shader sha ? new Material(sha) : 
						null;

				if(shaderMat != null) {
					var properties = new Dictionary<string, int>();

					var s = shaderMat.shader;

					for(var i = s.GetPropertyCount(); i-- != 0; )
						properties[s.GetPropertyName(i)] = s.GetPropertyNameId(i);

					this.propIds = properties;
				} else {
					b.Unload(true);
					throw new KeyNotFoundException();
				}

				b.Unload(shader == null);
			}
		}

		static readonly Dictionary<string, LoadedShader> loadedShaderMats = new Dictionary<string, LoadedShader>();

		static string GetShaderKey(string assetBundlePath, string shaderName) => $"{assetBundlePath}:{shaderName}";


		public static LoadedShader GetOrLoadShader(string assetBundlePath, string shaderName, bool reload = false) {
			var key = GetShaderKey(assetBundlePath, shaderName);

			if(loadedShaderMats.TryGetValue(key, out var loadedShader)) {
				if(!reload)
					return loadedShader;

				UnityEngine.Object.Destroy(loadedShader.shaderMat);
			}

			return null;
			//return loadedShaderMats[key] = new LoadedShader(assetBundlePath, shaderName);
		}

		public static void Reload() {
			foreach(var shader in loadedShaderMats.Values)
				GetOrLoadShader(shader.assetBundlePath, shader.shaderName, true);
		}
	}
}

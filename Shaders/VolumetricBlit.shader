Shader "Custom/VolumetricBlit" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
	}
	SubShader {
		ColorMask RGB
		Pass {
			Cull Back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5

			#include "UnityCG.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			half4 _MainTex_ST;

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

				return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
			}
			ENDCG
		}

		Pass {
			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5

			#include "UnityCG.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			half4 _MainTex_ST;

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				// For back face, we mirror the texture so that it appears right way around from both sides
				o.uv.x = 1 - o.uv.x;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
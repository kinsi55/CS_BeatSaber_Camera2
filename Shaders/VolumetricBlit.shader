Shader "Custom/VolumetricBlit" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
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

			sampler2D_half _MainTex;
			half4 _MainTex_ST;

			struct ayo {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			void vert(inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
			}

			fixed4 frag(ayo i) : SV_Target {
				return tex2D(_MainTex, i.texcoord);
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

			sampler2D_half _MainTex;
			half4 _MainTex_ST;

			struct ayo {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			void vert(inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
				// For back face, we mirror the texture so that it appears right way around from both sides
				v.texcoord.x = 1 - v.texcoord.x;
			}

			fixed4 frag(ayo i) : SV_Target {
				return tex2D(_MainTex, i.texcoord);
			}
			ENDCG
		}
	}
}

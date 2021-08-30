Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_HasDepth ("Depthtexture enabled", Range(0.0,1.0)) = 1.0
		_Threshold ("Transparency Threshold", Range(1.0,100.0)) = 30.0
	}
	SubShader {
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" }
		ColorMask A

		Pass {
			ZClip False
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct ayo {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			void vert (inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
			}

			sampler2D_half _LastCameraDepthTexture;
			sampler2D_half _MainTex;
			half _Threshold;
			half _HasDepth;

			fixed4 frag (ayo i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				if(_Threshold == 0)
					return 1;

				float rgbt = min(2, col.r + col.g + col.b) / (_Threshold / 3);

				if(_HasDepth == 0)
					return rgbt;

				float d = Linear01Depth(tex2D(_LastCameraDepthTexture, i.uv).r);

				return min(1, max((1 - d) * 10, rgbt));
			}
			ENDCG
		}
	}
}

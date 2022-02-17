Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_HasDepth ("Depthtexture enabled", Range(0.0,1.0)) = 1.0
		_Threshold ("Transparency Threshold", Range(1.0,80.0)) = 30.0
	}
	SubShader {
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" "ForceNoShadowCasting"="True" }

		Blend Off

		Pass {
			ZClip False
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5

			#include "UnityCG.cginc"

			struct ayo {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			void vert (inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
			}

			sampler2D_half _CameraDepthTexture;
			sampler2D_half _MainTex;
			half _Threshold;
			half _HasDepth;

			fixed4 frag (ayo i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				if(_Threshold == 0) {
					col.a = 1;
					return col;
				}

				if(_HasDepth == 1) {
					float d = tex2D(_CameraDepthTexture, i.uv).r;

					col.a = (1 - Linear01Depth(d)) * 40;
				}

				if(col.a < 0.01)
					col.a = clamp(pow(Luminance(col) * (80 - _Threshold), 2), 0, 0.9);

				return col;
			}
			ENDCG
		}
	}
}

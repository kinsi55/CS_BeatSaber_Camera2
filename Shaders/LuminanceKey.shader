Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_HasDepth ("Depthtexture enabled", Range(0.0,1.0)) = 1.0
		_Threshold ("Transparency Threshold", Range(1.0,20.0)) = 30.0
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
			#pragma target 3.5

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
				if(_Threshold == 0)
					return 1;

				if(_HasDepth == 1) {
					float d = tex2D(_LastCameraDepthTexture, i.uv).r;

					float outVal = pow((1 - Linear01Depth(d)) * 1.05, 3);

					if(outVal > 0.1)
						return outVal;
				}

				fixed4 col = tex2D(_MainTex, i.uv);
				return pow(min(1, (col.r + col.g + col.b) * (20 - _Threshold)), 2) - 0.1;
			}
			ENDCG
		}
	}
}

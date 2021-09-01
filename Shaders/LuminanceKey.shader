Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_HasDepth ("Depthtexture enabled", Range(0.0,1.0)) = 1.0
		_Threshold ("Transparency Threshold", Range(1.0,80.0)) = 30.0
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

					float outVal = 1 - pow(Linear01Depth(d), 2);

					if(outVal > 0.01)
						return outVal;
				}

				fixed3 col = tex2D(_MainTex, i.uv);
				return clamp(pow(Luminance(col) * (80 - _Threshold), 2), 0, 0.9);
			}
			ENDCG
		}
	}
}

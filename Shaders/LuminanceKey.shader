Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Threshold ("Transparency Threshold", Range(0.0,100.0)) = 30.0
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

			sampler2D_half _MainTex;
			half _Threshold;

			fixed4 frag (ayo i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				if(_Threshold == 0)
					return 1;

				return clamp(((col.r + col.g + col.b) * _Threshold) - 2.0, 0, 0.9);
			}
			ENDCG
		}
	}
}

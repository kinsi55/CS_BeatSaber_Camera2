Shader "Unlit/LuminanceKey" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Threshold ("Transparency Threshold", Range(0.0,100.0)) = 30.0
	}
	SubShader {
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" }
		ColorMask A

		Pass {
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

			sampler2D _MainTex;
			float _Threshold;
			float _NoAA;

			half4 frag (ayo i) : SV_Target {
				half4 col = tex2D(_MainTex, i.uv);

				if(_Threshold == 0)
					return 1;

				return clamp(((col.r + col.g + col.b) * _Threshold) - 2.0, 0, 0.9);
			}
			ENDCG
		}
	}
}

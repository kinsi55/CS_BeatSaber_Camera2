Shader "Unlit/OutlineUtil" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Width ("Outline Width", Range(0.0, 1.0)) = 10.0
	}
	SubShader {
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" "ForceNoShadowCasting"="True" }

		Blend Off
		ColorMask RGBA

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
				float4 screenPos : TEXCOORD1;
			};

			void vert (inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
				v.screenPos = ComputeScreenPos(v.vertex);
			}

			sampler2D_half _MainTex;
			half _Width;

			fixed4 frag (ayo i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				float2 s = _ScreenParams;
				float2 p = i.screenPos * s;

				if(p.x <= _Width || p.y <= _Width || p.x > s.x - _Width || p.y > s.y - _Width) {
					col = max(0.3, sin(_Time.w * 4)) - col;

					col.a = 1;
				}

				return col;
			}
			ENDCG
		}
	}
}

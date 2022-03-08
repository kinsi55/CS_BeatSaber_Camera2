// This is originating from https://github.com/unity3d-jp/UnityChanBallRoll I dont even know what that is lmao

Shader "Custom/MotionBlur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AccumOrig("AccumOrig", Float) = 0.65
	}

	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
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

			fixed4 _MainTex_ST;
			fixed _AccumOrig;

			void vert (inout ayo v) {
				v.vertex = UnityObjectToClipPos(v.vertex);
			}

			sampler2D_half _MainTex;

			fixed4 frag (ayo i) : SV_Target {
				return fixed4(tex2D(_MainTex, i.uv).rgb, _AccumOrig);
			}
			ENDCG
		}
	}
}
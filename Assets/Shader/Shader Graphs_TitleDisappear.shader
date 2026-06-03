Shader "Shader Graphs/TitleDisappear"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Strength ("Strength", Range(0, 1)) = 0
		_Scale ("Scale", Range(0.1, 2)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"CanUseSpriteAtlas"="True"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Strength;
			float _Scale;

			v2f vert(appdata_t input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.color = input.color * _Color;
				return output;
			}

			float Hash21(float2 p)
			{
				p = frac(p * float2(123.34, 456.21));
				p += dot(p, p + 45.32);
				return frac(p.x * p.y);
			}

			fixed4 frag(v2f input) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, input.uv);
				float safeScale = max(_Scale, 0.1);
				float noise = Hash21(floor(input.uv * (48.0 * safeScale) + 0.5));
				float threshold = saturate(_Strength);
				float dissolve = smoothstep(threshold - 0.08, threshold + 0.08, noise);
				color.a *= dissolve;
				return color * input.color;
			}
			ENDCG
		}
	}
}

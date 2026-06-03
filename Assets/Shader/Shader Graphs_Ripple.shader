Shader "Shader Graphs/Ripple"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Strength ("Strength", Range(0.005, 0.1)) = 0.03
		_Speed ("Speed", Range(0, 0.2)) = 0.05
		_Wave ("Wave", Range(0, 0.2)) = 0.04
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
			float _Speed;
			float _Wave;
			float _Scale;

			v2f vert(appdata_t input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.color = input.color * _Color;
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				float timeValue = _Time.y * max(_Speed, 0.001) * 18.0;
				float safeScale = max(_Scale, 0.1);
				float waveX = sin((input.uv.y * 12.0 + timeValue) * safeScale) * _Strength;
				float waveY = cos((input.uv.x * 9.0 - timeValue * 0.85) * (safeScale + 0.35)) * (_Strength * 0.65);
				float2 displacedUv = input.uv + float2(waveX, waveY);

				fixed4 color = tex2D(_MainTex, displacedUv);
				float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
				float shimmer = 0.5 + 0.5 * sin((input.uv.x + input.uv.y) * 18.0 + timeValue * (5.0 + _Wave * 40.0));

				color.rgb *= lerp(0.72, 1.08, shimmer);
				color.a *= saturate(luminance * (0.12 + _Wave * 1.8));

				return color * input.color;
			}
			ENDCG
		}
	}
}

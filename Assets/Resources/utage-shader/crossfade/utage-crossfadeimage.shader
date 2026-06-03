Shader "Utage/CrossFadeImage"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_FadeTex("Fade Texture", 2D) = "white" {}
		_Strength("Strength", Range(0.0, 1.0)) = 0.2
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 worldPosition : TEXCOORD2;
			};

			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;
				OUT.texcoord1 = IN.texcoord1;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _FadeTex;
			fixed _Strength;

			half4 SampleStraightColor(sampler2D tex, float2 uv)
			{
				half4 sample = tex2D(tex, uv) + _TextureSampleAdd;
				sample.a = saturate(sample.a);
				if (sample.a <= (1.0h / 255.0h))
				{
					sample.rgb = 0;
					sample.a = 0;
				}

				return sample;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = SampleStraightColor(_MainTex, IN.texcoord);
				half4 detail = SampleStraightColor(_FadeTex, IN.texcoord1);
				fixed4 result;
				half alpha = lerp(color.a, detail.a, _Strength) * IN.color.a;
				half3 premulRgb = lerp(color.rgb * color.a, detail.rgb * detail.a, _Strength);
				premulRgb *= IN.color.rgb * IN.color.a;
				half clipAlpha = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				alpha *= clipAlpha;
				premulRgb *= clipAlpha;
				if (alpha <= (1.0h / 255.0h))
				{
					result.rgb = 0;
					result.a = 0;
				}
				else
				{
					result.rgb = premulRgb / alpha;
					result.a = alpha;
				}

#ifdef UNITY_UI_ALPHACLIP
				clip(result.a - 0.001);
#endif
				return result;
			}
			ENDCG
		}
	}
}

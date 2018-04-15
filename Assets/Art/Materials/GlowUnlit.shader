Shader "Custom/GlowUnlit"
{
	Properties
	{ 
		_Color("Main Color", Color) = (0,1,0,1)
		_GlowColor("Glow Color", Color) = (0,0,0,1)
		_GlowWidth("Glow Width", Range(1.0,5.0)) = 1.01
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : POSITION;
		float3 normal : NORMAL;
	};

	float4 _GlowColor;
	float _GlowWidth;

	v2f vert(appdata v) 
	{
		v.vertex.xyz *= _GlowWidth;

		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		Pass  //Render Outline
		{
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) : COLOR
			{
				return _GlowColor;
			}
			ENDCG
		}

		Pass
		{
			Color [_Color]
		}
	}
}

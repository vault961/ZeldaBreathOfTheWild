Shader "Unlit/BarrierEyeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_RimPower("RimPower", float) = 2.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _RimPower;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color * _RimPower;
				float sinTime = (_SinTime.w + 1.0f) * 0.5f;

				col.a = col.b;

				//col.rgb = float3(
				//	col.r * _RimPower,
				//	col.g *_RimPower * sinTime * 0.8f,
				//	col.b *_RimPower * sinTime * 1.3f) * _Color;

				clip(col);

				col.a *= saturate(_SinTime.w + 1.0f);
				return col;
			}

			ENDCG
		}
	}
}

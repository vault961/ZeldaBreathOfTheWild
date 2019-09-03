// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/GoalBarrierShader_Unlit"
{
	Properties
	{	
		_Color("Color", Color) = (1,1,1,1)
		_WaveColor("Wave Color", Color) = (1,1,1,1)
		_WaveSize("Wave Size", Vector) = (1, 1, 1, 1)
		_MainTex ("Base Texture", 2D) = "white" {}
		_UVTex("UV Texture", 2D) = "white" {}		
		_SpeedY("Speed along Y", Range(0, 5)) = 1
	}	
	SubShader
	{
		Tags {"RenderType" = "Transparent" "Queue" = "Transparent" }
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

			fixed4 _Color;
			//fixed4 _WaveColor;
			float _WaveColorAmount  = 0.33;
			float4 _WaveSize;
			sampler2D _MainTex;
			sampler2D _UVTex;		
			float4 _MainTex_ST;
			float _SpeedY;
			float _Mitigation;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 result;

				//// sample the texture
				fixed4 col = tex2D(_MainTex, float2 (i.uv.x, i.uv.y + _Time.y));
				fixed4 col2 = tex2D(_UVTex, float2 (i.uv.x * _WaveSize.x, i.uv.y * _WaveSize.y - _Time.x * _SpeedY));
				
				col.a = col.r;
				col.rgb = _Color;
				
				col2.a = col2.r;
				col2.rgb = _WaveColorAmount;

				col += col2;

				//result = (1.0 - ((1.0f - col ) * (1.0f - col2)));
				//col += result;
				
				return col;
			}
			ENDCG
		}
	}
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MgtLineShader" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,0.5)
		_AlphaTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Offset("Offset", float) = 0
	//_ExtrusionAmount("Z Extrusion Amount", Float) = 0
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100

			ZWrite On
			ZTest Less
			Blend SrcAlpha One
		//Offset 0, -5000

	Pass{

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		fixed4 _Color;
		sampler2D _AlphaTex;
		float4 _AlphaTex_ST;
		float _Offset;

		struct v2f {
			float4 pos : SV_POSITION;
			half4 color : COLOR0;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata_full v)
		{
			v2f o;

			//float3 camDirObjSpace = normalize(ObjSpaceViewDir(v.vertex));
			//v.vertex.xyz += _ExtrusionAmount * camDirObjSpace;
			o.color = v.color;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _AlphaTex);
			o.uv.y = o.uv.y + _Offset ;

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 texcol = tex2D(_AlphaTex, i.uv * _Time.x) * i.color;
			return texcol * _Color;
		}
		ENDCG
	}
	}
		FallBack "Unlit/Transparent Cutout"
}
Shader "Custom/IronShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Burn("Burn",float) = 1
		_Emissive("Emissive",float) = 0.0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex ("Normal", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Renderqueue"="Transparent"}
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpTex;
		};

		fixed4 _Color;
		float _Burn;
		float _Emissive;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Albedo = c.rgb * _Burn;
			o.Alpha = c.a;
			o.Emission = _Emissive;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

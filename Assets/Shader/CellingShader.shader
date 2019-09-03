Shader "Custom/CellingShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex ("Normal", 2D) = "bump" {}
		_Burn("Burn",Range(0, 10)) = 3
		_Glow("Intensity", Range(0, 3)) = 1
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpTex;
		};

		half _Burn;
		half _Glow;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex)*_Color * _Burn;
			
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Emission = o.Albedo;
			o.Emission *= _Glow;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

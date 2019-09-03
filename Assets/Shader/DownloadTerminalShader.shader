Shader "Custom/DownloadTerminalShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Burn("Burn", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_BumpMap ("NormalMap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard
		#pragma shader_feature _METALLICGLOSSMAP

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MetallicGlossMap;
		float4 _Color;
		float _Burn;

		struct Input {
			float4 color : Color;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_MetallicGlossMap;
		};

		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 metal = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap) * _Color;
			
			o.Albedo = c.rgb;

			if (metal.r > _Burn )
			{	
				o.Emission = metal.r * _Color * 4.0f;
			}

			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}
	FallBack "Diffuse"
}

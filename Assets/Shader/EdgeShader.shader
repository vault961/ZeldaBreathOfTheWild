Shader "Custom/EdgeShader" {
	Properties {
		_ColorTint("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_AlphaTex("AlphaTex",2D) = "white"{}
		_BumpMap("NormalMap", 2D) = "bump" {}
		_RimPower("RimPower", float) = 2.0
	}
	SubShader {

		Tags{"RenderType" = "Opaque" }

		//Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows
		#pragma surface surf Lambert alphatest:_Cutoff

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float4 color : Color;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_AlphaTex;
		};

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _AlphaTex;

		float4 _ColorTint;
		float4 _RimColor;
		float _RimPower;

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutput o) {

			// Albedo comes from a texture tinted by color
			IN.color = _ColorTint;

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			
			o.Alpha = tex2D(_AlphaTex, IN.uv_AlphaTex).r;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));		
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color ;

			// 2.3f : RimPower
			float sinTime = (_SinTime.w + 1.0f);

			float3 col = float3(o.Albedo.r * _RimPower,
				o.Albedo.g * _RimPower * sinTime * 0.8f,
				o.Albedo.b * _RimPower * sinTime * 1.3f);

			clip(col);

			o.Emission = col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

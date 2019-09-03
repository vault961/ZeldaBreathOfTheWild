Shader "Custom/WaterShader" {
	Properties {
		_BumpMap ("NormalMap", 2D) = "bump" {}
		_MainTex("MainTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _BumpMap;
		sampler2D _MainTex;
		//samplerCUBE _Cube;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldRefl;
			float3 viewDir;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap /*+ _Time.x / 2.0f*/));
			float3 refcolor = tex2D(_MainTex, WorldReflectionVector(IN, o.Normal));
			
			//rim term
			float rim = saturate(dot(o.Normal, IN.viewDir));
			rim = pow(1 - rim, 1.5);

			
			o.Emission = refcolor * rim * 2.0;
			o.Alpha = saturate(rim - 0.8);
		}
		ENDCG
	}
	FallBack "Diffuse"
}

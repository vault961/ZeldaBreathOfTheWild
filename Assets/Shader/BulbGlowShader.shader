Shader "Custom/BulbGlowShader" {
	Properties {
		_ColorTint("Color", Color) = (1,1,1,1)
		_Alpha("Alpha", Range(0,1)) = 0.0
		_RimPower("RimPower", float) = 0.0
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float4 color : Color;
		};

		//fixed4 _Color;
		float4 _ColorTint;
		float4 _RimColor;
		half _Alpha;
		float _RimPower;
		uniform float4 _MyDirVector;

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {			
			IN.color = _ColorTint;
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Albedo = IN.color;
			o.Alpha = _Alpha;
			// 2.0f : RimPower

			half rim = 1.0 - saturate(dot(normalize(_MyDirVector.xyz), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower);

			//o.Emission = o.Albedo * 100.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

Shader "Custom/Dissolve" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SliceScaleX("SliceScaleX", Range(0.0, 10.0)) = 1.0
		_SliceScaleY("SliceScaleY", Range(0.0, 10.0)) = 1.0
		_SliceGuide("Slice Guide (RGB)", 2D) = "white" {}
		_SliceAmount("Slice Amount", Range(0.0, 1.0)) = 0

		_BurnSize("Burn Size", Range(0.0, 1.0)) = 0.15
		_BurnRamp("Burn Ramp (RGB)", 2D) = "white" {}
		_BurnColor("Burn Color", Color) = (1,1,1,1)
		_EmissionAmount("Emission amount", float) = 2.0

		_Delay("Delay", float) = 0.0
		_PreBurnColorAmount("Preburn Color Amount", float) = 1
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		Cull Off
		CGPROGRAM
		#pragma surface surf Lambert addshadow
		#pragma target 3.0

		fixed4 _Color;
		sampler2D _MainTex;
		fixed _SliceScaleX;
		fixed _SliceScaleY;
		sampler2D _SliceGuide;
		sampler2D _BumpMap;
		sampler2D _BurnRamp;
		fixed4 _BurnColor;
		float _BurnSize;
		float _SliceAmount;
		float _EmissionAmount;

		float _Delay;
		float _PreBurnColorAmount;

		struct Input {
			float2 uv_MainTex;
		};


		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			float time = (_SliceAmount - _Delay);

			float preburnAmount = _SliceAmount / _Delay;
			preburnAmount = clamp(preburnAmount, 0, 1);
			c = lerp(c, c * _Color * (1 + _SliceAmount) * _PreBurnColorAmount, preburnAmount);

			if (time > 0)
			{
				float2 noiseScale = float2(_SliceScaleX, _SliceScaleY);
				half test = tex2D(_SliceGuide, IN.uv_MainTex * noiseScale).rgb - (time * (1 + _Delay));
				clip(test);

				if (test < _BurnSize && time > 0) {
					o.Emission = tex2D(_BurnRamp, float2(test * (1 / _BurnSize), 0)) * _BurnColor * _EmissionAmount;
				}
			}

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
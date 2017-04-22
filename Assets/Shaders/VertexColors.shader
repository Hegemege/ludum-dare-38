Shader "Custom/Vertex Colors" {
    Properties {
        _MainColor ("Base color", Color) = (0, 1, 0, 1)
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

		#pragma surface surf Lambert

        sampler2D _MainColor;
 
        struct Input {
            float2 uv_MainColor;
			half4 color : COLOR;
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainColor, IN.uv_MainColor);
            o.Albedo = c.rgb * IN.color.rgb;
            o.Alpha = c.a * IN.color.a;
        }

		ENDCG
    }
    FallBack "Diffuse"
}
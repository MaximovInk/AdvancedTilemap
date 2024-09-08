Shader "ATilemap/Light"
{
    Properties
    {
          _MainTex ("Texture", 2D) = "" {}
            _Intensity ("Intensity",Float) = 0
            _Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Blend One One

        CGPROGRAM
         #pragma surface surf Lambert

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color: Color; 
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Intensity;

        void surf (Input IN, inout SurfaceOutput  o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex).rgba*IN.color;
            fixed3 rgb = c.rgb*_Color.rgb;
            o.Albedo = lerp(fixed3(0,0,0), rgb, _Intensity);
            //o.Alpha = (c.r*c.g*c.b)*IN.color.a;
            o.Alpha = (c.r*c.g*c.b)*IN.color.a;

            /* o.Albedo = lerp(fixed3(0,0,0), rgb, _Intensity);
            o.Alpha = (c.r*c.g*c.b)*IN.color.a;*/

            o.Emission = rgb*_Intensity;
        }
        ENDCG
    }
}

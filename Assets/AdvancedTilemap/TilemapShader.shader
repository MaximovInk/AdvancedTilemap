Shader "ATilemap/TilemapShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

        CGPROGRAM
        #pragma surface surf Standard vertex:vert alpha:blend fullforwardshadows 

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            fixed2 uv_MainTex;
            fixed4 vertexColor;
        };

        struct v2f {
           fixed4 pos : SV_POSITION;
           fixed4 color : COLOR;
        };
 
         void vert (inout appdata_full v, out Input o)
         {
             UNITY_INITIALIZE_OUTPUT(Input,o);

             o.vertexColor = v.color;

         }

        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

         float4 _MainTex_TexelSize;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            c = c * IN.vertexColor;

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

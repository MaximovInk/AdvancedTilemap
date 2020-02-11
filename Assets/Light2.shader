Shader "Custom/Light2"
{
    Properties
    {
          _MainTex ("Texture", 2D) = "" {}
            _Intensity ("Intensity",Float) = 0
    }
    SubShader
    {
        Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
            }
           
            //Lighting Off
                Blend Zero Zero
           

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
         #pragma surface surf Lambert alpha

        // Use shader model 3.0 target, to get nicer looking lighting
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

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput  o)
        {
            fixed4 c = IN.color;
            o.Albedo = lerp(fixed3(0,0,0), c.rgb, _Intensity );
            o.Alpha = c.a;

            o.Emission = c.rgb*_Intensity;
        }
        ENDCG
    }
}

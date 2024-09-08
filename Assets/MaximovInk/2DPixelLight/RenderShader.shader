Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Darkness ("Darkness",Color) = (0,0,0,0.3)
        _Radius ("Radius", float) = 15
        _HSamples ("Horizontal Samples",float) = 10
        _VSamples ("Vertical Samples",float) = 10
    }
    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _Darkness;
            float _Radius;
            float _Intensity;
            float _HSamples;
            float _VSamples;

            float4 _MainTex_ST;

float3 rgb_to_hsv_no_clip(float3 RGB)
{
    float3 HSV;
           
    float minChannel, maxChannel;
    if (RGB.x > RGB.y)
    {
        maxChannel = RGB.x;
        minChannel = RGB.y;
    }
    else
    {
        maxChannel = RGB.y;
        minChannel = RGB.x;
    }
         
    if (RGB.z > maxChannel)
        maxChannel = RGB.z;
    if (RGB.z < minChannel)
        minChannel = RGB.z;
           
    HSV.xy = 0;
    HSV.z = maxChannel;
    float delta = maxChannel - minChannel; //Delta RGB value
    if (delta != 0)
    { // If gray, leave H  S at zero
        HSV.y = delta / HSV.z;
        float3 delRGB;
        delRGB = (HSV.zzz - RGB + 3 * delta) / (6.0 * delta);
        if (RGB.x == HSV.z)
            HSV.x = delRGB.z - delRGB.y;
        else if (RGB.y == HSV.z)
            HSV.x = (1.0 / 3.0) + delRGB.x - delRGB.z;
        else if (RGB.z == HSV.z)
            HSV.x = (2.0 / 3.0) + delRGB.y - delRGB.x;
    }
    return (HSV);
}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                const float2 uv = i.uv;
                
                fixed4 c = tex2D(_MainTex, uv) * _Color;
                
                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                float2 tc = uv;
                const float inv_aspect = _ScreenParams.y / _ScreenParams.x;
                
                for (float index = -0; index < _HSamples; index++)
                {
                    for (float index2 = -0; index2 < _VSamples; index2++)
                    {
                        float2 tc = uv + float2((index / (_HSamples - 1) - 0.5) * _Radius * inv_aspect / 10, (index2 / (_VSamples - 1) - 0.5) * _Radius / 10);
                        sum += tex2D(_MainTex, tc);
                    }

                }
                
                sum = sum / _VSamples / _HSamples;
                
                col.rgb = _Darkness.rgb;
                
                if (sum.r == 0 && sum.g == 0 && sum.b == 0)
                {
                    col.a = _Darkness.a;
                }
                else
                {
                    float3 hsv = rgb_to_hsv_no_clip(sum.rgb);
                    col.a = _Darkness.a - hsv.b;
                }
    
                //col.a = 0;
    
                return col;
            }

            ENDCG
        }
    }
}

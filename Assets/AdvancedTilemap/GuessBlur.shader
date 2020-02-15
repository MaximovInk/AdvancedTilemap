Shader "ATilemap/RenderTexture"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Darkness("Darkness", Color) = (0,0,0,0.3)
        _Radius("Radius", Range(0,30)) = 15
        _Resolution("Resolution", float) = 800  
        _HStep("HorizontalStep", Range(0,1)) = 0.5
        _Vstep("VerticalStep", Range(0,1)) = 0.5  
        
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

        CGPROGRAM

#pragma surface surf Lambert alpha

        sampler2D _MainTex;

        struct Input
{
    float2 uv_MainTex;
};

fixed4 _Color;
fixed4 _Darkness;
float _Radius;
float _Resolution;
float _Intensity;
float _HStep;
float _VStep;
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

    if (RGB.z > maxChannel) maxChannel = RGB.z;
    if (RGB.z < minChannel) minChannel = RGB.z;

    HSV.xy = 0;
    HSV.z = maxChannel;
    float delta = maxChannel - minChannel;             //Delta RGB value
    if (delta != 0)
    {                    // If gray, leave H  S at zero
        HSV.y = delta / HSV.z;
        float3 delRGB;
        delRGB = (HSV.zzz - RGB + 3 * delta) / (6.0 * delta);
        if (RGB.x == HSV.z) HSV.x = delRGB.z - delRGB.y;
        else if (RGB.y == HSV.z) HSV.x = (1.0 / 3.0) + delRGB.x - delRGB.z;
        else if (RGB.z == HSV.z) HSV.x = (2.0 / 3.0) + delRGB.y - delRGB.x;
    }
    return (HSV);
}


void surf(Input IN, inout SurfaceOutput  o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

    float2 uv = IN.uv_MainTex;
    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
    float2 tc = uv;
    float blur = _Radius / _Resolution / 4;
    float invAspect = _ScreenParams.y / _ScreenParams.x;
    
    sum += tex2D(_MainTex, float2(tc.x - 4.0*blur*_HStep, tc.y - 4.0*blur*_VStep/invAspect)) * 0.0162162162;
    sum += tex2D(_MainTex, float2(tc.x - 3.0*blur*_HStep, tc.y - 3.0*blur*_VStep/invAspect)) * 0.0540540541;
    sum += tex2D(_MainTex, float2(tc.x - 2.0*blur*_HStep, tc.y - 2.0*blur*_VStep/invAspect)) * 0.1216216216;
        sum += tex2D(_MainTex, float2(tc.x - 1.0*blur*_HStep, tc.y - 1.0*blur*_VStep/invAspect)) * 0.1945945946;

        sum += tex2D(_MainTex, float2(tc.x, tc.y)) * 0.2270270270;

        sum += tex2D(_MainTex, float2(tc.x + 1.0*blur*_HStep, tc.y + 1.0*blur*_VStep/invAspect)) * 0.1945945946;
        sum += tex2D(_MainTex, float2(tc.x + 2.0*blur*_HStep, tc.y + 2.0*blur*_VStep/invAspect)) * 0.1216216216;
        sum += tex2D(_MainTex, float2(tc.x + 3.0*blur*_HStep, tc.y + 3.0*blur*_VStep/invAspect)) * 0.0540540541;
        sum += tex2D(_MainTex, float2(tc.x + 4.0*blur*_HStep, tc.y + 4.0*blur*_VStep/invAspect)) * 0.0162162162;


    if (sum.r == 0 && sum.g == 0 && sum.b == 0)
    {
        o.Alpha = _Darkness.a;

    }
    else
    {
        float3 hsv = rgb_to_hsv_no_clip(sum.rgb);
        fixed aa = hsv.b;
        o.Alpha = _Darkness.a - aa;

    }
    o.Albedo = (_Darkness.rgb + sum.rgb) / 2;
}
ENDCG
    }
}

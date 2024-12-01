﻿Shader "ATilemap/Diffuse cutout" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 200

CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
    float2 uv_MainTex;
     float3 vertColors;
};

 void vert(inout appdata_full v, out Input o)
    {
      o.vertColors= v.color.rgb;
      o.uv_MainTex = v.texcoord;
 
    }
 
void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb*IN.vertColors;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
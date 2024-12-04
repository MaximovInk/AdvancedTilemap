Shader "ATilemap/Diffuse transparent" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB


CGPROGRAM
#pragma surface surf Lambert vertex:vert alpha

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

Fallback "Legacy Shaders/Transparent/VertexLit"
}
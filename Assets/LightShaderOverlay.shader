
Shader "ATilemap/Lighting/Overlay"
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
           
            Lighting Off
                Blend One OneMinusSrcAlpha
           
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                struct appdata_custom
                {
                    float4 vertex : POSITION;
                    fixed2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };
               
                struct v2f
                {
                    float4 vertex : POSITION;
                    fixed2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };
               
                sampler2D _MainTex;
                fixed4 _MainTex_ST;
                fixed _Intensity;
               
                v2f vert (appdata_custom v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                    o.color = v.color;
                    return o;
                }          
               
                fixed4 frag (v2f i) : COLOR
                {
                    fixed4 diffuse = i.color;
                    fixed luminance =  dot(diffuse, fixed4(0.2126, 0.7152, 0.0722, 0));
                    fixed oldAlpha = diffuse.a;
                   
                    if (luminance < 0.5)
                        diffuse *= 2 * i.color;
                    else
                        diffuse = 1-2*(1-diffuse)*(1-i.color);
                   
                    diffuse.a  = i.color.a;
                    return diffuse;

                    //fixed4 result = i.color;

                  // return result;
                }
                ENDCG
            }
        }
        Fallback off
    }
    
// RingShaderShort.shader
Shader "UI/RingShaderShort"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _RingLineColor ("Ring Line Color", Color) = (0,0,0,1)
        _RingThickness ("Ring Thickness", Range(0, 0.5)) = 0.04
        _RingRadius ("Ring Radius", Range(0, 0.5)) = 0.495
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _RingLineColor;
            float _RingThickness;
            float _RingRadius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UVを中心からの距離に変換 (-1〜1)
                float2 center = i.uv - 0.5;
                float dist = length(center);
                
                // リング部分だけを描画
                float ring = step(_RingRadius - _RingThickness, dist) * step(dist, _RingRadius);
                
                // 色を適用
                return fixed4(_RingLineColor.rgb, _RingLineColor.a * ring);
            }
            ENDCG
        }
    }
}
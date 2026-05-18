Shader "Custom/UI_GaugeThreshold"
{
    Properties
    {
        _Fill ("Fill", Range(0,1)) = 1
        _Threshold ("Threshold", Range(0,1)) = 0.8
        _ColorA ("Color A", Color) = (1,0,0,1) // 赤
        _ColorB ("Color B", Color) = (1,1,0,1) // 黄
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Fill;
            float _Threshold;
            float4 _ColorA;
            float4 _ColorB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fill制御（左→右）
                if (i.uv.x > _Fill)
                    discard;

                // 色切り替え
                if (i.uv.x < _Threshold)
                    return _ColorA;
                else
                    return _ColorB;
            }
            ENDCG
        }
    }
}
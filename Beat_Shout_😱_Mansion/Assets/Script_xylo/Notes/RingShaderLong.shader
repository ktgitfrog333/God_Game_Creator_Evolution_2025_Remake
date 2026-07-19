// RingShaderLong.shader
Shader "UI/RingShaderLong"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // リング線の名称は内側から外側に向けてカウントアップさせて採番すること
        _RingLineColor ("Ring Line Color", Color) = (0,0,0,1)
        _RingThickness ("Ring Thickness", Range(0, 0.5)) = 0.04
        _RingRadius ("Ring Radius", Range(0, 0.5)) = 0.345
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _FillRadius ("Fill Radius", Range(0, 0.5)) = 0.495
        _FillMaskRadius ("Fill Mask Radius", Range(0, 0.4)) = 0.33
        _Ring_1Thickness ("Ring 1 Thickness", Range(0, 0.5)) = 0.04
        _Ring_1Radius ("Ring 1 Radius", Range(0, 0.5)) = 0.495
        
        // ★★★ Fill Amount 用パラメータ ★★★
        _FillAmount ("Fill Amount", Range(0, 1)) = 1.0
        _FillOrigin ("Fill Origin", Range(0, 360)) = 0  // 開始角度（度）
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
            float _FillRadius;
            float _FillMaskRadius;
            float _Ring_1Thickness;
            float _Ring_1Radius;
            float4 _FillColor;

            // ★★★ Fill Amount 用変数 ★★★
            float _FillAmount;
            float _FillOrigin;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ★★★ 時計回りの角度を計算する関数 ★★★
            // UV座標から角度を計算（0度は上方向、時計回り）
            float GetClockwiseAngle(float2 uv)
            {
                float2 center = uv - 0.5;
                float angle = atan2(center.x, center.y); // 上方向を0度に
                angle = angle / 3.14159265; // -1〜1 の範囲に変換
                angle = angle * 180.0;       // -180〜180 の範囲に変換
                
                // 時計回りに変換（-180〜180 → 0〜360）
                angle = angle * -1;
                float clockwiseAngle = angle < 0 ? 360.0 + angle : angle;
                return clockwiseAngle;
            }

            // ★★★ Fill Amount によるマスクを計算 ★★★
            float GetFillMask(float2 uv)
            {
                float angle = GetClockwiseAngle(uv);
                
                // 開始角度をオフセット
                float startAngle = _FillOrigin % 360.0;
                float endAngle = startAngle + (_FillAmount * 360.0);
                
                // 角度が開始〜終了の範囲内かを判定
                if (startAngle <= endAngle)
                {
                    // 通常範囲（開始 < 終了）
                    return (angle >= startAngle && angle <= endAngle) ? 1.0 : 0.0;
                }
                else
                {
                    // 0度をまたぐ場合（開始 > 終了）
                    return (angle >= startAngle || angle <= endAngle) ? 1.0 : 0.0;
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = i.uv - 0.5;
                float dist = length(center);
    
                float fill = step(dist, _FillRadius);
                float mask = step(dist, _FillMaskRadius);
                float ring = step(_RingRadius - _RingThickness, dist) * step(dist, _RingRadius);
                float ring_1 = step(_Ring_1Radius - _Ring_1Thickness, dist) * step(dist, _Ring_1Radius);

                // ★★★ Fill Amount によるマスク ★★★
                float fillAmountMask = GetFillMask(i.uv);

                float4 baseColor = _FillColor * fill;
    
                // 5. マスク領域を透明に（アルファを0にする）
                baseColor.a *= (1 - mask);

                // ★★★ Fill Amount を適用（マスク領域を透明化） ★★★
                baseColor.a *= fillAmountMask;

                float4 result = lerp(baseColor, _RingLineColor, ring_1);
                result = lerp(result, _RingLineColor, ring);

                // ★★★ リング部分にもFill Amountを適用 ★★★
                result.a *= fillAmountMask;
    
                return result;
            }
            ENDCG
        }
    }
}
Shader "Custom/DepthWindShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Sprite Color", Color) = (1,1,1,1)
        _PixelSize ("Pixelation Size", Float) = 32.0
        _WindSpeed ("Wind Speed", Float) = 1.0
        _WindStrength ("Wind Strength", Float) = 0.03
        _SwayAmplitude ("Sway Amplitude", Float) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
			Stencil
            {
                Ref 5
                Comp Always
                Pass Replace
            }

            ZWrite On     //  Must write depth
            ZTest LEqual
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _PixelSize;
            float _WindSpeed;
            float _WindStrength;
            float _SwayAmplitude;

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

            // 2D hash function for randomness
            float2 hash22(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(float2(p.x * p.y, p.x + p.y));
            }

            // Simple Voronoi cell noise returning distance and cell position
            float2 voronoi(float2 uv)
            {
                float2 i_uv = floor(uv);
                float2 f_uv = frac(uv);

                float minDist = 1.0;
                float2 nearestCell = float2(0, 0);

                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellPoint = hash22(i_uv + neighbor) + 0.5;

                        float2 diff = neighbor + cellPoint - f_uv;
                        float dist = length(diff);

                        if(dist < minDist)
                        {
                            minDist = dist;
                            nearestCell = cellPoint + i_uv + neighbor;
                        }
                    }
                }

                return nearestCell;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float time = _Time.y * _WindSpeed;

                // Add side-to-side sway to the whole sprite's vertex position
                // Using vertex.y for slight variation, but you can just use time for uniform sway
                float sway = sin(time + v.vertex.y * 5.0) * _SwayAmplitude;
                float4 displaced = v.vertex;
                displaced.x += sway;

                o.vertex = UnityObjectToClipPos(displaced);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Time variable
                float t = _Time.y * _WindSpeed;

                // Pixelated UV coords
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;

                // Voronoi cell position based on pixel UV + time to animate
                float2 cellPos = voronoi(pixelUV * _PixelSize + float2(t, t));

                // Generate wave offset from cell position + time
                float waveX = sin(t + cellPos.x * 10.0 + cellPos.y * 10.0);
                float waveY = cos(t * 1.3 + cellPos.y * 12.0 + cellPos.x * 8.0);

                // Create offset UV, scaled by wind strength and pixel size (to keep offset relative to pixel)
                float2 uvOffset = float2(waveX, waveY) * _WindStrength / _PixelSize;

                // Apply pixelation + distortion by offsetting pixelated UVs
                float2 finalUV = pixelUV + uvOffset;

                fixed4 texColor = tex2D(_MainTex, finalUV);

                if (texColor.a < 0.01)
                    discard;

                return texColor * _Color;
            }
            ENDCG
        }
    }

    Fallback Off
}

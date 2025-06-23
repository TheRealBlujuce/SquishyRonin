Shader "Custom/SilhouetteWithFallback"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _SilhouetteColor ("Silhouette Color", Color) = (1, 0, 0, 1)
    }

    SubShader
    {
        Tags 
		{ 
			"Queue"="Transparent"
	    	"IgnoreProjector" = "True"
          	"RenderType" = "TransparentCutout"
          	"PreviewType" = "Plane"
          	"CanUseSpriteAtlas" = "True" 
		}

        // Pass 1: Silhouette behind walls (stencil == 5)
        Pass
        {
            Stencil
            {
                Ref 5
                Comp Equal
                Pass Keep
            }

			ZTest Less
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            fixed4 _SilhouetteColor;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                if (texColor.a < 0.01)
                    discard;

                return fixed4(_SilhouetteColor.rgb, texColor.a * _SilhouetteColor.a);
            }
            ENDCG
        }

        // Pass 2: Normal white render where stencil != 4 (i.e., in front)
        Pass
        {
            Stencil
            {
                Ref 5
                Comp NotEqual
                Pass Keep
            }
			ZTest Greater       // Normal ZTest for rendering
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragWhite
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
                return o;
            }

            fixed4 fragWhite(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                if (texColor.a < 0.01)
                    discard;

                // Return white with original alpha to keep shape
                return fixed4(1.0, 1.0, 1.0, texColor.a);
            }
            ENDCG
        }
    }

    Fallback Off
}

// Este archivo crea un shader personalizado para los efectos de glitch en la visión
Shader "Custom/GlitchEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 0
        _InvertX ("Invert X", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _MainTex_ST;
            float _BlurAmount;
            float _InvertX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Aplicar inversión
                if (_InvertX > 0.5)
                    uv.x = 1.0 - uv.x;

                // Aplicar blur simple
                fixed4 col = tex2D(_MainTex, uv);
                
                if (_BlurAmount > 0)
                {
                    float blurStep = _BlurAmount * 0.01;
                    col += tex2D(_MainTex, uv + float2(blurStep, 0));
                    col += tex2D(_MainTex, uv - float2(blurStep, 0));
                    col += tex2D(_MainTex, uv + float2(0, blurStep));
                    col += tex2D(_MainTex, uv - float2(0, blurStep));
                    col /= 5.0;
                }

                return col;
            }
            ENDCG
        }
    }
}

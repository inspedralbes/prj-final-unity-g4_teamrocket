Shader "Custom/DarknessMask"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1) // Color de la oscuridad
        _ConeMask ("Cone Mask", 2D) = "white" {} // Textura de la máscara del cono
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            sampler2D _ConeMask;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lee la máscara del cono
                fixed4 coneMask = tex2D(_ConeMask, i.uv);

                // Si el píxel está dentro del cono, hazlo transparente
                if (coneMask.r > 0.5)
                    return fixed4(0, 0, 0, 0); // Transparente
                else
                    return _Color; // Oscuro
            }
            ENDCG
        }
    }
}
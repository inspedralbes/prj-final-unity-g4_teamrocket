Shader "Custom/DarkwoodVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightAngle ("Light Angle", Range(0, 360)) = 45
        _LightDirection ("Light Direction", Range(0, 360)) = 0 // Dirección del cono
    }
    SubShader
    {
        Tags {"Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

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

            sampler2D _MainTex;
            float _LightAngle;
            float _LightDirection; // Dirección del cono

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 center = float2(0.5, 0.5);
                float angle = atan2(i.uv.y - center.y, i.uv.x - center.x) * 57.2958; // Convierte a grados

                // Ajusta el ángulo para que esté en el rango [0, 360]
                if (angle < 0) angle += 360;

                // Calcula la diferencia entre el ángulo del píxel y la dirección de la luz
                float angleDifference = abs(angle - _LightDirection);
                if (angleDifference > 180) angleDifference = 360 - angleDifference;

                // Si el píxel está dentro del cono de visión, hazlo transparente
                if (angleDifference < _LightAngle / 2)
                    col.a = 0; // Fa transparent la zona de visió
                else
                    col.a = 1; // Manté la foscor

                return col;
            }
            ENDCG
        }
    }
}
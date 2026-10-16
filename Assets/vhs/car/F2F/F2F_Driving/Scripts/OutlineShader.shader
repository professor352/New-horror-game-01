Shader "Custom/InteractRimGlow"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1, 1, 0, 1) 
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0   
        _Intensity ("Intensity", Range(0.5, 5.0)) = 2.0  
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Cull Off 
        Blend SrcAlpha One 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _GlowColor;
            float _RimPower;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal); 
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos); 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);

                
                float dotProduct = abs(dot(viewDir, normal));
                float rim = 1.0 - saturate(dotProduct);
                rim = pow(rim, _RimPower) * _Intensity; 

                return fixed4(_GlowColor.rgb, rim * _GlowColor.a);
            }
            ENDCG
        }
    }
    FallBack Off
}
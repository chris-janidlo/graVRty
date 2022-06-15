Shader "GraVRty/Cone Test"
{
    Properties { }

    SubShader
    {
        Tags { "RenderType"="Opaque" } // in concert with SetReplacementShader, this ensures that this shader only replaces other opaque shaders
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            // properties are prefixed with ConeTest to avoid name collision when calling Shader.SetGlobal<> in scripts
            float ConeTest_Height;
            float ConeTest_BaseRadius;
            float3 ConeTest_TipPosition;
            float3 ConeTest_AxisDirection; // points from tip to base

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 insideColor = fixed4(1, 1, 1, 1);
                fixed4 outsideColor = fixed4(0, 0, 0, 0);

                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // draw a black border around the output so that the light doesn't bleed out of the cookie
                if (
                    screenUV.x <= _ScreenParams.z - 1 ||
                    screenUV.x >= 2 - _ScreenParams.z ||
                    screenUV.y <= _ScreenParams.w - 1 ||
                    screenUV.y >= 2 - _ScreenParams.w
                )
                {
                    return outsideColor;
                }

                float3 difference = i.worldPos - ConeTest_TipPosition;
                float distAlongAxis = dot(difference, ConeTest_AxisDirection);

                if (distAlongAxis < 0 || distAlongAxis > ConeTest_Height)
                {
                    return outsideColor;
                }

                float distFromAxis = length(difference - (distAlongAxis * ConeTest_AxisDirection));
                float radiusAlongAxis = distAlongAxis / ConeTest_Height * ConeTest_BaseRadius;

                return distFromAxis < radiusAlongAxis
                    ? insideColor
                    : outsideColor;
            }
            ENDCG
        }
    }
}

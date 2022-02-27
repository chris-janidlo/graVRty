Shader "GraVRty/Zombie"
{
    Properties
    {
        _Color   ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Bump (A)", 2D) = "white" {}
        _Cutoff  ("Alpha cutoff", Range(0,1)) = 0.5
        _Extrude ("Extrusion Amount", Range(-1,1)) = 0
        _Intrude ("Intrusion Amount", Range(-1, 1)) = 0
        _Lerp    ("Lerp Extremeness", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "RenderType" = "TransparentCutout"
        }
        LOD      100
        Lighting Off
        Cull     Off
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert addshadow
 
        sampler2D _MainTex;
        fixed4 _Color, _MainTex_ST;
        float _Extrude, _Intrude, _Cutoff, _Lerp;
 
        struct Input
        {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v)
        {
            float2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            fixed4 c = tex2Dlod(_MainTex, float4(uv, 0, 0));

            float t = _Lerp * ((c.a + _Cutoff) - .5) + .5;
            float x = lerp(_Intrude, _Extrude, saturate(t));
            v.vertex.xyz += v.normal * x;
        }
 
        void surf (Input IN, inout SurfaceOutput o)
        {
        }
        ENDCG
    }
}

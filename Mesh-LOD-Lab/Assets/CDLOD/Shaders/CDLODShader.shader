Shader "Unlit/CDLODShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;

                float4 DebugColor : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 g_morphConsts;
            float4 g_gridDim;
            float4 g_quadScale;

            float4 g_ObserverPos;

            float g_DebugMorphDistance;

            // morphs vertex xy from from high to low detailed mesh position
            float2 morphVertex(float4 inPos, float2 vertex, float morphLerpK)
            {
                float2 fracPart = (frac(inPos.xz * float2(g_gridDim.y, g_gridDim.y)) * float2(g_gridDim.z, g_gridDim.z)) * g_quadScale.xy;
                return vertex.xy - fracPart * morphLerpK;
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 WorldPos = mul(unity_ObjectToWorld, v.vertex);

                float eyeDist = distance(WorldPos.xyz, g_ObserverPos.xyz);
                float morphLerpK = 1.0f - clamp(g_morphConsts.z - eyeDist * g_morphConsts.w, 0.0, 1.0);

                float4 InPos = v.vertex + float4(0.5f, 0.5f, 0.0f, 0.0f);
                WorldPos.xz = morphVertex(InPos, WorldPos.xz, morphLerpK);
                o.vertex = UnityWorldToClipPos(WorldPos);

                float debugMorphK = 0.5f * (morphLerpK + 1.0f);//saturate(eyeDist / g_DebugMorphDistance);//
                o.DebugColor = float4(debugMorphK, debugMorphK, debugMorphK, 1.0f);


                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return i.DebugColor;
            }
            ENDCG
        }
    }
}

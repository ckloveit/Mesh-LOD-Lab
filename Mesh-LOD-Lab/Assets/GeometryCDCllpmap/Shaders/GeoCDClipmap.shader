Shader "Unlit/GeoCDClipmap"
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
                float3 worldPos : TEXCOORD1;
                float4 DebugColor : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float3 _OceanCenterPosWorld;
            // Morph Need
            uniform float _Scale;
            uniform float _lodAlphaBlackPointFade;
            uniform float _lodAlphaBlackPointWhitePointFade;

            // Geometry data
            // x: A square is formed by 2 triangles in the mesh. Here x is square size
            // yz: normalScrollSpeed0, normalScrollSpeed1
            // w: Geometry density - side length of patch measured in squares
            uniform float4 _GeomData = float4(1.0, 1.0, 1.0, 32.0);
            uniform float4 _GeomDataV2 = float4(1.0, 1.0, 1.0, 32.0);
            // MeshScaleLerp, FarNormalsWeight, LODIndex (debug), unused
            uniform float4 _InstanceData = float4(1.0, 1.0, 0.0, 0.0);

            float ComputeLodAlpha(float3 i_worldPos, float i_meshScaleAlpha, float Scale)
            {
                // taxicab distance from ocean center drives LOD transitions
                float2 offsetFromCenter = abs(float2(i_worldPos.x - _OceanCenterPosWorld.x, i_worldPos.z - _OceanCenterPosWorld.z));
                float taxicab_norm = max(offsetFromCenter.x, offsetFromCenter.y);

                // interpolation factor to next lod (lower density / higher sampling period)
                //const float scale = _CrestCascadeData[_LD_SliceIndex]._scale;
                float lodAlpha = taxicab_norm / Scale - 1.0;

                // LOD alpha is remapped to ensure patches weld together properly. Patches can vary significantly in shape (with
                // strips added and removed), and this variance depends on the base vertex density of the mesh, as this defines the 
                // strip width.
                lodAlpha = max((lodAlpha - _lodAlphaBlackPointFade) / _lodAlphaBlackPointWhitePointFade, 0.);

                // blend out lod0 when viewpoint gains altitude
                lodAlpha = min(lodAlpha + i_meshScaleAlpha, 1.);

                return lodAlpha;
            }

            void SnapAndTransitionVertLayout(float i_meshScaleAlpha, const float gridSize, float Scale, inout float3 io_worldPos, out float o_lodAlpha)
            {
                // Grid includes small "epsilon" to solve numerical issues.
                // :OceanGridPrecisionErrors
                const float GRID_SIZE_2 = 2.000001 * gridSize, GRID_SIZE_4 = 4.0 * gridSize;

                // snap the verts to the grid
                // The snap size should be twice the original size to keep the shape of the eight triangles (otherwise the edge layout changes).
                io_worldPos.xz -= frac(_OceanCenterPosWorld.xz / GRID_SIZE_2) * GRID_SIZE_2; // caution - sign of frac might change in non-hlsl shaders

                // compute lod transition alpha
                o_lodAlpha = ComputeLodAlpha(io_worldPos, i_meshScaleAlpha, Scale);

                // now smoothly transition vert layouts between lod levels - move interior verts inwards towards center
                float2 m = frac(io_worldPos.xz / GRID_SIZE_4); // this always returns positive
                float2 offset = m - 0.5;

                // check if vert is within one square from the center point which the verts move towards
                const float minRadius = 0.26; //0.26 is 0.25 plus a small "epsilon" - should solve numerical issues
                if (abs(offset.x) < minRadius) io_worldPos.x += offset.x * o_lodAlpha * GRID_SIZE_4;
                if (abs(offset.y) < minRadius) io_worldPos.z += offset.y * o_lodAlpha * GRID_SIZE_4;
            }


            v2f vert (appdata v)
            {
                v2f o;

                // see comments above on _GeomData
                const float SQUARE_SIZE = _GeomData.x, SQUARE_SIZE_4 = 4.0 * _GeomData.x;
                const float DENSITY = _GeomData.w;

                float Scale = _GeomDataV2.x;

                // move to world
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                float o_lodAlpha;
                const float meshScaleLerp = _InstanceData.x;
                SnapAndTransitionVertLayout(meshScaleLerp, SQUARE_SIZE, Scale, o.worldPos, o_lodAlpha);

                o.DebugColor = o_lodAlpha;// float4(offset.x, 0.0, 0.0, 1.0f);

                // view-projection	
                o.vertex = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.));

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            { 
                return float4(0.7f, 0.7f, 0.7f, 1.0f);
                return i.DebugColor;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

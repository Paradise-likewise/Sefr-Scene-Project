Shader "Custom/Terrain2SurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Terrain Texture Array", 2DArray) = "white" {}
        _BumpTex ("Bump Texture Array", 2DArray) = "bump"{}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.5 target, to enable texture arrays on all platforms that support it
        #pragma target 3.5

        UNITY_DECLARE_TEX2DARRAY(_MainTex);
        UNITY_DECLARE_TEX2DARRAY(_BumpTex);

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float2 terrainUV;
            float4 terrainType;
        };

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.terrainUV = v.texcoord.xy;
            data.terrainType = v.texcoord1;
        }

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 GetTerrainColor(Input IN, int index)
        {
            float3 uvw = float3(IN.terrainUV * 0.1, IN.terrainType[index]);
            float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
            return c * IN.color[index];
        }

        float3 GetTerrainBump(Input IN, int index)
        {
            float3 uvw = float3(IN.terrainUV * 0.1, IN.terrainType[index]);
            return UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_BumpTex, uvw)) * IN.color[index];
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c =
                GetTerrainColor(IN, 0) +
                GetTerrainColor(IN, 1) +
                GetTerrainColor(IN, 2) +
                GetTerrainColor(IN, 3);

            fixed3 b =
                GetTerrainBump(IN, 0) +
                GetTerrainBump(IN, 1) +
                GetTerrainBump(IN, 2);

            o.Albedo = c.rgb * _Color;
            o.Normal = b;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

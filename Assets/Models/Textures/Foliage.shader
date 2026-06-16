Shader "Custom/InstancedFoliage"
{
    Properties
    {
        _MainTex ("Albedo (RGB) & Alpha (A)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        [Header(Color Variation)]
        _Color1 ("Primary Leaf Color", Color) = (0.3, 0.6, 0.2, 1)
        _Color2 ("Secondary Leaf Color", Color) = (0.5, 0.7, 0.1, 1)

        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.2
        _WindFrequency ("Wind Frequency", Float) = 1.0
    }
    SubShader
    {
        // AlphaTest queue is crucial for performance with foliage
        Tags { "Queue" = "AlphaTest" "RenderType" = "TreeTransparentCutout" "IgnoreProjector" = "True" }
        LOD 200

        // Disable backface culling so leaves are visible from both sides.
        Cull Off

        CGPROGRAM
        // 'addshadow' ensures the vertex displacement is reflected in the shadow caster pass.
        // 'instancing_options:assumeuniformscaling' is a huge optimization if your foliage transforms scale uniformly.
        #pragma surface surf Lambert alphatest:_Cutoff vertex:vert addshadow
        #pragma instancing_options assumeuniformscaling
        #pragma target 3.0

        sampler2D _MainTex;

        float4 _Color1;
        float4 _Color2;
        float _WindSpeed;
        float _WindStrength;
        float _WindFrequency;

        struct Input
        {
            float2 uv_MainTex;
            float randomTint; // Passed from vert to surf to ensure per-instance uniformity
        };

        // Standard GPU Instancing setup
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // Get the origin of this specific instance in world space
            float3 objectOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

            // Generate a pseudo-random value between 0 and 1 based on the object's position.
            // This ensures every tree/cluster gets a locked color variant without breaking batching.
            o.randomTint = frac(sin(dot(objectOrigin, float3(12.9898, 78.233, 45.5432))) * 43758.5453);

            // --- WIND DISPLACEMENT ---
            // Create a staggered wave based on time and the object's world position
            float wavePhase = _Time.y * _WindSpeed + (objectOrigin.x + objectOrigin.z) * _WindFrequency;
            float windOffset = sin(wavePhase) * _WindStrength;

            // Determine how much the vertex should sway.
            // We use the V coordinate of the UVs here (assuming UVs go from 0 at the branch to 1 at the tip).
            // If your meshes use vertex colors (e.g., Red channel) for stiffness, change this to: float swayMask = v.color.r;
            float swayMask = v.texcoord.y;

            // Apply the displacement locally
            v.vertex.x += windOffset * swayMask;
            v.vertex.z += windOffset * swayMask;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 texColor = tex2D (_MainTex, IN.uv_MainTex);

            // Blend between the two chosen colors based on our per-instance random value
            float4 leafColor = lerp(_Color1, _Color2, IN.randomTint);

            o.Albedo = texColor.rgb * leafColor.rgb;
            o.Alpha = texColor.a;
        }
        ENDCG
    }
    // Fallback allows for basic shadow casting on lower-end hardware if needed
    FallBack "Transparent/Cutout/VertexLit"
}
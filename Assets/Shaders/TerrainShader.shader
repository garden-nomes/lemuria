Shader "Custom/TerrainShader"
{
    Properties
    {
        _HeightRamp ("Height Ramp (RGB)", 2D) = "white" {}
        _HeightFrom ("Height ramp starts at y-pos", float) = 0.0
        _HeightTo ("Height ramp ends at y-pos", float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _HeightRamp;

        struct Input
        {
            float2 uv_MainTex;
            float4 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _HeightFrom;
        half _HeightTo;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // sample color from heightmap ramp
            fixed heightRampPos = (IN.worldPos.y - _HeightFrom) / (_HeightTo / _HeightFrom);
            fixed4 heightRamp = tex2D(_HeightRamp, fixed2(heightRampPos, 0));

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * heightRamp * _Color;
            o.Albedo = c.rgb;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "TerrainShaderGUI"
}

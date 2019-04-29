Shader "Custom/Water"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _DepthFactor("Depth Factor", float) = 1.0
        _WaveSpeed("Wave Speed", float) = 1.0
        _WaveAmp("Wave Amplitude", float) = 1.0
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

            #include "UnityCG.cginc"

            float rand(float2 value)  {
                return frac(sin(dot(value, float2(12.9898,78.233))) * 43758.5453);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _CameraDepthTexture;
            float4 _Color;
            float4 _FoamColor;
            float _DepthFactor;
            float _WaveSpeed;
            float _WaveAmp;

            v2f vert(appdata v)
            {
                v2f o;

                float randomValue = rand(v.uv.xy);
                float a = _Time * _WaveSpeed * randomValue;
                float4 offset = float4(cos(a) * _WaveAmp, sin(a) * _WaveAmp, 0, 0);

                o.vertex = UnityObjectToClipPos(v.vertex) + offset;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
                float depth = LinearEyeDepth(depthSample).r - i.screenPos.w;
                return _Color * saturate(depth / _DepthFactor) +
                _FoamColor * (1 - saturate(depth / _DepthFactor));
            }
            ENDCG
        }
    }
}

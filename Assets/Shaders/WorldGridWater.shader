Shader "Custom/WorldGridWater"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.0, 0.67, 1.0, 1.0)
        _GridColor ("Grid Line Color", Color) = (0.35, 0.85, 1.0, 1.0)
        _CellSize ("Grid Cell Size (Meters)", Float) = 2.0
        _LineWidth ("Line Thickness", Range(0.005, 0.15)) = 0.04
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _GridColor;
                float  _CellSize;
                float  _LineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Pure static world-space coordinate grid
                float2 coord = input.positionWS.xz / max(_CellSize, 0.001);
                float2 grid = abs(frac(coord - 0.5) - 0.5);
                float2 dcoord = fwidth(coord);
                float2 lineAA = smoothstep(0.0, dcoord * 1.5, _LineWidth - grid);
                float gridFactor = max(lineAA.x, lineAA.y);

                return lerp(_WaterColor, _GridColor, saturate(gridFactor));
            }
            ENDHLSL
        }
    }
}

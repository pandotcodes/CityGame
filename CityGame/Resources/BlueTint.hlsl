Texture2D inputTexture : register(t0);
SamplerState inputSampler : register(s0);

float4 main(float2 uv : TEXCOORD) : SV_Target
{
    float4 color = inputTexture.Sample(inputSampler, uv);
    float4 grayscaleColor = float4(dot(color.rgb, float3(0.299, 0.587, 0.114)), dot(color.rgb, float3(0.299, 0.587, 0.114)), dot(color.rgb, float3(0.299, 0.587, 0.114)), color.a);
    return grayscaleColor;
}
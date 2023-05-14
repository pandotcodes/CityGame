#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler TextureSampler : register(s0);

float4x4 view_projection;
float2 position;
int size;
int pixelSize;
float timing;
float2 noiseOffset;

float3 color;

#define BLUR_KERNEL_X_MIN -0.5
#define BLUR_KERNEL_X_MAX 0.5
#define BLUR_KERNEL_X_JUMP 0.5
#define BLUR_KERNEL_Y_MIN -0.5
#define BLUR_KERNEL_Y_MAX 0.5
#define BLUR_KERNEL_Y_JUMP 0.5

struct VertexInput {
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct PixelInput {
    float4 Position : SV_Position0;
    float2 RelativePos : TEXCOORD0;
    float2 NoiseTextureCoord : TEXCOORD1;
};

float2 pixelizeCoord(float2 coord)
{
    float2 uv = coord / size;
    float2 scaledUV = uv * pixelSize;
    
    return scaledUV;
}
float2 scalePixelCoordBack(float2 coord)
{
    return round(coord) / pixelSize;
}

PixelInput SpriteVertexShader(VertexInput v) {
    PixelInput output;

    output.RelativePos = pixelizeCoord(v.Position.xy - position);
    output.Position = mul(v.Position, view_projection);
    output.NoiseTextureCoord = (v.TexCoord + noiseOffset) * (pixelSize * 2);
    
    return output;
}
float2 linear_mix(float t, float2 val1, float2 val2)
{
    float2 outval = val1;

    if (val2[0] > 0.5)
        outval[0] = val1[0] + t * (2.0 * (val2[0] - 0.5));
    else
        outval[0] = val1[0] + t * (2.0 * (val2[0]) - 1.0);

    if (val2[1] > 0.5)
        outval[1] = val1[1] + t * (2.0 * (val2[1] - 0.5));
    else
        outval[1] = val1[1] + t * (2.0 * (val2[1]) - 1.0);

    return outval;
}

float2 getNoise(float2 uv)
{
    return tex2D(TextureSampler, uv).rg;
}

float calculateAlpha(float2 uv, float2 noiseUV)
{
    float2 actualUV = scalePixelCoordBack(uv);

    float2 lengthUV = linear_mix(0.5, actualUV, getNoise(actualUV + noiseOffset));
    
    return step(length(lengthUV), timing);
}

float4 SpritePixelShader(PixelInput p) : SV_TARGET {

    float2 noiseCoord = round(p.NoiseTextureCoord) / (pixelSize * 2);
    float2 relPos = abs(p.RelativePos);

    int sampleAmount = 0;
    float val = 0;
    for(float x = BLUR_KERNEL_X_MIN; x <= BLUR_KERNEL_X_MAX; x += BLUR_KERNEL_X_JUMP)
    {
        for(float y = BLUR_KERNEL_X_MIN; y <= BLUR_KERNEL_X_MAX; y += BLUR_KERNEL_Y_JUMP)
        {
            float2 alphaUV = relPos + float2(x,y);
            val += calculateAlpha(alphaUV, noiseCoord);
            sampleAmount++;
        }
    }

    val = val / sampleAmount;
    
    val = val + calculateAlpha(relPos, noiseCoord);
    
    float noiseResult = getNoise(noiseCoord).r;
    float3 resultColor = color * lerp(0.5, 1, noiseResult);
    
    return float4(resultColor,val);
}

technique SpriteBatch {
    pass {
        VertexShader = compile VS_SHADERMODEL SpriteVertexShader();
        PixelShader = compile PS_SHADERMODEL SpritePixelShader();
    }
}

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler textureSampler : register(s0);

#define SAMPLE_COUNT 31
 
float radialBlurLength = 0.5;
 
float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 offsetCoord = texCoord - float2(0.5, 0.5);
    
    float r = length(offsetCoord);
    float theta = atan2(offsetCoord.y, offsetCoord.x);
        
    float4 sum = 0;
    float2 tapCoords = 0;
    
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        float tapTheta = theta + i * (radialBlurLength / SAMPLE_COUNT);
        float tapR = r;
        
        tapCoords.x = (tapR * cos(tapTheta)) + 0.5;
        tapCoords.y = (tapR * sin(tapTheta)) + 0.5;
        
        sum += tex2D(textureSampler, tapCoords);
        //sum += float4(tapCoords.x, tapCoords.y, 0, 1);
    }
    
    sum /= SAMPLE_COUNT;
    return sum;    
}

technique BloomBlur
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
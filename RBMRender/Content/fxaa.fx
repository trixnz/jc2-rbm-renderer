#define FXAA_PC 1
#define FXAA_HLSL_4 1
#define FXAA_GREEN_AS_LUMA 1

#include "Fxaa3_11.fxh"

// Majority of this file is authored by http://mtnphil.wordpress.com/2012/10/15/fxaa-in-xna/.
// All credit should fall on its respective author.

struct PSInputTx
{
	float4 Position	: SV_Position;
	float2 TexCoord : TEXCOORD0;
};

struct VSInput
{
	float4 Position	: SV_Position;
	float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
	float4 Position	: SV_Position;
	float2 TexCoord : TEXCOORD0;
};

extern Texture2D BackTexture : register(t0);
extern SamplerState Sampler : register(s0);


float2 InverseViewportSize;
float4 ConsoleSharpness;
float4 ConsoleOpt1;
float4 ConsoleOpt2;
float SubPixelAliasingRemoval;
float EdgeThreshold;
float EdgeThresholdMin;
float ConsoleEdgeSharpness;

float ConsoleEdgeThreshold;
float ConsoleEdgeThresholdMin;

// Must keep this as constant register instead of an immediate
float4 Console360ConstDir = float4(1.0, -1.0, 0.25, -0.25);

float4 PSBasic_FXAA(VSInput input) : SV_Target
{
	float4 texSample = BackTexture.Sample(Sampler, input.TexCoord);

	FxaaTex samplerState;
	samplerState.smpl = Sampler;
	samplerState.tex = BackTexture;

	float4 value = FxaaPixelShader(
		input.TexCoord,
		0,	// Not used in PC or Xbox 360
		samplerState,
		samplerState,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -1
		samplerState,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -2
		InverseViewportSize,	// FXAA Quality only
		ConsoleSharpness,		// Console only
		ConsoleOpt1,
		ConsoleOpt2,
		SubPixelAliasingRemoval,	// FXAA Quality only
		EdgeThreshold,// FXAA Quality only
		EdgeThresholdMin,
		ConsoleEdgeSharpness,
		ConsoleEdgeThreshold,	// TODO
		ConsoleEdgeThresholdMin, // TODO
		Console360ConstDir
	);

	return value;
}

VSOutput VSBasic(VSInput vin)
{
	VSOutput vout;
	vout.Position = vin.Position;
	vout.TexCoord = vin.TexCoord;

	return vout;
}

technique
{
	pass
	{
		Profile = 10.0;

		VertexShader = VSBasic;
		PixelShader = PSBasic_FXAA;
		GeometryShader = null;
	}
}
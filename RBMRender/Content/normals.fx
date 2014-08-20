struct VSInput
{
	float4 Position	: SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;

	float2 UV		: TexCoord0;
	float2 UV2		: TexCoord1;

	float3 VertexColor : Color0;
	float4 ChannelTextureMask : TexCoord2;
	float2 SpecularPower : TexCoord3;

	float4 BoneWeights : TexCoord4;
	float4 BoneIndices : TexCoord5;
};

struct VSOutput
{
	float4 Position	: SV_Position;
	float4 Normal	: Normal;
	float4 Tangent : Tangent;
	float4 Binormal	: Binormal;
	float4 Color	: Color;
};

struct PSInput
{
	float4 Position	: SV_Position;
	float4 Color	: Color;
};

float4x4 WorldViewProj;

float NormalLength;

VSOutput VSBasic(VSInput vin)
{
	VSOutput vout;

	vout.Position = mul(vin.Position, WorldViewProj);

	vout.Normal = normalize(float4(vin.Normal.xyz, 0));
	vout.Tangent = normalize(float4(vin.Tangent.xyz, 0));

	vout.Binormal = float4(vin.Normal.xyz, 0);
	vout.Binormal = normalize(float4(cross(vout.Tangent, vout.Normal).xyz, 0));

	vout.Normal = mul(vout.Normal, WorldViewProj);
	vout.Binormal = mul(vout.Binormal, WorldViewProj);
	vout.Tangent = mul(vout.Tangent, WorldViewProj);


	vout.Color = float4(1, 1, 0, 1);

	return vout;
}

float4 PSBasic(PSInput input) : SV_Target
{
	return input.Color;
}


[maxvertexcount(6)]
void GS(point VSOutput input[1], inout LineStream<PSInput> stream)
{
	PSInput gout = (PSInput)0;

	// Draw the normal	
	gout.Color = float4(1, 1, 0, 1);
	gout.Position = input[0].Position;
	stream.Append(gout);
	gout.Position = input[0].Position + (input[0].Normal * NormalLength);
	stream.Append(gout);
	
	// Draw the tangent
	gout.Color = float4(0, 1, 0, 1);
	gout.Position = input[0].Position;
	stream.Append(gout);
	gout.Position = input[0].Position + (input[0].Tangent * NormalLength);
	stream.Append(gout);

	// Draw the bi-normal
	gout.Color = float4(0, 0, 1, 1);
	gout.Position = input[0].Position;
	stream.Append(gout);
	gout.Position = input[0].Position + (input[0].Binormal * NormalLength);
	stream.Append(gout);
}

technique
{
	pass
	{
		Profile = 10.0;

		VertexShader = VSBasic;
		PixelShader = PSBasic;
		GeometryShader = GS;
	}
}
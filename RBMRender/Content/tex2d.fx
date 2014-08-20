extern Texture2D DiffuseTexture : register(t0);
extern Texture2D PropertiesTexture : register(t1);
extern Texture2D NormalsTexture : register(t2);
extern Texture2D DeformedDiffuseTexture : register(t3);


extern SamplerState WrappingSampler : register(s0);

float4 Diffuse;
bool IsWindow;

float3 CameraPosition;
float4x4 WorldViewProj;

float3 DiffuseLightDirection = float3(1, -1, 0);

float specularIntensity = 1;
float ambientIntensity = 0.0;

float3 ViewVector = float3(1, 0, 0);

struct PSInput
{
	float4 Position	: SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;
	float3 Binormal	: Binormal;
	float2 UV		: TexCoord0;
	float2 UV2		: TexCoord1;
	float3 VertexColor : Color0;
	float4 ChannelTextureMask : TexCoord2;
	float2 SpecularPower : TexCoord3;
};

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
	float4 Position : SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;
	float3 Binormal	: Binormal;
	float2 UV		: TexCoord0;
	float2 UV2		: TexCoord1;
	float3 VertexColor : Color0;
	float4 ChannelTextureMask : TexCoord2;
	float2 SpecularPower : TexCoord3;
};

float3 bumpMapping(float4 normalTexture, float3 normal, float3 tangent, float3 binormal)
{
	float3 bump = (normalTexture.xyz - float3(0.5, 0.5, 0.5));
	float3 bumpNormal = normal + (-bump.x * tangent + bump.y * binormal);

	return normalize(bumpNormal);
}

float4 PSCarPaint(PSInput input) : SV_Target
{
	// Yeah baby, those fake windows are guuuud.
	if (IsWindow)
		return float4(0, 0, 0, 0.8);

	float4 diffuseTexture = DiffuseTexture.Sample(WrappingSampler, input.UV);
	float4 propertyTexture = PropertiesTexture.Sample(WrappingSampler, input.UV);
	float4 normalTexture = NormalsTexture.Sample(WrappingSampler, input.UV);

	// Calculate the alpha of the diffuse from the texture
	float4 newDiffuse = float4(Diffuse.rgb, diffuseTexture.a);

	// Multiply the Diffuse by the Red channel of the property texture to zero
	// any color where decals, textures, etc might be.
	float4 newDiffuseTex = lerp(diffuseTexture, newDiffuse, propertyTexture.x);

	// Multiply the Diffuse by the Blue channel which handles AO
	//newDiffuseTex *= float4(propertyTexture.zzz, 1);

	// Add the Green channel which handles specularity. This isn't how you should
	// apply it, but it makes the scene look prettier.
	newDiffuseTex += float4(propertyTexture.yyy, 0);

	// If we haven't got any normals (unreversed block?), skip lighting
	if (length(input.Normal) == 0)
		return newDiffuseTex;

	float3 bumpNormal = bumpMapping(normalTexture, input.Normal, input.Tangent, input.Binormal);
	
	// Calculate the diffuse light component with the bump map normal
	float diffuseIntensity = dot(normalize(-DiffuseLightDirection), bumpNormal);
	diffuseIntensity = max(diffuseIntensity, 0);

	float4 lightColor = float4(1, 1, 1, 1);
	float4 ambientColor = lightColor * max(ambientIntensity, 0.1);

	float3 light = normalize(-DiffuseLightDirection);
	float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
	float3 v = normalize(ViewVector);

	float specularFactor = max(dot(r, v), 0);
	float4 specularColor = specularIntensity * lightColor * pow(specularFactor, 16) * diffuseIntensity;

	// Transparent light? No thanks
	specularColor.a = 1;

	return saturate(newDiffuseTex * (diffuseIntensity)+ambientColor + specularColor);
}

float4 getTextureChannel(float4 textureSample, float4 channelMask)
{
	float maskDP = dot(textureSample, channelMask);

	return float4(maskDP.xxx, 1);
}

float4 PSGeneral(PSInput input) : SV_Target
{
	// Horrid attempt at specular lighting
	float4 diffuseTexture = DiffuseTexture.Sample(WrappingSampler, input.UV);
	float4 detailTexture = DeformedDiffuseTexture.Sample(WrappingSampler, input.UV2);
	float4 normalTexture = NormalsTexture.Sample(WrappingSampler, input.UV);
	float4 propertyTexture = PropertiesTexture.Sample(WrappingSampler, input.UV);

	// If the texture is multi-channel (Metal, Wood, etc.)
	// then pick up the appropriate channel.	
	bool hasChannelTextureMask = input.SpecularPower.y > 0;
	if (hasChannelTextureMask)
	{
		diffuseTexture = getTextureChannel(diffuseTexture, input.ChannelTextureMask);
	}
	
	// Now merge that texture with the 'deformed' texture
	diffuseTexture *= detailTexture;

	diffuseTexture *= float4(input.VertexColor.xyz, 1);

	float3 bumpNormal = bumpMapping(normalTexture, input.Normal, input.Tangent, input.Binormal);

	// Calculate the diffuse light component with the bump map normal
	float diffuseIntensity = dot(normalize(-DiffuseLightDirection), bumpNormal);
	diffuseIntensity.x = max(diffuseIntensity, 0);
	 
	float4 lightColor = float4(1, 1, 1, 1);
	float4 ambientColor = lightColor * ambientIntensity;

	float3 light = normalize(-DiffuseLightDirection);
	float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
	float3 v = normalize(ViewVector);

	float specularFactor = max(dot(r, v), 0);
	float4 specularColor = specularIntensity * lightColor * pow(specularFactor, input.SpecularPower.x) * diffuseIntensity;

	// Transparent light? No thanks
	specularColor.a = 1;

	return saturate(diffuseTexture * (diffuseIntensity) + ambientColor + specularColor);
}

struct PSInputSkinned
{
	float4 Position	: SV_Position;
	float2 UV		: TexCoord0;
	float4 BoneWeights : TexCoord1;
	float4 BoneIndices : TexCoord2;
};

struct VSOutputSkinned
{
	float4 Position	: SV_Position;
	float2 UV		: TexCoord0;
	float4 BoneWeights : TexCoord1;
	float4 BoneIndices : TexCoord2;
};

float4x4 BoneMatrices[18];

VSOutputSkinned VSSkinnedGeneral(VSInput vin)
{
	VSOutputSkinned vout;

	float4 inputPos = vin.Position;

	float4 boneWeights = vin.BoneWeights;
	float4 boneIndices = vin.BoneIndices;

	float4 newPos = float4(0, 0, 0, 0);
	for(int i = 0; i < 4; i++)
	{
		newPos += mul(inputPos, BoneMatrices[boneIndices[i]]) * boneWeights[i];
	}
	
	vout.Position = mul(newPos, WorldViewProj);

	vout.UV = vin.UV;
	vout.BoneWeights = vin.BoneWeights; 
	vout.BoneIndices = vin.BoneIndices;

	return vout;
}

float4 PSSkinnedGeneral(PSInputSkinned input) : SV_Target
{
	float4 diffuseTexture = DiffuseTexture.Sample(WrappingSampler, input.UV);

	float4 lightColor = float4(1, 1, 1, 1);
	float4 ambientColor = lightColor * ambientIntensity;
	ambientColor.a = 0;

	return diffuseTexture + ambientColor;
}

VSOutput VSBasicBlock(VSInput vin)
{
	VSOutput vout;
	vout.Position = mul(vin.Position, WorldViewProj);
	
	vout.Normal = normalize(mul(vin.Normal, WorldViewProj));
	vout.Tangent = normalize(mul(vin.Tangent, WorldViewProj));
	vout.Binormal = normalize(mul(cross(vin.Tangent, vin.Normal), WorldViewProj));

	vout.UV = vin.UV;
	vout.UV2 = vin.UV2;
	vout.VertexColor = vin.VertexColor;
	vout.ChannelTextureMask = vin.ChannelTextureMask;	
	vout.SpecularPower = vin.SpecularPower;

	return vout;
}

struct VSBasicInput
{
	float4 Position : SV_Position;
	float4 Colour : Color;
};

struct VSBasicOutput
{
	float4 Position : SV_Position;
	float4 Colour : Color;
};

struct PSBasicInput
{
	float4 Position : SV_Position;
	float4 Colour : Color;
};

VSBasicOutput VSBasic(VSBasicInput input)
{
	VSBasicOutput output;
	output.Position = mul(input.Position, WorldViewProj);
	output.Colour = input.Colour;

	return output;
}

float4 PSBasic(PSBasicInput input) : SV_Target
{
	return input.Colour;
}

technique Basic
{
	pass
	{
		Profile = 10.0;

		Blending = "AlphaBlend";

		VertexShader = VSBasic;
		PixelShader = PSBasic;
		GeometryShader = null;
	}
};

technique CarPaint
{
	pass
	{
		Profile = 10.0;

		Blending = "AlphaBlend";

		VertexShader = VSBasicBlock;
		PixelShader = PSCarPaint;
		GeometryShader = null;
	}
}

technique General
{
	pass
	{
		Profile = 10.0;

		Blending = "AlphaBlend";

		VertexShader = VSBasicBlock;
		PixelShader = PSGeneral;
		GeometryShader = null;
	}
}

technique SkinnedGeneral
{
	pass
	{
		Profile = 10.0;

		Blending = "AlphaBlend";

		VertexShader = VSSkinnedGeneral;
		PixelShader = PSSkinnedGeneral;
		GeometryShader = null;
	}
}
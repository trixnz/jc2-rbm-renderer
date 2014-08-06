extern Texture2D DiffuseTexture : register(t0);
extern SamplerState DiffuseSampler : register(s0);

extern Texture2D PropertiesTexture : register(t1);
extern SamplerState PropertiesSampler : register(s1);

extern Texture2D NormalsTexture : register(t2);
extern SamplerState NormalsSampler : register(s2);

float4 Diffuse;
bool IsWindow;

float4x4 World;
float4x4 WorldViewProj;
 
float3 DiffuseLightDirection = float3(-1, 0, 0);
//float3 DiffuseLightDirection = float3(0, -1, 0);

float3 ViewVector = float3(1, 0, 0);

struct PSInputTx
{
	float4 Position	: SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;
	float3 Binormal	: Binormal;
	float2 UV		: TexCoord0;
};

struct VSInput
{
	float4 Position	: SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;
	float2 UV		: TexCoord0;
};

struct VSOutput
{
	float4 Position : SV_Position;
	float3 Normal	: Normal;
	float3 Tangent	: Tangent;
	float3 Binormal	: Binormal;
	float2 UV		: TexCoord0;
};


float4 PSAdvanced(PSInputTx input) : SV_Target
{
	// Yeah baby, those fake windows are guuuud.
	if (IsWindow)
		return float4(0, 0, 0, 0.8);

	float4 diffuseTexture = DiffuseTexture.Sample(DiffuseSampler, input.UV);
	float4 propertyTexture = PropertiesTexture.Sample(PropertiesSampler, input.UV);
	float4 normalTexture = NormalsTexture.Sample(NormalsSampler, input.UV);

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

	float4 bumpMap = (normalTexture * 2.f) - 1;
	float3 bumpNormal = (-bumpMap.x * input.Tangent) + (bumpMap.y * input.Binormal) + (bumpMap.z * input.Normal);
	bumpNormal = normalize(bumpNormal);

	float lightIntensity = saturate(dot(bumpNormal, -DiffuseLightDirection));

	float4 color = float4(saturate(newDiffuseTex * lightIntensity).xyz, newDiffuseTex.a);

	return color;
}

float4 PSBasic(PSInputTx input) : SV_Target
{
	float4 diffuseColor = float4(0, 1, 0, 1);
	float4 diffuseIntensity = float4(1, 1, 1, 1);

	float4 ambientColor = float4(1, 1, 1, 1);
	float ambientIntensity = 0.1;

	float lightIntensity = dot(input.Normal, -DiffuseLightDirection);

	float4 baseColor = saturate(diffuseColor * diffuseIntensity * lightIntensity);
	//return float4(saturate(baseColor + (ambientColor * ambientIntensity)).xyz, diffuseColor.a);

	float4 diffuseTexture = DiffuseTexture.Sample(DiffuseSampler, input.UV);
	float4 propertyTexture = PropertiesTexture.Sample(PropertiesSampler, input.UV);
	float4 normalTexture = NormalsTexture.Sample(NormalsSampler, input.UV);
	
	return diffuseTexture;
}

VSOutput VSBasic(VSInput vin)
{
	VSOutput vout;
	vout.Position = mul(vin.Position, WorldViewProj);
	vout.Normal = mul(vin.Normal, World);
	vout.Tangent = mul(vin.Tangent, World);

	vout.Binormal = normalize(cross(vin.Normal, vin.Tangent));
	vout.Binormal = mul(vout.Binormal, World);

	vout.UV = vin.UV;
		
	return vout;
}

technique 
{
	pass 
	{
		Profile = 10.0;

		Blending = "AlphaBlend";

		VertexShader = VSBasic;
		PixelShader = PSAdvanced;
	}
}
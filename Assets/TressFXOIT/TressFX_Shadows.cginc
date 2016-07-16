#ifndef TRESSFX_SHADOWS
#define TRESSFX_SHADOWS

// Shadow textures
uniform Texture2D _ShadowMap0;
uniform SamplerComparisonState sampler_ShadowMap0;
uniform Texture2D _ShadowMap1;
uniform SamplerComparisonState sampler_ShadowMap1;
uniform Texture2D _ShadowMap2;
uniform SamplerComparisonState sampler_ShadowMap2;
uniform Texture2D _ShadowMap3;
uniform SamplerComparisonState sampler_ShadowMap3;

// Shadow data
uniform float4x4 _ShadowData[4];
uniform float4x4 _ShadowMatrices[4];
uniform int _ShadowLightCount;

// Texture fake point
uniform Texture2D _TextureFakePoint;
uniform SamplerState sampler_TextureFakePoint;

// Poisson disk sampling
#define CHEAP_SHADOWS
cbuffer POISSON_DISKS {
#ifdef CHEAP_SHADOWS
	static half2 poisson[20] = {
		half2(0.02971195f, 0.8905211f),
		half2(0.2495298f, 0.732075f),
		half2(-0.3469206f, 0.6437836f),
		half2(-0.01878909f, 0.4827394f),
		half2(-0.2725213f, 0.896188f),
		half2(-0.6814336f, 0.6480481f),
		half2(0.4152045f, 0.2794172f),
		half2(0.1310554f, 0.2675925f),
		half2(0.5344744f, 0.5624411f),
		half2(0.8385689f, 0.5137348f),
		half2(0.6045052f, 0.08393857f),
		half2(0.4643163f, 0.8684642f),
		half2(0.335507f, -0.110113f),
		half2(0.03007669f, -0.0007075319f),
		half2(0.8077537f, 0.2551664f),
		half2(-0.1521498f, 0.2429521f),
		half2(-0.2997617f, 0.0234927f),
		half2(0.2587779f, -0.4226915f),
		half2(-0.01448214f, -0.2720358f),
		half2(-0.3937779f, -0.228529f),
	};
#else
	static half2 poisson[40] = {
		half2(0.02971195f, 0.8905211f),
		half2(0.2495298f, 0.732075f),
		half2(-0.3469206f, 0.6437836f),
		half2(-0.01878909f, 0.4827394f),
		half2(-0.2725213f, 0.896188f),
		half2(-0.6814336f, 0.6480481f),
		half2(0.4152045f, 0.2794172f),
		half2(0.1310554f, 0.2675925f),
		half2(0.5344744f, 0.5624411f),
		half2(0.8385689f, 0.5137348f),
		half2(0.6045052f, 0.08393857f),
		half2(0.4643163f, 0.8684642f),
		half2(0.335507f, -0.110113f),
		half2(0.03007669f, -0.0007075319f),
		half2(0.8077537f, 0.2551664f),
		half2(-0.1521498f, 0.2429521f),
		half2(-0.2997617f, 0.0234927f),
		half2(0.2587779f, -0.4226915f),
		half2(-0.01448214f, -0.2720358f),
		half2(-0.3937779f, -0.228529f),
		half2(-0.7833176f, 0.1737299f),
		half2(-0.4447537f, 0.2582748f),
		half2(-0.9030743f, 0.406874f),
		half2(-0.729588f, -0.2115215f),
		half2(-0.5383645f, -0.6681151f),
		half2(-0.07709587f, -0.5395499f),
		half2(-0.3402214f, -0.4782109f),
		half2(-0.5580465f, 0.01399586f),
		half2(-0.105644f, -0.9191031f),
		half2(-0.8343651f, -0.4750755f),
		half2(-0.9959937f, -0.0540134f),
		half2(0.1747736f, -0.936202f),
		half2(-0.3642297f, -0.926432f),
		half2(0.1719682f, -0.6798802f),
		half2(0.4424475f, -0.7744268f),
		half2(0.6849481f, -0.3031401f),
		half2(0.5453879f, -0.5152272f),
		half2(0.9634013f, -0.2050581f),
		half2(0.9907925f, 0.08320642f),
		half2(0.8386722f, -0.5428791f)
	};
#endif
};


float SampleShadow(float3 worldPos, int shadowMapIndex)
{
	// Shadow data matrix layout:
    // Row #0: ("u_UniqueShadowFilterWidth", 0, 0)
	// Row #1: "u_UniqueShadowBlockerWidth"
	// Row #2: ("u_UniqueShadowLightWidth", 0, 0)
    // Row #3: ("u_UniqueShadowBlockerDistanceScale", shadowStrength, 0, 0)

	float2 u_UniqueShadowFilterWidth = _ShadowData[shadowMapIndex][0].xy;
	float4 u_UniqueShadowBlockerWidth = _ShadowData[shadowMapIndex][1].xyzw;
	float2 u_UniqueShadowLightWidth = _ShadowData[shadowMapIndex][2].xy;
	float u_UniqueShadowBlockerDistanceScale = _ShadowData[shadowMapIndex][3].x;
	float shadowStrength = _ShadowData[shadowMapIndex][3].y;

	// Calculate uv 
	float4 uv = mul(_ShadowMatrices[shadowMapIndex], float4(worldPos.xyz, 1));
	float4 smDepth = float4(0, 0, 0, 0);

#ifdef CHEAP_SHADOWS
	float dist = _TextureFakePoint.Sample(sampler_TextureFakePoint, float2(0, 0)).a;

	for (int j = 0; j < 5; ++j)
	{
		const half2 poi = poisson[j + 12];
		const half2 off = poi * u_UniqueShadowBlockerWidth;

		float depth = 0;
		if (shadowMapIndex == 0)
			depth = _ShadowMap0.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		else if (shadowMapIndex == 1)
			depth = _ShadowMap1.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		/*else if (shadowMapIndex == 2)
			depth = _ShadowMap2.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		else if (shadowMapIndex == 3)
			depth = _ShadowMap3.Sample(sampler_TextureFakePoint, uv.xy + off).r;*/

		float d = uv.z - depth;
		dist += max(0.f, d);
	}
	dist *= u_UniqueShadowBlockerDistanceScale;

	half shadow = 0.f;
	for (int i = 0; i < 16; ++i)
	{
		const float c_LightWidth = lerp(u_UniqueShadowLightWidth.x, u_UniqueShadowLightWidth.y, min(1.f, dist));
		const half2 poi = poisson[i];
		const half2 rotPoi = poi;

		const half2 off = rotPoi * c_LightWidth;


		if (shadowMapIndex == 0)
			shadow += _ShadowMap0.SampleCmpLevelZero(sampler_ShadowMap0, uv.xy + off, uv.z);
		else if (shadowMapIndex == 1)
			shadow += _ShadowMap1.SampleCmpLevelZero(sampler_ShadowMap1, uv.xy + off, uv.z);
		/*else if (shadowMapIndex == 2)
			shadow += _ShadowMap2.SampleCmpLevelZero(sampler_ShadowMap2, uv.xy + off, uv.z);
		else if (shadowMapIndex == 3)
			shadow += _ShadowMap3.SampleCmpLevelZero(sampler_ShadowMap3, uv.xy + off, uv.z);*/
	}

	return 1 - ((1 - (shadow / 16.f)) * shadowStrength);
#else
	float dist = _TextureFakePoint.Sample(sampler_TextureFakePoint, float2(0, 0)).a;
	for (int j = 0; j < 10; ++j) {
		const half2 poi = poisson[j + 24];
		const half2 off = poi * u_UniqueShadowBlockerWidth;
		float depth = 0;

		if (shadowMapIndex == 0)
			depth = _ShadowMap0.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		else if (shadowMapIndex == 1)
			depth = _ShadowMap1.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		/*else if (shadowMapIndex == 2)
		depth = _ShadowMap2.Sample(sampler_TextureFakePoint, uv.xy + off).r;
		else if (shadowMapIndex == 3)
		depth = _ShadowMap3.Sample(sampler_TextureFakePoint, uv.xy + off).r;*/

		float d = uv.z - depth;
		dist += max(0.f, d);
	}
	dist *= u_UniqueShadowBlockerDistanceScale;

	half shadow = 0.f;
	for (int i = 0; i < 32; ++i)
	{
		const float c_LightWidth = lerp(u_UniqueShadowLightWidth.x, u_UniqueShadowLightWidth.y, min(1.f, dist));
		const half2 poi = poisson[i];
		const half2 rotPoi = poi;

		const half2 off = rotPoi * c_LightWidth;

		if (shadowMapIndex == 0)
			shadow += _ShadowMap0.SampleCmpLevelZero(sampler_ShadowMap0, uv.xy + off, uv.z);
		else if (shadowMapIndex == 1)
			shadow += _ShadowMap1.SampleCmpLevelZero(sampler_ShadowMap1, uv.xy + off, uv.z);
		/*else if (shadowMapIndex == 2)
		shadow += _ShadowMap2.SampleCmpLevelZero(sampler_ShadowMap2, uv.xy + off, uv.z);
		else if (shadowMapIndex == 3)
		shadow += _ShadowMap3.SampleCmpLevelZero(sampler_ShadowMap3, uv.xy + off, uv.z);*/
	}

	return 1 - ((1 - (shadow / 32.f)) * shadowStrength);
#endif

	return 1;
}

#endif // TRESSFX_SHADOWS
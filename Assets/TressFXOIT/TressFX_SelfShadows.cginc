#ifndef TRESSFX_SELFSHADOWS
#define TRESSFX_SELFSHADOWS
#define SHADOW_E 2.71828183 // Shadow epsilon
#define KERNEL_SIZE 5 // SelfShadow PCF filter range

// Projection informations
uniform float4x4 _SelfShadowMatrix;
uniform float2 _SelfShadowFarNear;

// Shadow map
uniform Texture2D _SelfShadowMap;
uniform float _SelfShadowFiberSpacing;
uniform SamplerState sampler_SelfShadowMap;

//--------------------------------------------------------------------------------------
// ComputeShadow
//
// Computes the shadow using a simplified deep shadow map technique for the hair and
// PCF for scene objects. It uses multiple taps to filter over a (KERNEL_SIZE x KERNEL_SIZE)
// kernel for high quality results.
//--------------------------------------------------------------------------------------
float ComputeShadow(float3 worldPos, float alpha)
{
	float4 projPosLight = mul(_SelfShadowMatrix, float4(worldPos, 1));

	float2 texSM = projPosLight.xy;
	float depth_fragment = projPosLight.z;

	// for shadow casted by scene objs, use PCF shadow
	float total_weight = 0;
	float amountLight_hair = 0;
	float farLight = _SelfShadowFarNear.x;
	float nearLight = _SelfShadowFarNear.y;
	float dist = _TextureFakePoint.Sample(sampler_TextureFakePoint, float2(0, 0)).a;

	total_weight = 0;
	[unroll] for (int dx = (1 - KERNEL_SIZE) / 2; dx <= KERNEL_SIZE / 2; dx++)
	{
		[unroll] for (int dy = (1 - KERNEL_SIZE) / 2; dy <= KERNEL_SIZE / 2; dy++)
		{
			float size = 2.4;
			float sigma = (KERNEL_SIZE / 2.0) / size; // standard deviation, when kernel/2 > 3*sigma, it's close to zero, here we use 1.5 instead
			float exp = -1 * (dx*dx + dy*dy) / (2 * sigma * sigma);
			float weight = 1 / (2 * PI*sigma*sigma) * pow(SHADOW_E, exp);

			// shadow casted by hair: simplified deep shadow map
			float depthSMHair = _SelfShadowMap.Sample(sampler_TextureFakePoint, texSM, int2(dx, dy)).x; //z/w

			float depth_smPoint = depthSMHair; //  nearLight / (1 - depthSMHair*(farLight - nearLight) / farLight);

			float depth_range = max(0, depth_fragment - depth_smPoint);
			float numFibers = depth_range / (_SelfShadowFiberSpacing*(_HairWidth*_HairWidthMultiplier));

			// if occluded by hair, there is at least one fiber
			[flatten]if (depth_range > 1e-5)
				numFibers += 1;
			amountLight_hair += pow(abs(1 - alpha), numFibers)*weight;

			total_weight += weight;
		}
	}
	amountLight_hair /= total_weight;

	return amountLight_hair - abs(dist - 0.5f);
}

#endif // TRESSFX_SELFSHADOWS
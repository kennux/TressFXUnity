#ifndef OIT_COMMON
#define OIT_COMMON

// Helpers
#define NULLPOINTER 0xFFFFFFFF
#define MAX_DEPTH 100000000

// Common fragment data struct for tressfx
struct FragmentData
{
	float depth;
	uint tangentAndCoverage;
	uint next;
	uint color;
};

// Buffers
RWBuffer<uint> fragmentHead : register(u6);
RWStructuredBuffer<FragmentData> fragmentData : register(u7);

// Read-only variants:
Buffer<uint> SRV_fragmentHead;
StructuredBuffer<FragmentData> SRV_fragmentData;

// Helper functions
uint PackFloat4IntoUint(float4 vValue)
{
	return ((uint(vValue.x * 255) & 0xFFUL) << 24) | ((uint(vValue.y * 255) & 0xFFUL) << 16) | ((uint(vValue.z * 255) & 0xFFUL) << 8) | (uint(vValue.w * 255) & 0xFFUL);
}

float4 UnpackUintIntoFloat4(uint uValue)
{
	return float4(((uValue & 0xFF000000) >> 24) / 255.0, ((uValue & 0x00FF0000) >> 16) / 255.0, ((uValue & 0x0000FF00) >> 8) / 255.0, ((uValue & 0x000000FF)) / 255.0);
}

uint PackTangentAndCoverage(float3 tangent, float coverage)
{
	return PackFloat4IntoUint(float4(tangent.xyz*0.5 + 0.5, coverage));
}

float3 GetTangent(uint packedTangent)
{
	return 2.0 * UnpackUintIntoFloat4(packedTangent).xyz - 1.0;
}

float GetCoverage(uint packedCoverage)
{
	return UnpackUintIntoFloat4(packedCoverage).w;
}

// pixelspace position to 1d array index
inline uint GetPixelAddress(uint2 addr)
{
	return ((addr.y * _ScreenParams.x) + addr.x);
}

uint StoreFragment(uint2 address, float depth, float3 tangent, float coverage, float4 color)
{
	// Exchange head pointer
	uint pointer = fragmentData.IncrementCounter();
	uint oldPointer;
	InterlockedExchange(fragmentHead[GetPixelAddress(address)], pointer, oldPointer);

	// Build fragment data
	FragmentData fd = (FragmentData)0;
	fd.depth = depth;
	fd.tangentAndCoverage = PackTangentAndCoverage(tangent, coverage);
	fd.next = oldPointer;
	fd.color = PackFloat4IntoUint(color);

	// Write to buffer
	fragmentData[pointer] = fd;
	return pointer;
}
#endif // OIT_COMMON
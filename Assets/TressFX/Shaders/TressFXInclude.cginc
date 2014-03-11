//The buffer containing the points we want to draw.
StructuredBuffer<float3> g_HairVertexTangents;
StructuredBuffer<float3> g_HairVertexPositions;
StructuredBuffer<int> g_TriangleIndicesBuffer;
StructuredBuffer<float> g_HairThicknessCoeffs;

	//--------------------------------------------------------------------------------------
	// Per-Pixel Linked List (PPLL) structure
	//--------------------------------------------------------------------------------------
	struct PPLL_STRUCT
	{
	    uint	TangentAndCoverage;	
	    uint	depth;
	    uint    uNext;
	};

// Configurations
uniform float4 _HairColor;
uniform float3 g_vEye;
uniform float4 g_WinSize;
uniform float g_FiberRadius;
uniform float g_bExpandPixels;
uniform float g_bThinTip;
uniform matrix g_mInvViewProj;
uniform float g_FiberAlpha;
uniform float g_alphaThreshold;
RWTexture2D<uint> LinkedListHeadUAV : register(t0);
RWStructuredBuffer<PPLL_STRUCT>	LinkedListUAV : register(t1);

//A simple input struct for our pixel shader step containing a position.
struct PS_INPUT_HAIR_AA {
	    float4 Position	: SV_POSITION;
	    float4 Tangent	: Tangent;
	    float4 p0p1		: TEXCOORD0;
};

//----------------------------------------------
// Helper functions
//----------------------------------------------
uint PackFloat4IntoUint(float4 vValue)
{
    return ( (uint(vValue.x*255)& 0xFFUL) << 24 ) | ( (uint(vValue.y*255)& 0xFFUL) << 16 ) | ( (uint(vValue.z*255)& 0xFFUL) << 8) | (uint(vValue.w * 255)& 0xFFUL);
}

float4 UnpackUintIntoFloat4(uint uValue)
{
    return float4( ( (uValue & 0xFF000000)>>24 ) / 255.0, ( (uValue & 0x00FF0000)>>16 ) / 255.0, ( (uValue & 0x0000FF00)>>8 ) / 255.0, ( (uValue & 0x000000FF) ) / 255.0);
}

uint PackTangentAndCoverage(float3 tangent, float coverage)
{
    return PackFloat4IntoUint( float4(tangent.xyz*0.5 + 0.5, coverage) );
}

float3 GetTangent(uint packedTangent)
{
    return 2.0 * UnpackUintIntoFloat4(packedTangent).xyz - 1.0;
}

float GetCoverage(uint packedCoverage)
{
    return UnpackUintIntoFloat4(packedCoverage).w;
}

//--------------------------------------------------------------------------------------
// ComputeCoverage
//
// Calculate the pixel coverage of a hair strand by computing the hair width
//--------------------------------------------------------------------------------------
float ComputeCoverage(float2 p0, float2 p1, float2 pixelLoc)
{
	// p0, p1, pixelLoc are in d3d clip space (-1 to 1)x(-1 to 1)

	// Scale positions so 1.f = half pixel width
	p0 *= g_WinSize.xy;
	p1 *= g_WinSize.xy;
	pixelLoc *= g_WinSize.xy;

	float p0dist = length(p0 - pixelLoc);
	float p1dist = length(p1 - pixelLoc);
	float hairWidth = length(p0 - p1);
    
	// will be 1.f if pixel outside hair, 0.f if pixel inside hair
	float outside = any( float2(step(hairWidth, p0dist), step(hairWidth, p1dist)) );
	
	// if outside, set sign to -1, else set sign to 1
	float sign = outside > 0.f ? -1.f : 1.f;
	
	// signed distance (positive if inside hair, negative if outside hair)
	float relDist = sign * saturate( min(p0dist, p1dist) );
	
	// returns coverage based on the relative distance
	// 0, if completely outside hair edge
	// 1, if completely inside hair edge
	return (relDist + 1.f) * 0.5f;
}

void StoreFragments_Hair(uint2 address, float3 tangent, float coverage, float depth)
{
    // Retrieve current pixel count and increase counter
    uint uPixelCount = LinkedListUAV.IncrementCounter();
    uint uOldStartOffset;

    // Exchange indices in LinkedListHead texture corresponding to pixel location 
    InterlockedExchange(LinkedListHeadUAV[address], uPixelCount, uOldStartOffset);  // link head texture

    // Append new element at the end of the Fragment and Link Buffer
    PPLL_STRUCT Element;
	Element.TangentAndCoverage = PackTangentAndCoverage(tangent, coverage);
	Element.depth = asuint(depth);
    Element.uNext = uOldStartOffset;
    LinkedListUAV[uPixelCount] = Element; // buffer that stores the fragments
}
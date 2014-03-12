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

// K-Buffer struct
struct KBuffer_STRUCT
{
    uint2	depthAndPackedtangent;
};

//A simple input struct for our pixel shader step containing a position.
struct PS_INPUT_HAIR_AA {
	    float4 Position	: SV_POSITION;
	    float4 Tangent	: Tangent;
	    float4 p0p1		: TEXCOORD0;
};

struct VS_OUTPUT_SCREENQUAD
{
    float4 vPosition : SV_POSITION;
    float2 vTex      : TEXCOORD;
};

struct VS_INPUT_SCREENQUAD
{
    float3 Position     : POSITION;		// vertex position 
    float3 Normal       : NORMAL;		// this normal comes in per-vertex
    float2 Texcoord	    : TEXCOORD0;	// vertex texture coords 
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


//--------------------------------------------------------------------------------------
// ComputeHairShading
//
// Hair shading using dual highlight approach and Kajiya lighting model
// dual highlight: marschner 03 
// kajiya model: kajiya 84
//--------------------------------------------------------------------------------------
float3 ComputeHairShading(float3 iPos, float3 iTangent, float4 iTex, float amountLight)
{
    /*float3 baseColor = g_MatBaseColor.xyz;
    float rand_value = 1;
    
    if(abs(iTex.x) + abs(iTex.y) >1e-5) // if texcoord is available, use texture map
        rand_value = g_txNoise.SampleLevel(g_samLinearWrap, iTex.xy, 0).x;
    
    // define baseColor and Ka Kd Ks coefficient for hair
    float Ka = g_MatKValue.x, Kd = g_MatKValue.y, 
          Ks1 = g_MatKValue.z, Ex1 = g_MatKValue.w,
          Ks2 = g_fHairKs2, Ex2 = g_fHairEx2;

    float3 lightPos = g_PointLightPos.xyz;
    float3 vLightDir = normalize(lightPos - iPos.xyz);
    float3 vEyeDir = normalize(g_vEye.xyz - iPos.xyz);
    float3 tangent = normalize(iTangent);

    // in Kajiya's model: diffuse component: sin(t, l)
    float cosTL = (dot(tangent, vLightDir));
    float sinTL = sqrt(1 - cosTL*cosTL);
    float diffuse = sinTL; // here sinTL is apparently larger than 0

    float alpha = (rand_value * 10) * PI/180; // tiled angle (5-10 dgree)

    // in Kajiya's model: specular component: cos(t, rl) * cos(t, e) + sin(t, rl)sin(t, e)
    float cosTRL = -cosTL;
    float sinTRL = sinTL;
    float cosTE = (dot(tangent, vEyeDir));
    float sinTE = sqrt(1- cosTE*cosTE);

    // primary highlight: reflected direction shift towards root (2 * Alpha)
    float cosTRL_root = cosTRL * cos(2 * alpha) - sinTRL * sin(2 * alpha);
    float sinTRL_root = sqrt(1 - cosTRL_root * cosTRL_root);
    float specular_root = max(0, cosTRL_root * cosTE + sinTRL_root * sinTE);

    // secondary highlight: reflected direction shifted toward tip (3*Alpha)
    float cosTRL_tip = cosTRL*cos(-3*alpha) - sinTRL*sin(-3*alpha);
    float sinTRL_tip = sqrt(1 - cosTRL_tip * cosTRL_tip);
    float specular_tip = max(0, cosTRL_tip * cosTE + sinTRL_tip * sinTE);

    float3 vColor = Ka * g_AmbientLightColor.xyz * baseColor + // ambient
                    amountLight * g_PointLightColor.xyz * (
                    Kd * diffuse * baseColor + // diffuse
                    Ks1 * pow(specular_root, Ex1)  + // primary hightlight r
                    Ks2 * pow(specular_tip, Ex2) * baseColor); // secondary highlight rtr */

   return _HairColor; // vColor;
}

//--------------------------------------------------------------------------------------
// SimpleHairShading
//
// Low quality, but faster hair shading
//--------------------------------------------------------------------------------------
float3 SimpleHairShading(float3 iPos, float3 iTangent, float4 iTex, float amountLight)
{
    
    /*float3 baseColor = g_MatBaseColor.xyz;
 	float Kd = g_MatKValue.y;
   
#ifdef SUPERSIMPLESHADING
	float3 vColor = amountLight * Kd * baseColor;
#else
    // define baseColor and Ka Kd Ks coefficient for hair
    float Ka = g_MatKValue.x;
	float Ks1 = g_MatKValue.z;
	float Ex1 = g_MatKValue.w;
	float Ks2 = g_fHairKs2;
	float Ex2 = g_fHairEx2;

    float3 lightPos = g_PointLightPos.xyz;
    float3 vLightDir = normalize(lightPos - iPos.xyz);
    float3 tangent = normalize(iTangent);

    // in Kajiya's model: diffuse component: sin(t, l)
    float cosTL = (dot(tangent, vLightDir));
    float sinTL = sqrt(1 - cosTL*cosTL);
    float diffuse = sinTL; // here sinTL is apparently larger than 0

    float3 vColor = Ka * g_AmbientLightColor.xyz * baseColor +							// ambient
                    amountLight * g_PointLightColor.xyz * (Kd * diffuse * baseColor);	// diffuse
#endif*/

    return _HairColor; // vColor;
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
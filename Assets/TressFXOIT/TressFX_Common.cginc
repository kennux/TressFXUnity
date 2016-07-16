#ifndef TRESSFX_COMMON
#define TRESSFX_COMMON

// Helper uniforms
uniform float4x4 _TFX_World2Object;
uniform float4x4 _TFX_ScaleMatrix;
uniform float4x4 _TFX_Object2World;
uniform float4 _TFX_PositionOffset;

// Buffers
StructuredBuffer<float3> HairVertexTangents;
StructuredBuffer<float3> HairVertexPositions;
StructuredBuffer<int> TriangleIndicesBuffer;
StructuredBuffer<float> HairThicknessCoeffs;
StructuredBuffer<float4> TexCoords;
StructuredBuffer<int> LineIndicesBuffer;

// Geometric parameters
uniform float _HairWidth;
uniform float _HairWidthMultiplier;
uniform float _LoDHairWidthMultiplier;
uniform float _ThinTip;

// Vertex 2 fragment struct
struct v2f
{
	float4 pos : SV_POSITION;
	fixed3 tangent : TANGENT;
	fixed2 texcoord : TEXCOORD1;
	float4 pixelspace : TEXCOORD2;
	float4 p0p1 : TEXCOORD3;
	float4 projSpace : TEXCOORD4;
};


// Inlined Helper functions
inline float GetHairWidth()
{
	return _HairWidth*_HairWidthMultiplier;// *_LoDHairWidthMultiplier;
}

inline float3 GetVertexPosition(uint index)
{
	float3 vert = mul(_TFX_World2Object, float4(HairVertexPositions[index].xyz - _TFX_PositionOffset, 1)).xyz;
	vert = mul(_TFX_ScaleMatrix, float4(vert, 1)).xyz;
	vert = mul(_TFX_Object2World, float4(vert, 1)).xyz;
	return vert;
}

// Reconstruct world pos from normalized screen pos and depth
inline float3 ReconstructWorldPos(float2 screenPos, float depth, float3 ray, out float4 vpos)
{
	// Scale ray
	ray = ray * (_ProjectionParams.z / ray.z);

	// Calculate view space
	vpos = float4(ray * Linear01Depth(depth), 1);

	// Transform into worldspace
	float3 wpos = mul(unity_CameraToWorld, vpos).xyz;

	return wpos;
}

v2f VertBase(uint vertexIndexId)
{
	// Initialize the output
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	// Vertex calculation
	uint vertexId = TriangleIndicesBuffer[vertexIndexId];

	// Access the current line segment
	uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
	float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;

	// Get updated positions and tangents from simulation result
	float3 t = HairVertexTangents[index].xyz;
	float3 vert = GetVertexPosition(index);
	float hairWidth = GetHairWidth();

	float ratio = (_ThinTip > 0) ? HairThicknessCoeffs[index] : 1.0;

	// Calculate right vector
	float3 right = normalize(cross(t, normalize(vert - _WorldSpaceCameraPos)));
	float3 offset = right * (hairWidth * ratio);

	// Calculate final vertex position
	float4 vertex = float4(vert + ((right * (hairWidth * ratio))  * fDirIndex), 1);
	o.pos = mul(UNITY_MATRIX_MVP, vertex);
	o.pixelspace = ComputeScreenPos(o.pos);
	o.projSpace = o.pos / o.pos.w;

	// Calculate edge positions
	float4 edges[2];
	edges[0] = float4(vert - offset, 1);
	edges[1] = float4(vert + offset, 1);

	// Calculate p0p1
	edges[0] = mul(UNITY_MATRIX_VP, edges[0]);
	edges[1] = mul(UNITY_MATRIX_VP, edges[1]);
	edges[0] = edges[0] / edges[0].w;
	edges[1] = edges[1] / edges[1].w;
	o.p0p1 = float4(edges[0].xy, edges[1].xy);

	// Set texcoords
	o.texcoord.xy = fixed2(fDirIndex == -1.0 ? 0 : 1, (vertexId % 16) / 16.0); // TexCoords[vertexId].xy;
	o.tangent = t;

	return o;
}

#endif // TRESSFX_COMMON
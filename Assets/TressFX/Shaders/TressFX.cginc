#include "UnityPBSLighting.cginc"

// Material information
uniform float g_MatKd;
uniform float g_MatKs1;
uniform float g_MatEx1;
uniform float g_MatKs2;
uniform float g_MatEx2;
uniform sampler2D _RandomTex;

// Helper matrices
uniform float4x4 _TFX_World2Object;
uniform float4x4 _TFX_ScaleMatrix;
uniform float4x4 _TFX_Object2World;
uniform float4 _TFX_PositionOffset;

struct SurfaceOutputKajiyaKay
{
	fixed3 iTangent;
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Specular;
	half3 Emission;
	fixed SpecularEx1;
	fixed SpecularEx2;
	fixed Alpha;
	float Random;
};
		
#define PI 3.1415926
		
#ifdef SHADER_API_D3D11
	// All needed buffers
	StructuredBuffer<float3> g_HairVertexTangents;
	StructuredBuffer<float3> g_HairVertexPositions;
	StructuredBuffer<int> g_TriangleIndicesBuffer;
	StructuredBuffer<float> g_HairThicknessCoeffs;
	StructuredBuffer<float4> g_TexCoords;
#endif

// Vertex index -> hair position (scaled)
inline float3 GetVertexPosition(uint index)
{
	float3 vert = mul(_TFX_World2Object, float4(g_HairVertexPositions[index].xyz - _TFX_PositionOffset.xyz, 1)).xyz;
	vert = mul(_TFX_ScaleMatrix, float4(vert, 1)).xyz;
	vert = mul(_TFX_Object2World, float4(vert, 1)).xyz;
	return vert;
}

inline fixed4 KajiyaKayLighting(SurfaceOutputKajiyaKay s, UnityLight light)
{
	fixed amountLight = light.ndotl;
	
    float3 baseColor = s.Albedo;
    float3 vEyeDir = normalize( _WorldSpaceCameraPos - light.dir );
    float3 tangent = normalize(s.iTangent);
    
    // Sample random value
    float rand_value = s.Random;

    // in Kajiya's model: diffuse component: sin(t, l)
    float cosTL = (dot(tangent, light.dir));
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

//    float3 vColor = g_MatKa * (UNITY_LIGHTMODEL_AMBIENT.xyz * UNITY_LIGHTMODEL_AMBIENT.a) * baseColor + // ambient
//                    amountLight * light.color.xyz * (
//                    g_MatKd * diffuse * baseColor + // diffuse
//                    g_MatKs1 * pow(specular_root, (g_MatEx1 * s.SpecularEx1))  + // primary hightlight r
//                    g_MatKs2 * pow(specular_tip, (g_MatEx2 * s.SpecularEx2)) * baseColor); // secondary highlight rtr 

    float3 vColor = UNITY_LIGHTMODEL_AMBIENT.rgb * baseColor + // ambient * base
					amountLight * light.color.xyz * (
                    g_MatKd * diffuse * baseColor + // diffuse
                    g_MatKs1 * pow(specular_root, (g_MatEx1 * s.SpecularEx1))  + // primary hightlight r
                    g_MatKs2 * pow(specular_tip, (g_MatEx2 * s.SpecularEx2)) * s.Specular); // secondary highlight rtr 

   return fixed4(vColor, 1);
}

//--------------------------------------------------------------------------------------
// LightingKajiyaKay
//
// Hair shading using dual highlight approach and Kajiya lighting model
// dual highlight: marschner 03 
// kajiya model: kajiya 84
//--------------------------------------------------------------------------------------
inline fixed4 LightingKajiyaKay(SurfaceOutputKajiyaKay s, UnityGI gi)
{
	fixed4 c = KajiyaKayLighting (s, gi.light);

	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += KajiyaKayLighting (s, gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += KajiyaKayLighting (s, gi.light3);
		#endif
	#endif

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += (s.Albedo * gi.indirect.diffuse) + (s.Specular * gi.indirect.specular);
	#endif
	
	return c;
}

inline void LightingKajiyaKay_GI
(
	SurfaceOutputKajiyaKay s,
	UnityGIInput data,
	inout UnityGI gi
)
{
	gi = UnityGlobalIllumination (data, 0.0f, 1.0f, s.Normal);
	gi.light.ndotl = data.atten;
}

//--------------------------------------------------------------------------------------
// LightingKajiyaKay
//
// Hair shading using dual highlight approach and Kajiya lighting model
// dual highlight: marschner 03 
// kajiya model: kajiya 84
//--------------------------------------------------------------------------------------
/*inline fixed4 LightingKajiyaKay_Deferred(SurfaceOutputKajiyaKay s, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
{
	outDiffuseOcclusion = half4(s.Albedo, 0);
	outSpecSmoothness = s.SpecularEx1;
	outNormal = half4(s.Normal, 1); // half4(s.Normal * 0.5 + 0.5, 1);
	
	fixed4 c = KajiyaKayLighting (s, gi.light);

	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += KajiyaKayLighting (s, gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += KajiyaKayLighting (s, gi.light3);
		#endif
	#endif

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += (s.Albedo * gi.indirect.diffuse) + (s.Specular * gi.indirect.specular);
	#endif
	
	return c;
	
//	s.Albedo = KajiyaKayLighting (s, gi.light);
//	
//	half oneMinusReflectivity = 1.0 - s.Reflectivity;
//	half oneMinusRoughness = 1.0 - s.Roughness;
//
//	half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular.rgb, oneMinusReflectivity, oneMinusRoughness ,s.Normal, s.viewDir, gi.light, gi.indirect);
//	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular.rgb, oneMinusReflectivity, oneMinusRoughness , s.Normal, s.viewDir, 0, gi);
//	
//   return fixed4(c.rgb, 1);
}*/
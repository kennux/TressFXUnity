#ifndef TRESSFX_LIGHTING
#define TRESSFX_LIGHTING

#define PI 3.1415926
#define MAX_LIGHTS 2

uniform float _Kajiya_Diffuse;
uniform float _Kajiya_PrimaryShift;
uniform float _Kajiya_SecondaryShift;
uniform float _Kajiya_PrimaryHighlight;
uniform float _Kajiya_SecondaryHighlight;

// XYZ = Position / Direction (only directional light)
// W = 0 -> Directional light, range -> point light
uniform float4 _LightPositions[MAX_LIGHTS];
uniform float4 _LightColors[MAX_LIGHTS];
uniform float4 _LightDatas[MAX_LIGHTS];
uniform int _LightCount;

// Kajiya-Kay lighting model
inline float3 KajiyaKay(fixed amountLight, float3 lightDir, float3 tangent, float3 lightColor, float3 vEyeDir, float3 hairColor)
{
	// Sample random value
	float rand_value = 1;

	// in Kajiya's model: diffuse component: sin(t, l)
	float cosTL = saturate(dot(tangent, lightDir));
	float sinTL = sqrt(1 - cosTL*cosTL);
	float diffuse = sinTL; // here sinTL is apparently larger than 0

	float alpha = (rand_value * 10) * PI / 180; // tiled angle (5-10 dgree)

	// in Kajiya's model: specular component: cos(t, rl) * cos(t, e) + sin(t, rl)sin(t, e)
	float cosTRL = -cosTL;
	float sinTRL = sinTL;
	float cosTE = (dot(tangent, vEyeDir));
	float sinTE = sqrt(1 - cosTE*cosTE);

	// primary highlight: reflected direction shift towards root (2 * Alpha)
	float cosTRL_root = cosTRL * cos(2 * alpha) - sinTRL * sin(2 * alpha);
	float sinTRL_root = sqrt(1 - cosTRL_root * cosTRL_root);
	float specular_root = max(0, cosTRL_root * cosTE + sinTRL_root * sinTE);

	// secondary highlight: reflected direction shifted toward tip (3*Alpha)
	float cosTRL_tip = cosTRL*cos(-3 * alpha) - sinTRL*sin(-3 * alpha);
	float sinTRL_tip = sqrt(1 - cosTRL_tip * cosTRL_tip);
	float specular_tip = max(0, cosTRL_tip * cosTE + sinTRL_tip * sinTE);

	return amountLight * lightColor * (
		_Kajiya_Diffuse * diffuse * hairColor + // diffuse
		_Kajiya_PrimaryShift * pow(specular_root, _Kajiya_PrimaryHighlight) + // primary hightlight r
		_Kajiya_SecondaryShift * pow(specular_tip, _Kajiya_SecondaryHighlight) * hairColor.rgb); // secondary highlight rtr
}

#endif // TRESSFX_LIGHTING
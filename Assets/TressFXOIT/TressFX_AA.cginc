#ifndef TRESSFX_AA
#define TRESSFX_AA

float ComputeCoverage(float2 p0, float2 p1, float2 pixelLoc)
{
	// p0, p1, pixelLoc are in d3d clip space (-1 to 1)x(-1 to 1)

	// Scale positions so 1.f = half pixel width
	p0 *= _ScreenParams.xy;
	p1 *= _ScreenParams.xy;
	pixelLoc *= _ScreenParams.xy;

	float p0dist = length(p0 - pixelLoc);
	float p1dist = length(p1 - pixelLoc);
	float hairWidth = length(p0 - p1);

	// will be 1.f if pixel outside hair, 0.f if pixel inside hair
	float outside = any(float2(step(hairWidth, p0dist), step(hairWidth, p1dist)));

	// if outside, set sign to -1, else set sign to 1
	float sign = outside > 0.f ? -1.f : 1.f;

	// signed distance (positive if inside hair, negative if outside hair)
	float relDist = sign * saturate(min(p0dist, p1dist));

	// returns coverage based on the relative distance
	// 0, if completely outside hair edge
	// 1, if completely inside hair edge
	return (relDist + 1.f) * 0.5f;
}

#endif // TRESSFX_AA
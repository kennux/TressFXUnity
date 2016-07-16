Shader "Unlit/TressFXOIT"
{
	Properties
	{
		_HairColor("Hair color", Color) = (1,1,1,1)
		_AlphaThreshold("Alpha threshold", Range(0,1)) = 0.4
		_HairWidth("Hair fiber width", Range(0,1)) = 0.14
		_HairWidthMultiplier("Hair fiber width multiplier", Range(0,1)) = 1
		_ThinTip("Thin tip (>0 means enabled)", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ZTest LEqual
		ColorMask 0
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "../../OIT_Common.cginc"
			#include "../../TressFX_Common.cginc"
			#include "../../TressFX_AA.cginc"

			uniform float4 _HairColor;
			uniform float _AlphaThreshold;
			
			v2f vert(uint vertexId : SV_VertexID)
			{
				return VertBase(vertexId);
			}
			
			[earlydepthstencil]
			fixed4 frag (v2f IN) : SV_Target
			{
				IN.pixelspace.xy = (IN.pixelspace.xy / IN.pixelspace.w);

				// Compute coverage and store in buffer
				IN.p0p1.y = IN.p0p1.y * _ProjectionParams.x;
				IN.p0p1.w = IN.p0p1.w * _ProjectionParams.x;
				float coverage = ComputeCoverage(IN.p0p1.xy, IN.p0p1.zw, IN.projSpace.xy);
				uint tmp = 0;
				if (coverage > _AlphaThreshold)
				{
					tmp = StoreFragment(uint2(IN.pixelspace.xy * _ScreenParams.xy), IN.pos.z, IN.tangent.xyz, coverage, _HairColor);
				}

				return float4(coverage, coverage, coverage, 1);
			}
			ENDCG
		}
	}
}

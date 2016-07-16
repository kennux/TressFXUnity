Shader "Hidden/Evaluation"
{
	Properties
	{
		_Kajiya_Diffuse("Diffuse Strength", Range(0, 1)) = 0.4
		_Kajiya_PrimaryShift("Primary highlight shift", Range(0, 1)) = 0.14
		_Kajiya_PrimaryHighlight("Primary highlight strength", Range(0, 100)) = 80
		_Kajiya_SecondaryHighlight("Secondary highlight strength", Range(0, 10)) = 8
		_Kajiya_SecondaryShift("Secondary highlight shift", Range(0, 1)) = 0.5
		_SelfShadowStrength("Self-Shadow Strength", Range(0,1)) = 0.75
	}
		SubShader
	{
		// No culling or depth
		ZWrite On
		ZTest Always
		Blend One OneMinusSrcAlpha
		BlendOp Add
		Cull Off
		// ColorMask 0

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../../OIT_Common.cginc"
			#include "../../TressFX_Common.cginc"
			#include "../../TressFX_Lighting.cginc"
			#include "../../TressFX_Shadows.cginc"
			#include "../../TressFX_SelfShadows.cginc"
			#define KBUFFER_SIZE 8
			#define MAX_FRAGMENTS 128
			//#define COLORDEBUG

			uniform float _SelfShadowStrength;
			uniform int _SelfShadows;

			struct SortedFragmentData
			{
				float depth;
				uint tangentAndCoverage;
				uint color;
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 ray : NORMAL;
			};

			struct v2f_eval
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 ray : NORMAL;
				float4 screenPos : TEXCOORD1;
			};

			v2f_eval vert(appdata v)
			{
				v2f_eval o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				o.ray = v.ray;
				o.screenPos = ComputeScreenPos(o.vertex);

				return o;
			}

			fixed4 frag(v2f_eval IN) : SV_Target
			{
				// Initialize
				IN.screenPos.xy /= IN.screenPos.w;
				uint2 pixelPos = uint2(IN.screenPos.xy * _ScreenParams.xy);
				uint pointer = SRV_fragmentHead[GetPixelAddress(pixelPos)];

				// Not occupied? Sad...
				if (pointer == NULLPOINTER)
				{
					clip(-1);
					return float4(0, 0, 0, 0);
				}

				// Init counter and color
				uint fragmentCount = 0;
				uint i = 0;

				// Initialize K-Buffer
				SortedFragmentData kBuffer[KBUFFER_SIZE];

				[unroll]
				for (int t = 0; t<KBUFFER_SIZE; t++)
				{
					if (pointer == NULLPOINTER)
					{
						kBuffer[t] = (SortedFragmentData)0;
						kBuffer[t].depth = 1000000;	// must be larger than the maximum possible depth value
						kBuffer[t].tangentAndCoverage = 0;
						kBuffer[t].color = 0;
					}
					else
					{
						FragmentData e = SRV_fragmentData[pointer];

						kBuffer[t].depth = e.depth;
						kBuffer[t].tangentAndCoverage = e.tangentAndCoverage;
						kBuffer[t].color = e.color;

						pointer = e.next;
						fragmentCount++;
					}
				}

				// Declarations for variables inside the loop
				float max_depth = 0;
				int id = 0;
				float4 fragmentColor = float4(0, 0, 0, 1);
				float4 finalColor = float4(0, 0, 0, 1);

				// Initial find furthest fragment in k-buffer
				max_depth = 0;
				[unroll]for (uint j = 0; j < KBUFFER_SIZE; j++)
				{
					if (max_depth < kBuffer[j].depth)
					{
						max_depth = kBuffer[j].depth;
						id = j;
					}
				}

				// Move through the rest of the list
				[allow_uav_condition] for (i = 0; i < MAX_FRAGMENTS; i++)
				{
					if (pointer == NULLPOINTER)
					{
						break;
					}

					FragmentData e = SRV_fragmentData[pointer];

					// Check if the current fragment is nearer than the furthest from the PPLL
					if (max_depth > e.depth)
					{
						// Exchange
						float depthBackup = kBuffer[id].depth;
						uint tangentAndCoverageBackup = kBuffer[id].tangentAndCoverage;
						uint colorBackup = kBuffer[id].color;

						kBuffer[id].depth = e.depth;
						kBuffer[id].tangentAndCoverage = e.tangentAndCoverage;
						kBuffer[id].color = e.color;

						e.depth = depthBackup;
						e.tangentAndCoverage = tangentAndCoverageBackup;
						e.color = colorBackup;

						// Update furthest fragment in k-buffer
						max_depth = 0;
						[unroll]for (uint j = 0; j < KBUFFER_SIZE; j++)
						{
							if (max_depth < kBuffer[j].depth)
							{
								max_depth = kBuffer[j].depth;
								id = j;
							}
						}
					}

					// Unpack data
					float3 tangent = GetTangent(e.tangentAndCoverage);
					float coverage = GetCoverage(e.tangentAndCoverage);

					// Reconstruct world position
					float4 viewPos = float4(0, 0, 0, 0);
					float3 worldPos = ReconstructWorldPos(IN.screenPos.xy, e.depth, IN.ray, viewPos);
					float4 hairColor = UnpackUintIntoFloat4(e.color);

					// Light fragment
					// TODO: Do i really want to light those fragments?
					// -> Performance?
					fragmentColor = float4(hairColor.rgb, coverage * hairColor.a);

					// Blend the fragment color Out of order
					finalColor.xyz = mad(-finalColor.xyz, fragmentColor.w, finalColor.xyz) + fragmentColor.xyz * fragmentColor.w;
					finalColor.w = mad(-finalColor.w, fragmentColor.w, finalColor.w);

					pointer = e.next;
					fragmentCount++;
				}

				// Sort kbuffer
				uint kBufferFragments = min(fragmentCount, KBUFFER_SIZE);
				[unroll(KBUFFER_SIZE)]
				for (i = 0; i < KBUFFER_SIZE; i++)
				{
					if (i >= kBufferFragments)
						break;

					float max_depth = -1;
					int id = -1;
					// Find furthest fragment and blend in back-to-front order
					[unroll] for (uint j = 0; j < KBUFFER_SIZE; j++)
					{
						if (i >= kBufferFragments || kBuffer[j].depth == 1000000)
						{
							continue;
						}

						if (max_depth < kBuffer[j].depth)
						{
							max_depth = kBuffer[j].depth;
							id = j;
						}
					}

					// We ran out of fragments...
					if (id == -1)
					{
						break;
					}

					// Unpack data
					float3 tangent = GetTangent(kBuffer[id].tangentAndCoverage);
					float coverage = GetCoverage(kBuffer[id].tangentAndCoverage);

					// Reconstruct world position
					float4 viewPos = float4(0, 0, 0, 0);
					float3 worldPos = ReconstructWorldPos(IN.screenPos.xy, kBuffer[id].depth, IN.ray, viewPos);
					float4 hairColor = UnpackUintIntoFloat4(kBuffer[id].color);
					float3 eyeDir = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);

					// Light fragment
					fragmentColor = float4(0, 0, 0, coverage * hairColor.a);

					// Self-shadowing
					float selfShadow = 1;
					if (_SelfShadows > 0)
					{
						selfShadow = 1 - ((1 - ComputeShadow(worldPos, fragmentColor.a)) * _SelfShadowStrength);
					}

					//finalColor = float4(selfShadow, selfShadow, selfShadow, 0);

					// For each light
					// [unroll]
					for (int i = 0; i < MAX_LIGHTS; i++)
					{
						if (i >= _LightCount)
							break;

						// Get light data
						float4 lightPos = _LightPositions[i];
						float4 lightColor = _LightColors[i];

						// Shadowed?
						float4 lightData = _LightDatas[i];
						float shadow = selfShadow;
						if (lightData.x > -1)
						{
							int shadowMapIndex = (int)lightData.x;
							// Yes, shadowed light! 
							shadow *= SampleShadow(worldPos, shadowMapIndex);
						}

						// Additively blend the light data
						if (lightPos.w == 0)
						{
							// Directional lit
							fragmentColor += float4(KajiyaKay(shadow, lightPos.xyz, tangent, lightColor.rgb, eyeDir, hairColor.rgb).rgb, 0);
						}
					}

					// Blend the fragment color
					finalColor.xyz = mad(-finalColor.xyz, fragmentColor.w, finalColor.xyz) + fragmentColor.xyz * fragmentColor.a;
					finalColor.w = mad(-finalColor.w, fragmentColor.w, finalColor.w);

					// Take fragment out of the search
					kBuffer[id].depth = 1000000;
				}

#ifdef COLORDEBUG
				if (fragmentCount <= 16)
					return float4(0, 1, 0, 1); // Green
				else if (fragmentCount <= 32)
					return float4(1, 1, 0, 1); // Yellow
				else if (fragmentCount <= 64)
					return float4(1, 0, 0, 1); // Red
				else if (fragmentCount > 64)
					return float4(1, 1, 1, 1); // White
#endif
				return float4(finalColor.rgb, 1 - finalColor.a);
			}
			ENDCG
		}
	}
}

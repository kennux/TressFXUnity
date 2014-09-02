Shader "TressFX/TFXShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+100" }
        Pass
        {
			Tags {"LightMode" = "ForwardBase" } 
        	// ColorMask 0
        	ZWrite Off
        	ZTest LEqual
        	Cull Off
        	
			Stencil
			{
				Ref 1
				CompFront Always
				PassFront IncrSat
				FailFront Keep
				ZFailFront Keep
				CompBack Always
				PassBack IncrSat
				FailBack keep
				ZFailBack keep
			}
			
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            
            // Shader structs
            struct PS_INPUT_HAIR_AA
            {
				    float4 pos	: SV_POSITION;
				    float4 Tangent	: Tangent;
				    float4 p0p1		: TEXCOORD2;
				    float3 screenPos : TEXCOORD3;
					LIGHTING_COORDS(0,1)
			};
			
			//--------------------------------------------------------------------------------------
			// Per-Pixel Linked List (PPLL) structure
			//--------------------------------------------------------------------------------------
			struct PPLL_STRUCT
			{
			    uint	TangentAndCoverage;	
			    uint	depth;
			    uint    uNext;
			    uint	ammountLight;
			};
            
            // UAV's
			RWStructuredBuffer<struct PPLL_STRUCT>	LinkedListUAV;
            RWTexture2D<uint> LinkedListHeadUAV;
            
            // All needed buffers
            StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			StructuredBuffer<float> g_HairThicknessCoeffs;
			
			uniform float4 _HairColor;
			uniform float3 g_vEye;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;
			uniform matrix g_mInvViewProj;
			uniform matrix g_mInvViewProjViewport;
			uniform float g_FiberAlpha;
			uniform float g_alphaThreshold;
			uniform float4 g_MatKValue;
			uniform float g_fHairEx2;
			uniform float g_fHairKs2;
			
			// HELPER FUNCTIONS
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
			
			void StoreFragments_Hair(uint2 address, float3 tangent, float coverage, float depth, float ammountLight)
			{
			    // Retrieve current pixel count and increase counter
			    uint uPixelCount = LinkedListUAV.IncrementCounter();
			    uint uOldStartOffset;
			    
			    // uint address_i = ListIndex(address);
			    // Exchange indices in LinkedListHead texture corresponding to pixel location 
			    InterlockedExchange(LinkedListHeadUAV[address], uPixelCount, uOldStartOffset);  // link head texture

			    // Append new element at the end of the Fragment and Link Buffer
			    PPLL_STRUCT Element;
				Element.TangentAndCoverage = PackTangentAndCoverage(tangent, coverage);
				Element.depth = asuint(depth);
			    Element.uNext = uOldStartOffset;
			    Element.ammountLight = asuint(ammountLight);
			    LinkedListUAV[uPixelCount] = Element; // buffer that stores the fragments
			}
              
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            PS_INPUT_HAIR_AA vert (appdata_base input)
            {
            	uint vertexId = g_TriangleIndicesBuffer[(int)input.vertex.x];
			    
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 v = g_HairVertexPositions[index].xyz;

			    // Get hair strand thickness
			    float ratio = 1.0f; // ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(v - _WorldSpaceCameraPos) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_MVP, float4(right, 0) ).xy );

			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

				// Calculate the negative and positive offset screenspace positions
				float4 hairEdgePositions[2]; // 0 is negative, 1 is positive
				hairEdgePositions[0] = float4(v +  -1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[1] = float4(v +   1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[0] = mul(UNITY_MATRIX_MVP, hairEdgePositions[0]);
				hairEdgePositions[1] = mul(UNITY_MATRIX_MVP, hairEdgePositions[1]);
			    float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
				
				// P0P1 screen positions
				float4 p0screen = ComputeScreenPos(hairEdgePositions[0]);
				float4 p1screen = ComputeScreenPos(hairEdgePositions[1]);
				float4 screenPos = ComputeScreenPos((fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float4(proj_right * expandPixels / g_WinSize.y, 0.0f, 1.0f));
				screenPos.xy /= screenPos.w;
				
				hairEdgePositions[0] = hairEdgePositions[0]/hairEdgePositions[0].w;
				hairEdgePositions[1] = hairEdgePositions[1]/hairEdgePositions[1].w;

			    // Write output data
			    PS_INPUT_HAIR_AA Output = (PS_INPUT_HAIR_AA)0;
			    Output.pos = (fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float4(proj_right * expandPixels / g_WinSize.y, 0.0f, 0.0f);
			    Output.Tangent  = float4(t, ratio);
			    Output.p0p1     = float4( hairEdgePositions[0].xy, hairEdgePositions[1].xy );
			    Output.screenPos = float3(screenPos.xy, LinearEyeDepth(Output.pos.z));
			    
    			TRANSFER_VERTEX_TO_FRAGMENT(Output);
			    
			    return Output;
            }
			
			// A-Buffer pass
            [earlydepthstencil]
            float4 frag( PS_INPUT_HAIR_AA In) : SV_Target
			{
				float2 screenPos = In.screenPos.xy * g_WinSize.xy;
				float2 origScreenPos = screenPos;
				screenPos.y = g_WinSize.y - screenPos.y;
				
			     // Render AA Line, calculate pixel coverage
			    float4 proj_pos = float4(   2*screenPos.x*g_WinSize.z - 1.0,  // g_WinSize.z = 1.0/g_WinSize.x
			                                1.0 - 2*screenPos.y*g_WinSize.w,    // g_WinSize.w = 1.0/g_WinSize.y 
			                                1, 
			                                1);
				
				float coverage = ComputeCoverage(In.p0p1.xy, In.p0p1.zw, proj_pos.xy);
				
				// coverage *= g_FiberAlpha;

			    // only store fragments with non-zero alpha value
			    if (coverage > g_alphaThreshold) // ensure alpha is at least as much as the minimum alpha value
			    {
			        StoreFragments_Hair(screenPos, In.Tangent.xyz, coverage, In.screenPos.z, LIGHT_ATTENUATION(In));
			    }
			    
			    // output a mask RT for final pass    
			    return float4(In.screenPos.z, In.screenPos.z, In.screenPos.z, 1);
			}
            
            ENDCG
        }
		
		// Pass to render object as a shadow collector
	    Pass
	    {
	        Name "ShadowCollector"
	        Tags { "LightMode" = "ShadowCollector" }
	 
	        Fog {Mode Off}
			ZWrite On ZTest LEqual
			
	        CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        #pragma multi_compile_shadowcollector
			#pragma target 5.0

	        #define SHADOW_COLLECTOR_PASS
	        #include "UnityCG.cginc"
			
			StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			StructuredBuffer<float> g_HairThicknessCoeffs;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;

	        struct v2f {
	            V2F_SHADOW_COLLECTOR;
	        };

        	// --------------------------------------
        	// TressFX Antialias shader written by AMD
        	// 
        	// Modified by KennuX
        	// --------------------------------------
	        v2f vert (appdata_base v)
	        { 
	            v2f o;
	            
	        	// Access the current line segment
				uint vertexId = g_TriangleIndicesBuffer[(int)v.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
				
			    // Get updated positions and tangents from simulation result
			    float3 vert = g_HairVertexPositions[index].xyz;
			    float3 t = g_HairVertexTangents[index].xyz;
			    fixed ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    fixed3 right      = normalize( cross( t, normalize(vert - _WorldSpaceCameraPos) ) );
			    
			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    fixed expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;
			    
			    // Which direction to expand?
			    fixed fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    
			    // Calculate the edge position
			    v.vertex = float4(vert + fDirIndex * right * ratio * g_FiberRadius, 1.0);
	            
	            TRANSFER_SHADOW_COLLECTOR(o)
	            return o;
	        }

	        half4 frag (v2f i) : COLOR
	        {
	            SHADOW_COLLECTOR_FRAGMENT(i)
	        }
	        ENDCG
	    }
	}
}

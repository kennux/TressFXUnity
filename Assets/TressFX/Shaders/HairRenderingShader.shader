Shader "TressFX/Hair Rendering Shader"
{
    SubShader
    {
        /*Pass
        {
    		Tags { "LightMode" = "ForwardBase" }
        	Blend SrcAlpha OneMinusSrcAlpha // turn on alpha blending
        	ZWrite On
        	Cull Off
        	
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            #pragma multi_compile_fwdbase
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            
            #include "UnityCG.cginc"
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> _VertexPositionBuffer;
            StructuredBuffer<int> _StrandIndicesBuffer;
            uniform float4 _HairColor;
            uniform float _HairThickness;
            uniform float4 _CameraDirection;
 
            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                int vertexIndex : COLOR0;
            };
            
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                ps_input o;
                
                // Position transformation
                o.pos = mul (UNITY_MATRIX_VP, float4(_VertexPositionBuffer[id],1.0f));
                o.vertexIndex = id;
                
                return o;
            }

			[maxvertexcount(2)]
			void geom (line ps_input input[2], inout LineStream<ps_input> outStream)
			{
				outStream.Append(input[0]);
				if (_StrandIndicesBuffer[input[0].vertexIndex+1] == 0)
				{
					outStream.RestartStrip();
				}
				outStream.Append(input[1]);
			}
 
            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return _HairColor;
            }
 
            ENDCG
 
        }*/
        // A-Buffer fill pass
        Pass
        {
        	ColorMask 0
        	ZWrite On
        	ZTest LEqual
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
			Blend SrcColor One
			Blend DstColor Zero
			
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "TressFXInclude.cginc"
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            PS_INPUT_HAIR_AA vert (uint id : SV_VertexID)
            {
            	uint vertexId = g_TriangleIndicesBuffer[id];
			    
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 v = g_HairVertexPositions[index].xyz;

			    // Get hair strand thickness
			    float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(v - g_vEye) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_VP, float4(right, 0) ).xy );

			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

				// Calculate the negative and positive offset screenspace positions
				float4 hairEdgePositions[2]; // 0 is negative, 1 is positive
				hairEdgePositions[0] = float4(v +  -1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[1] = float4(v +   1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[0] = mul(UNITY_MATRIX_VP, hairEdgePositions[0]);
				hairEdgePositions[1] = mul(UNITY_MATRIX_VP, hairEdgePositions[1]);
				hairEdgePositions[0] = hairEdgePositions[0]/hairEdgePositions[0].w;
				hairEdgePositions[1] = hairEdgePositions[1]/hairEdgePositions[1].w;

			    // Write output data
			    PS_INPUT_HAIR_AA Output = (PS_INPUT_HAIR_AA)0;
			    float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    Output.Position = (fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float4(proj_right * expandPixels / g_WinSize.y, 0.0f, 0.0f);
			    Output.Tangent  = float4(t, ratio);
			    Output.p0p1     = float4( hairEdgePositions[0].xy, hairEdgePositions[1].xy );

			    return Output;
            }
            
            // A-Buffer pass
            float4 frag( PS_INPUT_HAIR_AA In) : SV_Target
			{ 
			    // Render AA Line, calculate pixel coverage
			    float4 proj_pos = float4(   2*In.Position.x*g_WinSize.z - 1.0,  // g_WinSize.z = 1.0/g_WinSize.x
			                                1 - 2*In.Position.y*g_WinSize.w,    // g_WinSize.w = 1.0/g_WinSize.y 
			                                1, 
			                                1);

			    float4 original_pos = mul(proj_pos, g_mInvViewProj);
			    
			    float curve_scale = 1;
			    if (g_bThinTip > 0 )
			        curve_scale = In.Tangent.w;
			    
			    float fiber_radius = curve_scale * g_FiberRadius;
				
				float coverage = 1.f;
				if(true)
				{	
			        coverage = ComputeCoverage(In.p0p1.xy, In.p0p1.zw, proj_pos.xy);
				}

				coverage *= g_FiberAlpha;

			    // only store fragments with non-zero alpha value
			    if (coverage > g_alphaThreshold) // ensure alpha is at least as much as the minimum alpha value
			    {
			        StoreFragments_Hair(In.Position.xy, In.Tangent.xyz, coverage, In.Position.z);
			    }
			    // output a mask RT for final pass    
			    return float4(1, 0, 0, 0);
			}
            
            ENDCG
        }
        
        // K-Buffer and Draw pass
        Pass
        {
        	Tags { "Queue" = "Transparent" }
        	// Z-Buffer and Stencil
        	ZWrite off
        	ZTest LEqual
			Stencil
			{
				Ref 1
				CompFront LEqual
				PassFront Zero
				FailFront keep
				ZFailFront keep
				CompBack LEqual
				PassBack Zero
				FailBack keep
				ZFailBack keep
			}
			
			Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
			
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #include "UnityCG.cginc"
            #include "TressFXInclude.cginc"
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
            
			#define KBUFFER_SIZE 8
			#define NULLPOINTER 0xFFFFFFFF
			#define g_iMaxFragments 768
			#define COLORDEBUG 1
            
            VS_OUTPUT_SCREENQUAD vert (VS_INPUT_SCREENQUAD input)
            {
			    VS_OUTPUT_SCREENQUAD output = (VS_OUTPUT_SCREENQUAD)0;

			    output.vPosition = float4(input.Position.xyz, 1.0);
			    output.vTex = input.Texcoord.xy;

			    return output;
            }
            
            float4 frag( VS_OUTPUT_SCREENQUAD In) : SV_Target
            {
            	float4 fcolor = float4(0,0,0,1);
			    float amountLight;
			    float lightIntensity;
				float4 fragmentColor = float4(0,0,0,0);
				float4 vWorldPosition = float4(0,0,0,0);
				float3 vTangent = float3(0,0,0);
				float coverage;
				uint tangentAndCoverage;

				// get the start of the linked list from the head pointer
				uint pointer = LinkedListHeadUAV[In.vPosition.xy];

			    // A local Array to store the top k fragments(depth and color), where k = KBUFFER_SIZE
			#ifdef ALU_INDEXING

			    uint4 kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215;
			    uint4 kBufferPackedTangentV03, kBufferPackedTangentV47, kBufferPackedTangentV811, kBufferPackedTangentV1215;
			    kBufferDepthV03 = uint4(asuint(100000.0f), asuint(100000.0f), asuint(100000.0f), asuint(100000.0f)); 
			    kBufferDepthV47 = uint4(asuint(100000.0f), asuint(100000.0f), asuint(100000.0f), asuint(100000.0f)); 
			    kBufferDepthV811 = uint4(asuint(100000.0f), asuint(100000.0f), asuint(100000.0f), asuint(100000.0f)); 
			    kBufferDepthV1215 = uint4(asuint(100000.0f), asuint(100000.0f), asuint(100000.0f), asuint(100000.0f)); 
			    kBufferPackedTangentV03 = uint4(0,0,0,0);
			    kBufferPackedTangentV47 = uint4(0,0,0,0);
			    kBufferPackedTangentV811 = uint4(0,0,0,0);
			    kBufferPackedTangentV1215 = uint4(0,0,0,0);

			    // Get the first k elements in the linked list
			    int nNumFragments = 0;
			    for(int p=0; p<KBUFFER_SIZE; p++)
			    {
			        if (pointer != NULLPOINTER)
			        {
			            StoreUintAtIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, p, LinkedListUAV[pointer].depth);
			            StoreUintAtIndex_Size16(kBufferPackedTangentV03, kBufferPackedTangentV47, kBufferPackedTangentV811, kBufferPackedTangentV1215, p, LinkedListUAV[pointer].TangentAndCoverage);
			            pointer = LinkedListUAV[pointer].uNext;
			#ifdef COLORDEBUG
			            nNumFragments++;
			#endif
			        }
			    }

			#else

			    KBuffer_STRUCT kBuffer[KBUFFER_SIZE];

				[unroll]for(int t=0; t<KBUFFER_SIZE; t++)
				{
			        kBuffer[t].depthAndPackedtangent.x = asuint(100000.0f);	// must be larger than the maximum possible depth value
			        kBuffer[t].depthAndPackedtangent.y = 0;
				}

			    // Get the first k elements in the linked list
			    int nNumFragments = 0;
			    for(int p=0; p<KBUFFER_SIZE; p++)
			    {
			        if (pointer != NULLPOINTER)
			        {
			            kBuffer[p].depthAndPackedtangent.x	= LinkedListUAV[pointer].depth;
			            kBuffer[p].depthAndPackedtangent.y	= LinkedListUAV[pointer].TangentAndCoverage;
			            pointer								= LinkedListUAV[pointer].uNext;
			#ifdef COLORDEBUG
			            nNumFragments++;
			#endif
			        }
			    }

			#endif
				
			    // Go through the rest of the linked list, and keep the closest k fragments, but not in sorted order.
			    [allow_uav_condition]
			    for(int l=0; l < g_iMaxFragments; l++)
			    {
			        if(pointer == NULLPOINTER)	break;

			#ifdef COLORDEBUG
			        nNumFragments++;
			#endif

			        int id = 0;
			        float max_depth = 0;

					// find the furthest node in array
			        [unroll]for(int i=0; i<KBUFFER_SIZE; i++)
			        {	
			#ifdef ALU_INDEXING
			            float fDepth = asfloat(GetUintFromIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, i));
			#else
						float fDepth = asfloat(kBuffer[i].depthAndPackedtangent.x);
			#endif
			            if(max_depth < fDepth)
			            {
			                max_depth = fDepth;
			                id = i;
			            }
			        }

			        uint nodePackedTangent = LinkedListUAV[pointer].TangentAndCoverage;
					uint nodeDepth         = LinkedListUAV[pointer].depth;
					float fNodeDepth       = asfloat(nodeDepth);

			        // If the node in the linked list is nearer than the furthest one in the local array, exchange the node 
			        // in the local array for the one in the linked list.
			        if (max_depth > fNodeDepth)
			        {
			#ifdef ALU_INDEXING
			            uint tmp								= GetUintFromIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, id);
			            StoreUintAtIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811,  kBufferDepthV1215, id, nodeDepth);
			            fNodeDepth								= asfloat(tmp);
			            tmp										= GetUintFromIndex_Size16(kBufferPackedTangentV03, kBufferPackedTangentV47, kBufferPackedTangentV811, kBufferPackedTangentV1215, id);
			            StoreUintAtIndex_Size16(kBufferPackedTangentV03, kBufferPackedTangentV47, kBufferPackedTangentV811, kBufferPackedTangentV1215, id, nodePackedTangent);
						nodePackedTangent						= tmp;
			#else
			            uint tmp								= kBuffer[id].depthAndPackedtangent.x;
			            kBuffer[id].depthAndPackedtangent.x	= nodeDepth;
			            fNodeDepth								= asfloat(tmp);
			            tmp										= kBuffer[id].depthAndPackedtangent.y;
			            kBuffer[id].depthAndPackedtangent.y	= nodePackedTangent;
						nodePackedTangent						= tmp;
			#endif
			        }

			        // Do simple shading and out of order blending for nodes that are not part of the k closest fragments
			        vWorldPosition = mul(float4(In.vPosition.xy, fNodeDepth, 1), g_mInvViewProj);
					vWorldPosition.xyz /= vWorldPosition.www;

			/* #ifdef SIMPLESHADOWING
					amountLight = ComputeSimpleShadow(vWorldPosition.xyz, g_HairShadowAlpha, g_iTechSM);
			#else
			        amountLight = ComputeShadow(vWorldPosition.xyz, g_HairShadowAlpha, g_iTechSM);
			#endif */

			        fragmentColor.w = GetCoverage(nodePackedTangent);
			        vTangent = GetTangent(nodePackedTangent);
					
			/* #ifdef SIMPLESHADING
			        fragmentColor.xyz = SimpleHairShading( vWorldPosition.xyz, vTangent, float4(0,0,0,0), amountLight);
			#else
					fragmentColor.xyz = ComputeHairShading( vWorldPosition.xyz, vTangent, float4(0,0,0,0), amountLight);
			#endif */
			        
			        // Blend the fragment color
			        fcolor.xyz = mad(-fcolor.xyz, fragmentColor.w, fcolor.xyz) + fragmentColor.xyz * fragmentColor.w;
					fcolor.w = mad(-fcolor.w, fragmentColor.w, fcolor.w);

			        // Retrieve next node pointer
			        pointer = LinkedListUAV[pointer].uNext;
			    }


			    // Blend the k nearest layers of fragments from back to front, where k = KBUFFER_SIZE
			    for(int j=0; j<KBUFFER_SIZE; j++)
			    {
			        int id = 0;
			        float max_depth = 0;
					float initialized = 1;

					// find the furthest node in the array
			        for(int i=0; i<KBUFFER_SIZE; i++)
			        {
			#ifdef ALU_INDEXING
			            float fDepth = asfloat(GetUintFromIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, i));
			#else
						float fDepth = asfloat(kBuffer[i].depthAndPackedtangent.x);
			#endif
			            if(max_depth < fDepth)
			            {
			                max_depth = fDepth;
			                id = i;
			            }
			        }


			        // take this node out of the next search
			#ifdef ALU_INDEXING
			        uint nodePackedTangent = GetUintFromIndex_Size16(kBufferPackedTangentV03, kBufferPackedTangentV47, kBufferPackedTangentV811, kBufferPackedTangentV1215, id);
			        uint nodeDepth         = GetUintFromIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, id);

			        StoreUintAtIndex_Size16(kBufferDepthV03, kBufferDepthV47, kBufferDepthV811, kBufferDepthV1215, id, 0);
			#else
					uint nodePackedTangent = kBuffer[id].depthAndPackedtangent.y;
			        uint nodeDepth         = kBuffer[id].depthAndPackedtangent.x;

					// take this node out of the next search
			        kBuffer[id].depthAndPackedtangent.x = 0;
			#endif

					// Use high quality shading for the nearest k fragments
					float fDepth = asfloat(nodeDepth);
			        vWorldPosition = mul(float4(In.vPosition.xy, fDepth, 1), g_mInvViewProj);
					vWorldPosition.xyz /= vWorldPosition.www;

					amountLight = 1; // ComputeShadow(vWorldPosition.xyz, g_HairShadowAlpha, g_iTechSM); 

			        // Get tangent and coverage
			        vTangent        = GetTangent(nodePackedTangent);
			        fragmentColor.w = GetCoverage(nodePackedTangent);

			        // Shading
					fragmentColor.xyz = ComputeHairShading( vWorldPosition.xyz, vTangent, float4(0,0,0,0), amountLight);

					// Blend the fragment color
			        fcolor.xyz = mad(-fcolor.xyz, fragmentColor.w, fcolor.xyz) + fragmentColor.xyz * fragmentColor.w;
					fcolor.w = mad(-fcolor.w, fragmentColor.w, fcolor.w);
			    }


			#ifdef COLORDEBUG
			    fcolor.xyz = float3(0,1,0);
			    if (nNumFragments>32) fcolor.xyz = float3(1,1,0);
			    if (nNumFragments>64) fcolor.xyz = float3(1,0.5,0);
			    if (nNumFragments>128) fcolor.xyz = float3(1,0,0);
			#endif

			    return fcolor;
            }
            
            ENDCG
        }
    }
 
    Fallback Off
}
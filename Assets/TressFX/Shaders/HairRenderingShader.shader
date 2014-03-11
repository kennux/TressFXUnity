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
				WriteMask 0
				CompFront LEqual
				PassFront keep
				FailFront keep
				ZFailFront keep
				CompBack LEqual
				PassBack keep
				FailBack keep
				ZFailBack keep
			}
			
			// Blend state
			/*Blend SrcColor One
			Blend DstColor SrcAlpha
			BlendOp Add
			Blend SrcAlpha Zero
			Blend DstAlpha Zero
			BlendOp Add*/
			
			// Cull Off
			
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #include "UnityCG.cginc"
            #include "TressFXInclude.cginc"
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert_img
            #pragma fragment frag
            
            
            float4 frag( PS_INPUT_HAIR_AA In) : SV_Target
            {
            	return _HairColor;
            }
            
            ENDCG
        }
    }
 
    Fallback Off
}
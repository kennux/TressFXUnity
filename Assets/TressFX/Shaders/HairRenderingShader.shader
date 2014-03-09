Shader "TressFX/Hair Rendering Shader"
{
    SubShader
    {
        Pass
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
 
        }
        // A-Buffer fill pass
        Pass
        {
    		Tags { "LightMode" = "ForwardBase" }
        	
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> g_HairVertexTangents;
            StructuredBuffer<float3> g_HairVertexPositions;
            StructuredBuffer<int> g_TriangleIndicesBuffer;
            StructuredBuffer<float> g_HairThicknessCoeffs;
            uniform float4 _HairColor;
            uniform float3 g_vEye;
            uniform float4 g_WinSize;
 
            //A simple input struct for our pixel shader step containing a position.
            struct PS_INPUT_HAIR_AA {
			    float4 Position	: SV_POSITION;
			    float4 Tangent	: Tangent;
			    float4 p0p1		: TEXCOORD0;
            };
            
 
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
			    float ratio = 1.0; // ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(v - g_vEye) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_VP, float4(right, 0) ).xy );

			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = 0.5; // (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

				// Calculate the negative and positive offset screenspace positions
				float g_FiberRadius = 0.18;
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
 
            //Pixel function returns a solid color for each point.
            float4 frag (PS_INPUT_HAIR_AA i) : COLOR
            {
                return _HairColor;
            }
            
            ENDCG
        }
    }
 
    Fallback Off
}
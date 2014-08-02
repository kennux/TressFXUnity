Shader "TressFX/HairSurfaceShader" {
	Properties
	{
		  _HairColor ("Hair Color", Color) = (0,0,0,1)
		  _SpecColor ("Specular Color", Color) = (0,0,0,1)
		  _Shininess ("Shininess", Range (0, 1)) = 0.5
		  _Gloss ("Gloss", Range (0, 1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 5.0

#ifdef SHADER_API_D3D11
		StructuredBuffer<float3> g_HairVertexTangents;
		StructuredBuffer<float3> g_HairVertexPositions;
		StructuredBuffer<int> g_TriangleIndicesBuffer;
#endif

		uniform float3 g_vEye;
		uniform float4 g_WinSize;
		uniform float g_FiberRadius;
		uniform float g_bExpandPixels;
		uniform fixed4 _HairColor;
		uniform fixed _Shininess;
		uniform fixed _Gloss;
			
		void vert (inout appdata_full data)
		{
            #ifdef SHADER_API_D3D11
			uint vertexId = g_TriangleIndicesBuffer[(int)data.vertex.x];
			
		    // Access the current line segment
		    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

		    // Get updated positions and tangents from simulation result
		    float3 t = g_HairVertexTangents[index].xyz;
		    float3 vert = g_HairVertexPositions[index].xyz;
		    float ratio = 1.0f; // ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

		    // Calculate right and projected right vectors
		    float3 right      = normalize( cross( t, normalize(vert - g_vEye) ) );
		    float2 proj_right = normalize( mul( UNITY_MATRIX_VP, float4(right, 0) ).xy );
		    
		    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
		    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

			// Calculate the negative and positive offset screenspace positions
			float4 hairEdgePositions[2]; // 0 is negative, 1 is positive
			float4 hairEdgePositionsNormal[2]; // 0 is negative, 1 is positive
			hairEdgePositions[0] = float4(vert +  -1.0 * right * ratio * g_FiberRadius, 1.0);
			hairEdgePositions[1] = float4(vert +   1.0 * right * ratio * g_FiberRadius, 1.0);
			hairEdgePositionsNormal[0] = hairEdgePositions[0];
			hairEdgePositionsNormal[1] = hairEdgePositions[1];
			hairEdgePositions[0] = mul(UNITY_MATRIX_MVP, hairEdgePositions[0]);
			hairEdgePositions[1] = mul(UNITY_MATRIX_MVP, hairEdgePositions[1]);
			hairEdgePositions[0] = hairEdgePositions[0]/hairEdgePositions[0].w;
			hairEdgePositions[1] = hairEdgePositions[1]/hairEdgePositions[1].w;
			
		    // Write output data
		    float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
		    float3 pos = (fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float3(proj_right * expandPixels / g_WinSize.y, 0.0f);
		    
		    float3 posi = (fDirIndex==-1.0 ? hairEdgePositionsNormal[0] : hairEdgePositionsNormal[1]) + fDirIndex * float3(proj_right * expandPixels / g_WinSize.y, 0.0f);
		    
		    data.vertex =float4(posi, 1);
		    data.normal = normalize(posi);
		    
			/*o.pos = float4(pos, 1);
			o.normal = normalize(o.pos);
			
			o.lightDir = ObjSpaceLightDir( float4(input.vertex.xyz, 1) );
			o.viewDir = WorldSpaceViewDir( float4(input.vertex.xyz, 1) );*/
			#endif
        }

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = _HairColor;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Gloss = _Gloss;
			o.Specular = _Shininess;
		}
		ENDCG
	}
	Fallback "TressFX/HairShader"
}

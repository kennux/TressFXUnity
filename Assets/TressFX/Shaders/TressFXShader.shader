Shader "TressFX/Hair Rendering Shader"
{
    SubShader
    {
        Pass
        {
        	Blend SrcAlpha OneMinusSrcAlpha // turn on alpha blending
        	ZWrite On
        	
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geom
 
            #include "UnityCG.cginc"
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> g_HairVertexTangents;
            StructuredBuffer<int> _StrandIndicesBuffer;
            uniform float4 _HairColor;
            
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                ps_input o;
                
                // Position transformation
                o.pos = mul (UNITY_MATRIX_VP, float4(_VertexPositionBuffer[id],1.0f));
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                
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
                return _HairColor * LIGHT_ATTENUATION(i);
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
}
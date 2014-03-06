Shader "TressFX/Hair Rendering Shader"
{
    SubShader
    {
        Pass
        {
        	Blend SrcAlpha OneMinusSrcAlpha // turn on alpha blending
        
            CGPROGRAM
            #pragma target 5.0
            #pragma multi_compile_fwdbase
 
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geom
 
            #include "UnityCG.cginc"

			struct StrandIndex
			{
				int vertexInStrandId;
				int hairId;
				int vertexCountInStrand;
			};
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> _VertexPositionBuffer;
            StructuredBuffer<int> _StrandIndicesBuffer;
            uniform float4 _HairColor;
 
            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                int vertexIndex : TEXCOORD;
            };
            
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                ps_input o;
                
                // Position transformation
                float3 worldPos = _VertexPositionBuffer[id];
                o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
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
    }
 
    Fallback Off
}
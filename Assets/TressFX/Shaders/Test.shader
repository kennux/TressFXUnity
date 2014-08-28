Shader "Custom/Test" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
			
			CGPROGRAM
			#pragma target 5.0
		    
		    #pragma exclude_renderers gles

		    #pragma vertex vert
		    #pragma fragment frag
		    
		    #include "UnityCG.cginc"
		    
		    StructuredBuffer<float4> _VertexPositionBuffer;

			//A simple input struct for our pixel shader step containing a position.
		    struct ps_input {
		        float4 pos : SV_POSITION;
		    };
		    

		    //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
		    //which we transform with the view-projection matrix before passing to the pixel program.
		    ps_input vert (uint id : SV_VertexID)
		    {
		        ps_input o = (ps_input)0;
		        
		        // Position transformation
		        o.pos = mul (UNITY_MATRIX_VP, float4(_VertexPositionBuffer[id].xyz,1.0f));
		        
		        return o;
		    }
		    
		    //Pixel function returns a solid color for each point.
		    fixed4 frag (ps_input i) : COLOR
		    {
		        return fixed4(1,0,0,1);
		    }
		    
			ENDCG
		}
	} 
	FallBack "Diffuse"
}

Shader "Custom/FSQuad" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
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
            
            #pragma exclude_renderers gles
 
            #pragma vertex vert
            #pragma fragment frag
			
			uniform sampler2D _TestTex;
			
            struct VS_OUTPUT_SCREENQUAD
			{
			    float4 vPosition : SV_POSITION;
			    float2 vTex      : TEXCOORD;
			};
			
			struct VS_INPUT_SCREENQUAD
			{
			    float3 Position     : POSITION;		// vertex position 
			    float3 Normal       : NORMAL;		// this normal comes in per-vertex
			    float2 Texcoord	    : TEXCOORD;	// vertex texture coords 
			};

            VS_OUTPUT_SCREENQUAD vert (VS_INPUT_SCREENQUAD input)
            {
			    VS_OUTPUT_SCREENQUAD output = (VS_OUTPUT_SCREENQUAD)0;

			    output.vPosition = float4(input.Position.xyz, 1.0);
			    output.vTex = input.Texcoord.xy;

			    return output;
            }
            
            float4 frag( VS_OUTPUT_SCREENQUAD In) : SV_Target
            {
            	float4 c = tex2D(_TestTex, In.vTex);
            	c.a = 1;
            	return c;
            }
            
            ENDCG
        }
	} 
	FallBack "Diffuse"
}

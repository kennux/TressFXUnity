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
			
			// Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
			
            CGPROGRAM
            #pragma debug
            #pragma target 5.0
            
            #include "UnityCG.cginc"
            
            #pragma exclude_renderers gles
 
			#define NULLPOINTER 0xffffffff
			#define MAX_FRAGMENTS 256
			#define KBUFFER_SIZE 8
			
            #pragma vertex vert
            #pragma fragment frag
            
			uniform sampler2D _TestTex;
			
            struct VS_OUTPUT_SCREENQUAD
			{
			    float4 vPosition : SV_POSITION;
			    float2 vTex      : TEXCOORD0;
			};
			
			struct VS_INPUT_SCREENQUAD
			{
			    float3 Position     : POSITION;		// vertex position 
			    float3 Normal       : NORMAL;		// this normal comes in per-vertex
			    float2 Texcoord	    : TEXCOORD;	// vertex texture coords 
			};
<<<<<<< HEAD
			
			float4 UnpackUintIntoFloat4(uint uValue)
			{
			    return float4( ( (uValue & 0xFF000000)>>24 ) / 255.0, ( (uValue & 0x00FF0000)>>16 ) / 255.0, ( (uValue & 0x0000FF00)>>8 ) / 255.0, ( (uValue & 0x000000FF) ) / 255.0);
			}
			
			uint PackFloat4IntoUint(float4 vValue)
			{
			    return ( (uint(vValue.x*255)& 0xFFUL) << 24 ) | ( (uint(vValue.y*255)& 0xFFUL) << 16 ) | ( (uint(vValue.z*255)& 0xFFUL) << 8) | (uint(vValue.w * 255)& 0xFFUL);
			}
=======
>>>>>>> parent of e26fc89... Update. Fragment lighting test works :3

            VS_OUTPUT_SCREENQUAD vert (VS_INPUT_SCREENQUAD input)
            {
			    VS_OUTPUT_SCREENQUAD output = (VS_OUTPUT_SCREENQUAD)0;

			    output.vPosition = mul(UNITY_MATRIX_VP, float4(input.Position.xyz, 1.0));
			    output.vTex = input.Texcoord.xy;

			    return output;
            }
            
            float4 frag( VS_OUTPUT_SCREENQUAD In) : SV_Target
            {
<<<<<<< HEAD
		       	return tex2D(_TestTex, In.vTex);
=======
            	float4 c = tex2D(_TestTex, In.vTex);
            	c.a = 1;
            	return c;
>>>>>>> parent of e26fc89... Update. Fragment lighting test works :3
            }
            
            ENDCG
        }
	} 
	FallBack "Diffuse"
}

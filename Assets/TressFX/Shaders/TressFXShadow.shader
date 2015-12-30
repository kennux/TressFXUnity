Shader "TressFX/HairShadowShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue"="Transparent" }
		
		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" "Queue" = "Geometry" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
	            
			#include "UnityCG.cginc"
			
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<int> g_LineIndices;

			struct v2f
			{ 
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base input) // uint vertexId : SV_VertexID) // 
			{
				float3 vertexPosition = g_HairVertexPositions[(int)input.vertex.x];
	            
	            //int index = g_LineIndices[vertexId];
	            //float3 vertexPosition = g_HairVertexPositions[index];
	            
		        appdata_base v;
		        v.vertex = float4(vertexPosition.xyz, 1);
	            
	            v2f o;
	            
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag( v2f i ) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	} 
}
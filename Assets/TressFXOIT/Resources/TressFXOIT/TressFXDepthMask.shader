Shader "Hidden/TressFX/DepthMask"
{
	Properties
	{
	}
	SubShader
	{
		ZWrite On
		ZTest LEqual
		Cull Back
		ColorMask 0
		Offset 1, 0
		
		// Line topology pass
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "../../TressFX_Common.cginc"
			
			struct v2f_simple
			{
				float4 pos : SV_POSITION;
			};

			v2f_simple vert (uint vertexId : SV_VertexID)
			{
				v2f_simple o;
				o.pos = mul(UNITY_MATRIX_MVP, float4(GetVertexPosition(LineIndicesBuffer[vertexId]), 1));
				o.pos = UnityApplyLinearShadowBias(o.pos);
				return o;
			}
			
			float frag (v2f_simple i) : SV_Target
			{
				return Linear01Depth(i.pos.z / i.pos.w);
			}
			ENDCG
		}
		
		// Triangle topology pass
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "../../TressFX_Common.cginc"

			struct v2f_simple
			{
				float4 pos : SV_POSITION;
			};

			v2f_simple vert (uint indexIndex : SV_VertexID)
			{
				// Init output
				v2f_simple o;
				UNITY_INITIALIZE_OUTPUT(v2f_simple, o);
				
				uint vertexId = TriangleIndicesBuffer[indexIndex];

				// Access the current line segment
				uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
				float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;

				// Get updated positions and tangents from simulation result
				float3 t = HairVertexTangents[index].xyz;
				float3 vert = GetVertexPosition(index);
				float ratio = 1.0; // (_ThinTip > 0) ? HairThicknessCoeffs[index] : 1.0;
				
				// Calculate right vector
				float3 right = normalize( cross( t, normalize(vert - _WorldSpaceCameraPos) ) );
				float3 left = normalize(cross (t, float3(0,1,0)));
				
				// Set output data
				o.pos = mul(UNITY_MATRIX_MVP, float4(vert + ((right * (GetHairWidth() * ratio))  * fDirIndex), 1));
				o.pos = UnityApplyLinearShadowBias(o.pos);

				return o;
			}
			
			half4 frag (v2f_simple i) : SV_Target
			{
				return Linear01Depth(i.pos.z / i.pos.w);
			}
			ENDCG
		}
	}
}
